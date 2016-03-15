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
        planId: string;
        subroutes: Array<model.SubrouteDTO>;
        fields: Array<model.Field>;
        //currentSubroute: model.SubrouteDTO;

        // Identity of currently edited processNodeTemplate.
        //curNodeId: number;
        //// Flag, that indicates if currently edited processNodeTemplate has temporary identity.
        //curNodeIsTempId: boolean;
        current: model.RouteBuilderState;
        actionGroups: model.ActionGroup[];
        processedSubRoutes: any[];

        addAction(group: model.ActionGroup): void;
        deleteAction: (action: model.ActivityDTO) => void;
        reConfigureAction: (action: model.ActivityDTO) => void;
        chooseAuthToken: (action: model.ActivityDTO) => void;
        selectAction(action): void;
        isBusy: () => boolean;
        onActionDrop: (group: model.ActionGroup, actionId: string, index: number) => void;
        mode: string;
        editingMode: string;
        solutionName: string;
        curAggReloadingActions: Array<string>;
        addSubPlan: () => void;
    }


    //Setup aliases
    import pwd = dockyard.directives.paneWorkflowDesigner;
    import pca = dockyard.directives.paneConfigureAction;
    import psa = dockyard.directives.paneSelectAction;

    export class RouteBuilderController {
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
            '$modal',
            'AuthService',
            'ConfigureTrackerService',
            'SubPlanService'
        ];

        private _longRunningActionsCounter: number;
        private _loading = false;

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
            private LayoutService: services.ILayoutService,
            private $modal: any,
            private AuthService: services.AuthService,
            private ConfigureTrackerService: services.ConfigureTrackerService,
            private SubPlanService: services.ISubPlanService
            ) {

            this.LayoutService.resetLayout();
            this.$scope.current = new model.RouteBuilderState();
            this.$scope.actionGroups = [];

            this.$scope.curAggReloadingActions = []; 

            this.setupMessageProcessing();

            this.$scope.addAction = (group: model.ActionGroup) => {
                this.addAction(group);
            }

            this.$scope.isBusy = () => {
                return this._longRunningActionsCounter > 0 || this._loading;
            };

            this._longRunningActionsCounter = 0;

            $scope.deleteAction = <() => void>angular.bind(this, this.deleteAction);
            $scope.addSubPlan = <() => void> angular.bind(this, this.addSubPlan);
            $scope.reConfigureAction = (action: model.ActivityDTO) => {
                var actionsArray = new Array<model.ActivityDTO>();
                actionsArray.push(action);
                this.reConfigure(actionsArray);
            };
            this.$scope.chooseAuthToken = (action: model.ActivityDTO) => {
                this.chooseAuthToken(action);
            };

            this.$scope.selectAction = (action: model.ActivityDTO) => {
                if (!this.$scope.current.activities || this.$scope.current.activities.id !== action.id)
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
                var downstreamActions: model.ActivityDTO[] = this.findAndRemoveAction(realAction);

                //TODO check parent action change with a more solid method
                //this action is moved to a different parent
                if (realAction.parentRouteNodeId !== group.envelopes[0].activity.parentRouteNodeId) {
                    //set new parent
                    realAction.parentRouteNodeId = group.envelopes[0].activity.parentRouteNodeId;
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
                var uniqueDownstreamActions = _.uniq(downstreamActions, (action: model.ActivityDTO) => action.id);
                
                //let's wait for UI to finish it's rendering
                this.$timeout(() => {
                    //reconfigure those actions
                    this.reConfigure(uniqueDownstreamActions);
                });

            };


            var currentState: number;
            $scope.$watch('current.route.routeState', () => {
                if ($scope.current.route) {
                    if (currentState === undefined) currentState = $scope.current.route.routeState;

                    if (currentState !== $scope.current.route.routeState) {
                        if ($scope.current.route.routeState === model.RouteState.Inactive) {
                            RouteService.deactivate({ planId: $scope.current.route.id });
                        } else if ($scope.current.route.routeState === model.RouteState.Active) {
                            RouteService.activate(<any>{ planId: $scope.current.route.id, routeBuilderActivate: true })
                                    .$promise.then((result) => {
                                    if (result != null && result.status === "validation_error") {
                                        this.renderActions(result.activitiesCollection);
                                        $scope.current.route.routeState = model.RouteState.Inactive;
                                    }
                            });
                        }
                    }
                }
            });

            this.processState($state);
        }

        private startLoader() {
            this._loading = true;
        }

        private stopLoader() {
            this._loading = false;
        }

        private setAdvancedEditingMode() {
            this.$scope.editingMode = 'advanced';
        }

        private addSubPlan() {
            var currentRoute = this.$scope.current.route;

            this.setAdvancedEditingMode();
            var newSubPlan = new model.SubrouteDTO(null, true, currentRoute.id, "SubPlan-" + currentRoute.subroutes.length);
            
            this.SubPlanService.create(newSubPlan).$promise.then((createdSubPlan: model.SubrouteDTO) => {
                createdSubPlan.activities = [];
                createdSubPlan.criteria = null;
                currentRoute.subroutes.push(createdSubPlan);
                this.renderRoute(<interfaces.IRouteVM>currentRoute);
            });
        }

        //re-orders actions according to their position on array
        private reOrderActions(actions: model.ActivityDTO[]) {
            for (var i = 0; i < actions.length; i++) {
                actions[i].ordering = i + 1;
            }
        }

        private reConfigure(actions: model.ActivityDTO[]) {
            for (var i = 0; i < actions.length; i++) {
                this.$scope.$broadcast(pca.MessageType[pca.MessageType.PaneConfigureAction_Reconfigure], new pca.ActionReconfigureEventArgs(actions[i]));
                if (actions[i].childrenActivities.length > 0) {
                    this.reConfigure(<model.ActivityDTO[]>actions[i].childrenActivities);
                }
            }
        }

        //inserts specified action to it's parent and returns downstream actions
        private insertActionToParent(action: model.ActivityDTO, index: number): model.ActivityDTO[] {
            //we should update childActions property of specified action
            var newParent = this.findActionById(action.parentRouteNodeId);
            var newList: interfaces.IActivityDTO[];
            //might be root level
            if (newParent !== null) {
                newList = newParent.childrenActivities;
            } else {
                //lets check subroutes
                var subRoute = this.findSubRouteById(action.parentRouteNodeId);
                newList = subRoute.activities;
            }

            //now we should inject this action to it's new parent to proper position
            newList.splice(index, 0, action);

            //set their ordering according to their position
            this.reOrderActions(<model.ActivityDTO[]>newList);

            //lets call reconfigure on downstream actions
            return <model.ActivityDTO[]>newList.slice(index + 1, newList.length);
        }
        
        //removes specified action from it's parent and returns downstream actions
        private findAndRemoveAction(action: model.ActivityDTO): model.ActivityDTO[] {
            var currentParent = this.findActionById(action.parentRouteNodeId);
            var listToRemoveActionFrom: interfaces.IActivityDTO[];
            //might be root level
            if (currentParent !== null) {
                listToRemoveActionFrom = currentParent.childrenActivities;
            } else {
                //lets check subroutes
                var subRoute = this.findSubRouteById(action.parentRouteNodeId);
                listToRemoveActionFrom = subRoute.activities;
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
            this.reOrderActions(<model.ActivityDTO[]>listToRemoveActionFrom);

            //return downstream actions of removed action
            return <model.ActivityDTO[]>listToRemoveActionFrom.slice(index, listToRemoveActionFrom.length);
        }

        private findSubRouteById(id: string): model.SubrouteDTO {
            for (var i = 0; i < this.$scope.current.route.subroutes.length; i++) {
                if (this.$scope.current.route.subroutes[i].id === id) {
                    return this.$scope.current.route.subroutes[i];
                }
            }

            return null;
        }

        private findActionById(id: string): model.ActivityDTO {
            for (var subroute of this.$scope.current.route.subroutes) {
                var foundAction = this.searchAction(id, subroute.activities);
                if (foundAction !== null) {
                    return foundAction;
                }
            }

            return null;
        }

        private searchAction(id: string, actionList: model.ActivityDTO[]): model.ActivityDTO {
            for (var i = 0; i < actionList.length; i++) {
                if (actionList[i].id === id) {
                    return actionList[i];
                }
                if (actionList[i].childrenActivities.length) {
                    var foundAction = this.searchAction(id, <model.ActivityDTO[]>actionList[i].childrenActivities);
                    if (foundAction !== null) {
                        return foundAction;
                    }
                }
            }
            return null;
        }

        private processState($state: ngState) {
            if ($state.params.solutionName) {
                this.$scope.solutionName = $state.params.solutionName;
                var isGuid = /\w{8}-\w{4}-\w{4}-\w{4}-\w{12}/.test($state.params.solutionName);
                if (isGuid) {
                    this.$scope.planId = $state.params.solutionName;
                } else {
                    return this.createNewSolution($state.params.solutionName);
                }
            } else {
                this.$scope.planId = $state.params.id;
            }

            this.loadRoute();
        }

        private createNewSolution(solutionName: string) {
            var route = this.ActionService.createSolution({
                solutionName: solutionName
            });
            route.$promise.then((curRoute: interfaces.IRouteVM) => {
                this.$scope.planId = curRoute.id;
                this.onRouteLoad('solution', curRoute);
            });
        }

        private loadRoute(mode = 'route') {
            var routePromise = this.RouteService.getFull({ id: this.$scope.planId });
            routePromise.$promise.then(this.onRouteLoad.bind(this, mode));
        }

        private onRouteLoad(mode: string, curRoute: interfaces.IRouteVM) {
            this.AuthService.setCurrentPlan(curRoute);
            this.AuthService.clear();

            this.$scope.mode = mode;
            this.$scope.current.route = curRoute;
            //this.$scope.currentSubroute = curRoute.subroutes[0];
            if (curRoute.subroutes.length > 1) {
                this.setAdvancedEditingMode();
            }
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


            this.$scope.$on(pca.MessageType[pca.MessageType.PaneConfigureAction_DownStreamReconfiguration],
                (event: ng.IAngularEvent, eventArgs: pca.DownStreamReConfigureEventArgs) => this.PaneConfigureAction_ReConfigureDownStreamActivities(eventArgs));

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
            this.$scope.$on(pca.MessageType[pca.MessageType.PaneConfigureAction_ExecutePlan], () => this.PaneConfigureAction_ExecutePlan());

            // Handles Response from Configure call from PaneConfiguration
            this.$scope.$on(pca.MessageType[pca.MessageType.PaneConfigureAction_ConfigureCallResponse],
                (event: ng.IAngularEvent, callConfigureResponseEventArgs: pca.CallConfigureResponseEventArgs) => this.PaneConfigureAction_ConfigureCallResponse(callConfigureResponseEventArgs));
        }

        private renderRoute(curRoute: interfaces.IRouteVM) {

            this.LayoutService.resetLayout();

            if (curRoute.subroutes.length === 0) return;
            
            this.$scope.processedSubRoutes = [];
            for (var subroute of curRoute.subroutes) {
                var actionGroups = this.LayoutService.placeActions(subroute.activities, subroute.id);
                this.$scope.processedSubRoutes.push({ subroute: subroute, actionGroups: actionGroups });
            }
        }

        private renderActions(activitiesCollection: model.ActivityDTO[]) {
            if (activitiesCollection != null && activitiesCollection.length != 0) {
                this.$scope.actionGroups = this.LayoutService.placeActions(activitiesCollection,
                    this.$scope.current.route.startingSubrouteId);  
            }
        }

        // If action updated, notify interested parties and update $scope.current.action
        private handleActionUpdate(action: model.ActivityDTO) {
            if (!action) return;

            this.$scope.current.activities = action;
            //self.$scope.current.action.id = result.action.id;
            //self.$scope.current.action.isTempId = false;

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

        private getDownstreamActions(currentAction: model.ActivityDTO) {
            var results: Array<model.ActivityDTO> = [];
            this.$scope.actionGroups.forEach(group => {
                group.envelopes.filter((envelope: model.ActivityEnvelope) => {
                    return envelope.activity.parentRouteNodeId === currentAction.parentRouteNodeId && envelope.activity.ordering > currentAction.ordering;
                }).forEach(envelope => {
                    results.push(envelope.activity);
                });
            });
            return results;
            
        }

        private chooseAuthToken(action: model.ActivityDTO) {

            var self = this;

            var modalScope = <any>self.$scope.$new(true);
            modalScope.actionIds = [action.id];

            self.$modal.open({
                animation: true,
                templateUrl: '/AngularTemplate/AuthenticationDialog',
                controller: 'AuthenticationDialogController',
                scope: modalScope
            })
                .result
                .then(() => {
                    
                });
        }

        private deleteAction(action: model.ActivityDTO) {
            var self = this;
            self.startLoader();
            self.ActionService.deleteById({ id: action.id, confirmed: false }).$promise.then((response) => {
                self.reloadRoute();
                self.stopLoader();
            }, (error) => {
                //TODO check error status while completing DO-1335
                this.uiHelperService
                    .openConfirmationModal('Are you sure you want to delete this Activity? You will have to reconfigure all downstream Activities.')
                    .then(() => {
                        self.startLoader();
                        self.ActionService.deleteById({ id: action.id, confirmed: true }).$promise.then(() => {
                            self.reloadRoute();
                            self.stopLoader();
                        });
                    });
            });
        }

        private PaneSelectAction_ActivityTypeSelected(eventArgs: psa.ActivityTypeSelectedEventArgs) {

            var activityTemplate = eventArgs.activityTemplate;
            // Generate next Id.
            var id = this.LocalIdentityGenerator.getNextId();
            var parentId = eventArgs.group.parentId;
            var action = new model.ActivityDTO(this.$scope.planId, parentId, id);

            action.label = activityTemplate.label;
            // Add action to Workflow Designer.
            this.$scope.current.activities = action.toActionVM();
            this.$scope.current.activities.activityTemplate = activityTemplate;
            this.selectAction(action, eventArgs.group);
        }

        private addActionToUI(action: model.ActivityDTO, group: model.ActionGroup) {
            this.$scope.current.activities = action;

            var parentAction = this.findActionById(action.parentRouteNodeId);
            if (parentAction != null) {
                parentAction.childrenActivities.push(action);
            } else {
                var subRoute = this.findSubRouteById(action.parentRouteNodeId);
                subRoute.activities.push(action);
            }

            this.renderRoute(<interfaces.IRouteVM>this.$scope.current.route);
        }

        /*
            Handles message 'WorkflowDesignerPane_ActionSelected'. 
            This message is sent when user is selecting an existing action or after addng a new action. 
        */
        private selectAction(action: model.ActivityDTO, group: model.ActionGroup) {

            console.log("Activity selected: " + action.id);
            var originalId,
                actionId = action.id,
                canBypassActionLoading = false; // Whether we can avoid reloading an action from the backend

            if (this.$scope.current.activities) {
                originalId = this.$scope.current.activities.id;
            }
            // Save previously selected action (and associated entities)
            // If a new action has just been added, it will be saved. 
            var promise = this.RouteBuilderService.saveCurrent(this.$scope.current);

            promise.then((result: model.RouteBuilderState) => {

                if (result.activities != null) {
                    // Notity interested parties of action update and update $scope
                    this.handleActionUpdate(result.activities);

                    // Whether id of the previusly selected action has changed after calling save
                    var idChangedFromTempToPermanent = (originalId != result.activities.id);

                    // Since actions are saved immediately after addition, assume that 
                    // any selected action with a temporary id has just been added by user. 
                    // NOTE: this assumption may lead to subtle bugs if user is adding
                    // actions faster than his/her bandwidth allows to save them. 

                    // If earlier we saved a newly added action, set current action id to
                    // the permanent id we received after saving operation. 
                    actionId = idChangedFromTempToPermanent && result.activities
                        ? result.activities.id
                        : action.id;

                    //Whether user selected a new action or just clicked on the current one
                    var actionChanged = action.id != originalId;
                
                    // Determine if we need to load action from the db or we can just use 
                    // the one returned from the above saveCurrent operation.
                    canBypassActionLoading = idChangedFromTempToPermanent || !actionChanged;
                }

                if (actionId == '00000000-0000-0000-0000-000000000000') {
                    throw Error('Activity has not been persisted. Process Builder cannot proceed ' +
                        'to action type selection for an unpersisted action.');
                }
                if (canBypassActionLoading) {
                    this.addActionToUI(result.activities, group);
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
                    this.handleActionUpdate(result.activities);

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

        private PaneConfigureAction_ReConfigureDownStreamActivities(eventArgs: pca.DownStreamReConfigureEventArgs) {
            var actionsToReconfigure = this.getDownstreamActions(<model.ActivityDTO>eventArgs.action);
            actionsToReconfigure.splice(0, 0, <model.ActivityDTO>eventArgs.action);
            this.reConfigure(actionsToReconfigure);
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
            this.AuthService.clear();
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
            if (this.$scope.solutionName) {
                return this.createNewSolution(this.$scope.solutionName);
            } else {
                this.loadRoute("solution");
            }
        }

        private PaneConfigureAction_ChildActionsDetected() {
            this.loadRoute();
        }

        private PaneConfigureAction_ExecutePlan() {
            var self = this;

            ++self._longRunningActionsCounter;

            this.RouteService.runAndProcessClientAction(this.$scope.current.route.id)
                .finally(() => {
                    if (this._longRunningActionsCounter > 0) {
                        --this._longRunningActionsCounter;
                    }
                });
        }

        // This should handle everything that should be done when a configure call response arrives from server.
        private PaneConfigureAction_ConfigureCallResponse(callConfigureResponseEventArgs: pca.CallConfigureResponseEventArgs) {
            
            //let's wait for last configure call before starting on aggresive actions
            if (this.ConfigureTrackerService.hasPendingConfigureCalls()) {
                return;
            }

            var results: Array<model.ActivityDTO> = [];
            results = this.getAgressiveReloadingActions(this.$scope.actionGroups, callConfigureResponseEventArgs.action);

            for (var index = 0; index < results.length; index++) {
                if (this.$scope.curAggReloadingActions.indexOf(results[index].id) === -1) {
                    this.$scope.curAggReloadingActions.push(results[index].id);
                } else {
                    var positionToRemove = this.$scope.curAggReloadingActions.indexOf(results[index].id);
                    this.$scope.curAggReloadingActions.splice(positionToRemove, 1);
                    return;
                }
            }
            
            // scann all actions to find actions with tag AgressiveReload in ActivityTemplate
            this.reConfigure(results);

            //wait UI to finish rendering
            this.$timeout(() => {
                if (callConfigureResponseEventArgs.focusElement != null) {
                    //broadcast to control to set focus on current element        
                    this.$scope.$broadcast("onFieldFocus", callConfigureResponseEventArgs);
                }
            }, 300);
        }

        private getAgressiveReloadingActions (actionGroups: Array<model.ActionGroup>, currentAction: interfaces.IActivityDTO) {
            var results: Array<model.ActivityDTO> = [];
            actionGroups.forEach(group => {
                group.envelopes.filter(envelope => {
                    return envelope.activity.activityTemplate.tags !== null && envelope.activity.activityTemplate.tags.indexOf('AggressiveReload') !== -1;
                }).forEach(env => {
                    results.push(env.activity);
                });
            });

            return results;
        }
    }
    app.controller('RouteBuilderController', RouteBuilderController);
} 