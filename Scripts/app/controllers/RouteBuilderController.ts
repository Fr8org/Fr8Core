/// <reference path="../_all.ts" />

/*
    Detail (view/add/edit) controller
*/

module dockyard.controllers {
    'use strict';

    // This is a fix for incomplit ts defenition for angular-ui module
    interface ngState extends ng.ui.IState {
        current: ng.ui.IState;
    }

    export interface IRouteBuilderScope extends ng.IScope {
        routeId: string;
        subroutes: Array<model.SubrouteDTO>;
        fields: Array<model.Field>;
        currentSubroute: model.SubrouteDTO;

        // Identity of currently edited processNodeTemplate.
        //curNodeId: number;
        //// Flag, that indicates if currently edited processNodeTemplate has temporary identity.
        //curNodeIsTempId: boolean;
        current: model.RouteBuilderState;
        actionGroups: model.ActionGroup[];

        addAction(group: model.ActionGroup): void;
        deleteAction: (action: model.ActionDTO) => void;
        selectAction(action): void;
        isBusy: () => boolean;
        onActionDrop: (group: model.ActionGroup, actionId: string, index: number) => void;
        mode: string;
        solutionName: string;
    }

    //Setup aliases
    import pwd = dockyard.directives.paneWorkflowDesigner;
    import pca = dockyard.directives.paneConfigureAction;
    import psa = dockyard.directives.paneSelectAction;

    class RouteBuilderController {
        // $inject annotation.
        // It provides $injector with information about dependencies to be injected into constructor
        // it is better to have it close to the constructor, because the parameters must match in count and type.
        // See http://docs.angularjs.org/guide/di


        public static $inject = [
            '$scope',
            'LocalIdentityGenerator',
            '$state',
            'ActionService',
            '$http',
            'RouteService',
            '$timeout',
            'RouteBuilderService',
            'CrateHelper',
            '$filter',
            'UIHelperService',
            'LayoutService',
            '$state'
        ];

        private _longRunningActionsCounter: number;

        constructor(
            private $scope: IRouteBuilderScope,
            private LocalIdentityGenerator: services.ILocalIdentityGenerator,
            private $state: ngState,
            private ActionService: services.IActionService,
            private $http: ng.IHttpService,
            private RouteService: services.IRouteService,
            private $timeout: ng.ITimeoutService,
            private RouteBuilderService: services.IRouteBuilderService,
            private CrateHelper: services.CrateHelper,
            private $filter: ng.IFilterService,
            private uiHelperService: services.IUIHelperService,
            private LayoutService: services.ILayoutService
            ) {

            this.$scope.current = new model.RouteBuilderState();
            this.$scope.actionGroups = [];

            this.setupMessageProcessing();

            this.$scope.addAction = (group: model.ActionGroup) => {
                this.addAction(group);
            }

            this.$scope.isBusy =  () => {
                return this._longRunningActionsCounter > 0;
            };

            this._longRunningActionsCounter = 0;

            $scope.deleteAction = <() => void> angular.bind(this, this.deleteAction);

            this.$scope.selectAction = (action: model.ActionDTO) => {
                if (!this.$scope.current.action || this.$scope.current.action.id !== action.id)
                    this.selectAction(action, null);
            }

            //Group: which group action is dropped to
            //actionId: id of dropped action
            //dropped index
            $scope.onActionDrop = (group: model.ActionGroup, actionId: string, index: number) => {

                var realAction = this.findActionById(actionId);
                if (realAction === null) {
                    return;
                }
                
                //let's remove this action from it's old parent
                var downstreamActions: model.ActionDTO[] = this.findAndRemoveAction(realAction);

                //TODO check parent action change with a more solid method
                //this action is moved to a different parent
                if (realAction.parentRouteNodeId !== group.actions[0].parentRouteNodeId) {
                    //set new parent
                    realAction.parentRouteNodeId = group.actions[0].parentRouteNodeId;
                } else {
                    //this action is moved to same parent
                    //our index calculation might have been wrong
                    //while dragging an action we don't delete that action
                    //we just make it hidden - so it calculates dragged action too while calculating index
                    if (realAction.ordering <= index) {
                        index -= 1;
                    }
                }

                //now we should inject it to proper position and get downstream actions
                downstreamActions = downstreamActions.concat(this.insertActionToParent(realAction, index));

                //let's add our current action to configure list
                downstreamActions.push(realAction);

                //let's re-render route builder
                this.renderRoute(<interfaces.IRouteVM>this.$scope.current.route);

                //if this action is dragged to same parent as it was before
                //there might be duplicate actions in our downstreamactions array
                //let's eliminate them
                var uniqueDownstreamActions = _.uniq(downstreamActions, (action: model.ActionDTO) => action.id);
                
                //let's wait for UI to finish it's rendering
                this.$timeout(() => {
                    //reconfigure those actions
                    this.reConfigure(uniqueDownstreamActions);    
                });
                
            };

            this.processState($state);
        }

        //re-orders actions according to their position on array
        private reOrderActions(actions: model.ActionDTO[]) {
            for (var i = 0; i < actions.length; i++) {
                actions[i].ordering = i + 1;
            }
        }

        private reConfigure(actions: model.ActionDTO[]) {
            for (var i = 0; i < actions.length; i++) {
                
                this.$scope.$broadcast(pca.MessageType[pca.MessageType.PaneConfigureAction_Reconfigure], new pca.ActionReconfigureEventArgs(actions[i]));
                if (actions[i].childrenActions.length > 0) {
                    this.reConfigure(<model.ActionDTO[]>actions[i].childrenActions);
                }
            }
        }

        //inserts specified action to it's parent and returns downstream actions
        private insertActionToParent(action: model.ActionDTO, index: number): model.ActionDTO[] {
            //we should update childActions property of specified action
            var newParent = this.findActionById(action.parentRouteNodeId);
            var newList: interfaces.IActionDTO[];
            //might be root level
            if (newParent !== null) {
                newList = newParent.childrenActions;
            } else {
                //lets check subroutes
                var subRoute = this.findSubRouteById(action.parentRouteNodeId);
                newList = subRoute.actions;
            }

            //now we should inject this action to it's new parent to proper position
            newList.splice(index, 0, action);

            //set their ordering according to their position
            this.reOrderActions(<model.ActionDTO[]>newList);

            //lets call reconfigure on downstream actions
            return <model.ActionDTO[]>newList.slice(index + 1, newList.length);
        }
        
        //removes specified action from it's parent and returns downstream actions
        private findAndRemoveAction(action: model.ActionDTO): model.ActionDTO[] {
            var currentParent = this.findActionById(action.parentRouteNodeId);
            var listToRemoveActionFrom: interfaces.IActionDTO[];
            //might be root level
            if (currentParent !== null) {
                listToRemoveActionFrom = currentParent.childrenActions;
            } else {
                //lets check subroutes
                var subRoute = this.findSubRouteById(action.parentRouteNodeId);
                listToRemoveActionFrom = subRoute.actions;
            }

            var index = 0;
            //remove this action from it's old parent
            for (var i = 0; i < listToRemoveActionFrom.length; i++) {
                if (listToRemoveActionFrom[i].id === action.id) {
                    listToRemoveActionFrom.splice(i, 1);
                    index = i;
                    break;
                }
            }
            this.reOrderActions(<model.ActionDTO[]>listToRemoveActionFrom);

            //return downstream actions of removed action
            return <model.ActionDTO[]>listToRemoveActionFrom.slice(index, listToRemoveActionFrom.length);
        }

        private findSubRouteById(id: string): model.SubrouteDTO {
            for (var i = 0; i < this.$scope.current.route.subroutes.length; i++) {
                if (this.$scope.current.route.subroutes[i].id === id) {
                    return this.$scope.current.route.subroutes[i];
                }
            }

            return null;
        }

        private findActionById(id: string): model.ActionDTO {
            for (var subroute of this.$scope.current.route.subroutes) {
                for (var action of subroute.actions) {
                    var foundAction = this.searchAction(id, subroute.actions);
                    if (foundAction !== null) {
                        return foundAction;
                    }
                }
            }

            return null;
        }

        private searchAction(id: string, actionList: model.ActionDTO[]): model.ActionDTO {
            for (var i = 0; i < actionList.length; i++) {
                if (actionList[i].id === id) {
                    return actionList[i];
                }
                if (actionList[i].childrenActions.length) {
                    var foundAction = this.searchAction(id, <model.ActionDTO[]>actionList[i].childrenActions);
                    if (foundAction !== null) {
                        return foundAction;
                    }
                }
            }
            return null;
        }

        private processState($state: ngState) {
            if ($state.params.solutionName) {
                var isGuid = /\w{8}-\w{4}-\w{4}-\w{4}-\w{12}/.test($state.params.solutionName);
                if (isGuid) {
                    this.$scope.routeId = $state.params.solutionName;
                } else {
                    return this.createNewSolution($state.params.solutionName);
                }
            } else {
                this.$scope.routeId = $state.params.id;
            }

            this.loadRoute();
        }

        private createNewSolution(solutionName: string) {
            var route = this.ActionService.createSolution({
                solutionName: solutionName
            });
            route.$promise.then((curRoute: interfaces.IRouteVM) => {
                this.$scope.routeId = curRoute.id;
                this.onRouteLoad('solution', curRoute);
            });
        }

        private loadRoute(mode = 'route') {
            var routePromise = this.RouteService.getFull({ id: this.$scope.routeId });
            routePromise.$promise.then(this.onRouteLoad.bind(this, mode));
        }

        private onRouteLoad(mode: string, curRoute: interfaces.IRouteVM) {
            this.$scope.mode = mode;
            this.$scope.current.route = curRoute;
            this.$scope.currentSubroute = curRoute.subroutes[0];
            this.renderRoute(curRoute);
        }

        /*
            Mapping of incoming messages to handlers
        */
        private setupMessageProcessing() {
            //Process Configure Action Pane events
            this.$scope.$on(pca.MessageType[pca.MessageType.PaneConfigureAction_ActionUpdated],
                (event: ng.IAngularEvent, eventArgs: pca.ActionUpdatedEventArgs) => this.PaneConfigureAction_ActionUpdated(eventArgs));
            this.$scope.$on(pca.MessageType[pca.MessageType.PaneConfigureAction_ActionRemoved],
                (event: ng.IAngularEvent, eventArgs: pca.ActionRemovedEventArgs) => this.PaneConfigureAction_ActionRemoved(eventArgs));
            this.$scope.$on(pca.MessageType[pca.MessageType.PaneConfigureAction_ChildActionsReconfiguration],
                (event: ng.IAngularEvent, childActionReconfigEventArgs: pca.ChildActionReconfigurationEventArgs) => this.PaneConfigureAction_ChildActionsReconfiguration(childActionReconfigEventArgs));

            //Process Select Action Pane events
            this.$scope.$on(psa.MessageType[psa.MessageType.PaneSelectAction_ActivityTypeSelected],
                (event: ng.IAngularEvent, eventArgs: psa.ActivityTypeSelectedEventArgs) => this.PaneSelectAction_ActivityTypeSelected(eventArgs));

            //TODO: is this necessary??
            this.$scope.$on(psa.MessageType[psa.MessageType.PaneSelectAction_ActionTypeSelected],
                (event: ng.IAngularEvent, eventArgs: psa.ActionTypeSelectedEventArgs) => this.PaneSelectAction_ActionTypeSelected(eventArgs));
            // TODO: do we need this any more?
            // this._scope.$on(psa.MessageType[psa.MessageType.PaneSelectAction_ActionUpdated],
            //     (event: ng.IAngularEvent, eventArgs: psa.ActionUpdatedEventArgs) => this.PaneSelectAction_ActionUpdated(eventArgs));
            //Handles Save Request From PaneSelectAction
            this.$scope.$on(psa.MessageType[psa.MessageType.PaneSelectAction_InitiateSaveAction],
                (event: ng.IAngularEvent, eventArgs: psa.ActionTypeSelectedEventArgs) => this.PaneSelectAction_InitiateSaveAction(eventArgs));

            this.$scope.$on(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_LongRunningOperation],
                (event: ng.IAngularEvent, eventArgs: pwd.LongRunningOperationEventArgs) => this.PaneWorkflowDesigner_LongRunningOperation(eventArgs));

            this.$scope.$on(pca.MessageType[pca.MessageType.PaneConfigureAction_SetSolutionMode], () => this.PaneConfigureAction_SetSolutionMode());
            this.$scope.$on(pca.MessageType[pca.MessageType.PaneConfigureAction_ChildActionsDetected], () => this.PaneConfigureAction_ChildActionsDetected());
        }

        private renderRoute(curRoute: interfaces.IRouteVM) {
            if (curRoute.subroutes.length == 0) return;

            var actions = [];
            for (var subroute of curRoute.subroutes) {
                for (var action of subroute.actions) {
                    actions.push(action);
                }
            }

            this.$scope.actionGroups = this.LayoutService.placeActions(actions, curRoute.startingSubrouteId);  
        }

        // If action updated, notify interested parties and update $scope.current.action
        private handleActionUpdate(action: model.ActionDTO) {
            if (!action) return;

            if (this.$scope.current.action.isTempId) {
                this.$scope.$broadcast(
                    pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ActionTempIdReplaced],
                    new pwd.ActionTempIdReplacedEventArgs(this.$scope.current.action.id, action.id)
                    );
            }

            this.$scope.current.action = action;
            //self.$scope.current.action.id = result.action.id;
            //self.$scope.current.action.isTempId = false;

                //Notify workflow designer of action update
            this.$scope.$broadcast(
                    pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ActionNameUpdated],
                    new pwd.ActionNameUpdatedEventArgs(action.id, action.name)
                    );

                if (this.CrateHelper.hasControlListCrate(action.crateStorage)) {
                    action.configurationControls = this.CrateHelper
                        .createControlListFromCrateStorage(action.crateStorage);
                }
            }

        private addAction(group: model.ActionGroup) {
            var self = this;
            var promise = this.RouteBuilderService.saveCurrent(this.$scope.current);
            promise.then((result: model.RouteBuilderState) => {
                //we should just raise an event for this
                self.$scope.$broadcast(psa.MessageType[psa.MessageType.PaneSelectAction_ActionAdd], new psa.ActionAddEventArgs(group));
            });
        }

        private reloadRoute() {
            //this.$scope.actionGroups = [];
            this.$scope.current = new model.RouteBuilderState();
            this.loadRoute();
        }

        private deleteAction(action: model.ActionDTO) {
            var self = this;
            self.ActionService.deleteById({ id: action.id, confirmed: false }).$promise.then((response) => {
                self.reloadRoute();
            }, (error) => {
                //TODO check error status while completing DO-1335
                this.uiHelperService
                    .openConfirmationModal('Are you sure you want to delete this Action? You will have to reconfigure all downstream Actions.')
                    .then(() => {
                    self.ActionService.deleteById({ id: action.id, confirmed: true }).$promise.then(() => {
                        self.reloadRoute();
                    });
                });
            }); 
        }


        private PaneSelectAction_ActivityTypeSelected(eventArgs: psa.ActivityTypeSelectedEventArgs) {

            var activityTemplate = eventArgs.activityTemplate;
            // Generate next Id.
            var id = this.LocalIdentityGenerator.getNextId();                
            var parentId = this.$scope.currentSubroute.id;
            if (eventArgs.group !== null && eventArgs.group.parentAction !== null) {
                parentId = eventArgs.group.parentAction.id;
            }
            // Create new action object.
            var action = new model.ActionDTO(parentId, id, true);
            action.name = activityTemplate.name;
            action.label = activityTemplate.label;
            // Add action to Workflow Designer.
            this.$scope.current.action = action.toActionVM();
            this.$scope.current.action.activityTemplate = activityTemplate;
            this.$scope.current.action.activityTemplateId = activityTemplate.id;
            this.selectAction(action, eventArgs.group);
        }

        private addActionToUI(action: model.ActionDTO, group: model.ActionGroup) {
            this.$scope.current.action = action;
            if (group !== null && group.parentAction !== null) {
                group.parentAction.childrenActions.push(action);
            } else {
                this.$scope.currentSubroute.actions.push(action);
            }

            //lets check if this add operation requires a complete re-render
            /*if (action.childrenActions.length < 1 && action.activityTemplate.type !== 'Loop') {
                return;
            }*/

            this.renderRoute(<interfaces.IRouteVM>this.$scope.current.route);
        }

        /*
            Handles message 'WorkflowDesignerPane_ActionSelected'. 
            This message is sent when user is selecting an existing action or after addng a new action. 
        */
        private selectAction(action: model.ActionDTO, group: model.ActionGroup) {
            
            console.log("Action selected: " + action.id);
            var originalId,
                actionId = action.id,
                canBypassActionLoading = false; // Whether we can avoid reloading an action from the backend

            if (this.$scope.current.action) {
                originalId = this.$scope.current.action.id;
            }

            // Save previously selected action (and associated entities)
            // If a new action has just been added, it will be saved. 
            var promise = this.RouteBuilderService.saveCurrent(this.$scope.current);

            promise.then((result: model.RouteBuilderState) => {

                if (result.action != null) {
                    // Notity interested parties of action update and update $scope
                    this.handleActionUpdate(result.action);

                    // Whether id of the previusly selected action has changed after calling save
                    var idChangedFromTempToPermanent = (originalId != result.action.id);

                    // Since actions are saved immediately after addition, assume that 
                    // any selected action with a temporary id has just been added by user. 
                    // NOTE: this assumption may lead to subtle bugs if user is adding
                    // actions faster than his/her bandwidth allows to save them. 

                    // If earlier we saved a newly added action, set current action id to
                    // the permanent id we received after saving operation. 
                    actionId = idChangedFromTempToPermanent && result.action
                        ? result.action.id
                        : action.id;

                    //Whether user selected a new action or just clicked on the current one
                    var actionChanged = action.id != originalId;
                
                    // Determine if we need to load action from the db or we can just use 
                    // the one returned from the above saveCurrent operation.
                    canBypassActionLoading = idChangedFromTempToPermanent || !actionChanged;
                }
                
                if (actionId == '00000000-0000-0000-0000-000000000000') {
                    throw Error('Action has not been persisted. Process Builder cannot proceed ' +
                        'to action type selection for an unpersisted action.');
                }
                if (canBypassActionLoading) {
                    this.addActionToUI(result.action, group);
                }
                else {
                    this.ActionService.get({ id: actionId }).$promise.then(action => {
                        this.addActionToUI(action, group);
                    });
                }
            });
        }

        /*
            Handles message 'WorkflowDesignerPane_TemplateSelected'
        */
        private PaneWorkflowDesigner_TemplateSelected(eventArgs: pwd.TemplateSelectedEventArgs) {
            console.log("RouteBuilderController: template selected");

            this.RouteBuilderService.saveCurrent(this.$scope.current)
                .then((result: model.RouteBuilderState) => {
                // Notity interested parties of action update and update $scope
                this.handleActionUpdate(result.action);

            });
        }

        /*
            Handles message 'ConfigureActionPane_ActionUpdated'
        */
        private PaneConfigureAction_ActionUpdated(eventArgs: pca.ActionUpdatedEventArgs) {
        }

        /*
            Handles message 'SelectActionPane_ActionTypeSelected'
        */
        private PaneSelectAction_ActionTypeSelected(eventArgs: psa.ActionTypeSelectedEventArgs) {
            var pcaEventArgs = new pca.RenderEventArgs(eventArgs.action);
            var pwdEventArs = new pwd.UpdateActivityTemplateIdEventArgs(eventArgs.action.id, eventArgs.action.activityTemplate.id);
            this.$scope.$broadcast(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_UpdateActivityTemplateId], pwdEventArs);
        }

        /*
           Handles message 'SelectActionPane_InitiateSaveAction'
       */
        private PaneSelectAction_InitiateSaveAction(eventArgs: psa.ActionTypeSelectedEventArgs) {
            var promise = this.RouteBuilderService.saveCurrent(this.$scope.current);
        }

        /*
            Handles message 'PaneConfigureAction_ActionRemoved'
        */
        private PaneConfigureAction_ActionRemoved(eventArgs: pca.ActionRemovedEventArgs) {
            this.$scope.$broadcast(
                pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ActionRemoved],
                new pwd.ActionRemovedEventArgs(eventArgs.id, eventArgs.isTempId)
                );
        }

        private updateChildActionsRecursive(curAction: interfaces.IActionVM) {
            this.$scope.$broadcast(pca.MessageType[pca.MessageType.PaneConfigureAction_Reconfigure]);
            
        }

        

        private PaneConfigureAction_ChildActionsReconfiguration(childActionReconfigEventArgs: pca.ChildActionReconfigurationEventArgs) {
            for (var i = 0; i < childActionReconfigEventArgs.actions.length; i++) {
                this.$scope.$broadcast(pca.MessageType[pca.MessageType.PaneConfigureAction_ReloadAction], new pca.ReloadActionEventArgs(childActionReconfigEventArgs.actions[i]));
            }
        }

        private PaneWorkflowDesigner_LongRunningOperation(eventArgs: dockyard.directives.paneWorkflowDesigner.LongRunningOperationEventArgs) {
            this._longRunningActionsCounter += eventArgs.flag === pwd.LongRunningOperationFlag.Started ? 1 : -1;

            if (this._longRunningActionsCounter < 0) {
                this._longRunningActionsCounter = 0;
            }
        }

        private PaneConfigureAction_SetSolutionMode() {
            this.loadRoute('solution');
        }

        private PaneConfigureAction_ChildActionsDetected() {
            this.loadRoute();
        }
    }
    app.controller('RouteBuilderController', RouteBuilderController);
} 