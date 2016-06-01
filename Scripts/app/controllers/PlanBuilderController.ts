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

    export interface IPlanBuilderScope extends ng.IScope {
        isPlanBuilderScope: boolean;
        planId: string;
        subPlans: Array<model.SubPlanDTO>;
        fields: Array<model.Field>;
        //currentSubroute: model.SubrouteDTO;

        // Identity of currently edited processNodeTemplate.
        //curNodeId: number;
        //// Flag, that indicates if currently edited processNodeTemplate has temporary identity.
        //curNodeIsTempId: boolean;
        current: model.PlanBuilderState;
        actionGroups: model.ActionGroup[];
        processedSubPlans: any[];

        addAction(group: model.ActionGroup): void;
        deleteAction: (action: model.ActivityDTO) => void;
        reConfigureAction: (action: model.ActivityDTO) => void;
        openAddLabelModal: (action: model.ActivityDTO) => void;
        isReConfiguring: boolean;
        chooseAuthToken: (action: model.ActivityDTO) => void;
        selectAction(action): void;
        isBusy: () => boolean;
        onActionDrop: (group: model.ActionGroup, actionId: string, index: number) => void;
        mode: string;
        editingMode: string;
        solutionName: string;
        curAggReloadingActions: Array<string>;
        addSubPlan: () => void;
        openMenu: ($mdOpenMenu: any , ev: any) => void;
        view: string;
        viewMode: string;
        hasAnyActivity: (pSubPlan: any) => boolean;
}


    //Setup aliases
    import pwd = dockyard.directives.paneWorkflowDesigner;
    import pca = dockyard.directives.paneConfigureAction;
    import psa = dockyard.directives.paneSelectAction;
    import planEvents = dockyard.Fr8Events.Plan;

    export class PlanBuilderController {
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
            'PlanService',
            '$timeout',
            'PlanBuilderService',
            'CrateHelper',
            '$filter',
            'UIHelperService',
            'LayoutService',
            '$modal',
            'AuthService',
            'ConfigureTrackerService',
            'SubPlanService',
            '$stateParams'
        ];

        private _longRunningActionsCounter: number;
        private _loading = false;

        constructor(
            private $scope: IPlanBuilderScope,
            private LocalIdentityGenerator: services.ILocalIdentityGenerator,
            private $state: ng.ui.IStateService,
            private ActionService: services.IActionService,
            private $http: ng.IHttpService,
            private PlanService: services.IPlanService,
            private $timeout: ng.ITimeoutService,
            private PlanBuilderService: services.IPlanBuilderService,
            private CrateHelper: services.CrateHelper,
            private $filter: ng.IFilterService,
            private uiHelperService: services.IUIHelperService,
            private LayoutService: services.ILayoutService,
            private $modal: any,
            private AuthService: services.AuthService,
            private ConfigureTrackerService: services.ConfigureTrackerService,
            private SubPlanService: services.ISubPlanService,
            private $stateParams: ng.ui.IStateParamsService
        ) {

            this.LayoutService.resetLayout();

            this.$scope.isPlanBuilderScope = true;
            this.$scope.isReConfiguring = false;

            this.$scope.current = new model.PlanBuilderState();
            this.$scope.actionGroups = [];

            this.$scope.curAggReloadingActions = [];

            this.setupMessageProcessing();

            this.$scope.view = $stateParams['view'];
            this.$scope.viewMode = $stateParams['viewMode'];
            
            this.$scope.addAction = (group: model.ActionGroup) => {
                this.addAction(group);
            }

            this.$scope.hasAnyActivity = (pSubPlan) => {
                var actionGroups = <Array<model.ActionGroup>>pSubPlan.actionGroups;
                return _.any(actionGroups, (actionGroup: model.ActionGroup) => {
                    // return true where any outcome has a "test" property defined
                    return actionGroup.envelopes.length > 0;
                });
            };

            this.$scope.isBusy = () => {
                return this._longRunningActionsCounter > 0 || this._loading;
            };

            this._longRunningActionsCounter = 0;

            $scope.deleteAction = <() => void>angular.bind(this, this.deleteAction);
            $scope.addSubPlan = <() => void>angular.bind(this, this.addSubPlan);
            $scope.openMenu = ($mdOpenMenu, ev) => {
                $mdOpenMenu(ev);
            };
            $scope.reConfigureAction = (action: model.ActivityDTO) => {
                var actionsArray = new Array<model.ActivityDTO>();
                actionsArray.push(action);
                this.reConfigure(actionsArray);
            };

            $scope.openAddLabelModal = (action: model.ActivityDTO) => { 

                var modalInstance = $modal.open({
                    animation: true,
                    templateUrl: '/AngularTemplate/ActivityLabelModal',
                    controller: 'ActivityLabelModalController',
                    resolve: {
                        label: () => action.label
                    }
                })
                modalInstance.result.then(function (label: string) {
                    action.label = label;
                    ActionService.save(action);
                });
            }
            this.$scope.chooseAuthToken = (action: model.ActivityDTO) => {
                this.chooseAuthToken(action);
            };

            this.$scope.selectAction = (action: model.ActivityDTO) => {
                if (!this.$scope.current.activities || this.$scope.current.activities.id !== action.id)
                    this.selectAction(action, null);

            }

            $scope.$watch(function () {
                return $(".resizable").width();
            }, function (newVal, oldVal) {
                if (newVal !== oldVal) {
                    $(".designer-header-fixed").width(newVal);
                }
            })

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
                if (realAction.parentPlanNodeId !== group.envelopes[0].activity.parentPlanNodeId) {
                    //set new parent
                    realAction.parentPlanNodeId = group.envelopes[0].activity.parentPlanNodeId;
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

                //let's re-render plan builder
                this.renderPlan(<interfaces.IPlanVM>this.$scope.current.plan);

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
            var currentPlan = this.$scope.current.plan;

            this.setAdvancedEditingMode();
            var newSubPlan = new model.SubPlanDTO(null, true, currentPlan.id, currentPlan.id, "SubPlan-" + currentPlan.subPlans.length);

            this.SubPlanService.create(newSubPlan).$promise.then((createdSubPlan: model.SubPlanDTO) => {
                createdSubPlan.activities = [];
                createdSubPlan.criteria = null;
                currentPlan.subPlans.push(createdSubPlan);

                //dirty hack
                var processedGroup = this.LayoutService.addEmptyProcessedGroup(createdSubPlan.subPlanId);
                this.$scope.processedSubPlans.push({ subPlan: createdSubPlan, actionGroups: processedGroup });
                //this.renderPlan(<interfaces.IPlanVM>currentPlan);
                this.$scope.$broadcast(<any>planEvents.SUB_PLAN_MODIFICATION);
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
            var newParent = this.findActionById(action.parentPlanNodeId);
            var newList: interfaces.IActivityDTO[];
            //might be root level
            if (newParent !== null) {
                newList = newParent.childrenActivities;
            } else {
                //lets check subplans
                var subPlan = this.findSubPlanById(action.parentPlanNodeId);
                newList = subPlan.activities;
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
            var currentParent = this.findActionById(action.parentPlanNodeId);
            var listToRemoveActionFrom: interfaces.IActivityDTO[];
            //might be root level
            if (currentParent !== null) {
                listToRemoveActionFrom = currentParent.childrenActivities;
            } else {
                //lets check subplans
                var subPlan = this.findSubPlanById(action.parentPlanNodeId);
                listToRemoveActionFrom = subPlan.activities;
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

        private findSubPlanById(id: string): model.SubPlanDTO {
            for (var i = 0; i < this.$scope.current.plan.subPlans.length; i++) {
                if (this.$scope.current.plan.subPlans[i].subPlanId === id) {
                    return this.$scope.current.plan.subPlans[i];
                }
            }

            return null;
        }

        private findActionById(id: string): model.ActivityDTO {
            for (var subPlan of this.$scope.current.plan.subPlans) {
                var foundAction = this.searchAction(id, subPlan.activities);
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

            this.loadPlan($state.params.viewMode);
        }

        
        private createNewSolution(solutionName: string) {
            var plan = this.PlanService.createSolution({
                solutionName: solutionName
            });
            plan.$promise.then((curPlan: interfaces.IPlanFullDTO) => {
                this.$scope.planId = curPlan.plan.id;
                this.onPlanLoad('solution', curPlan);
            });
        }

        private loadPlan(mode = 'plan') {
            var planPromise = this.PlanService.getFull({ id: this.$scope.planId });
            planPromise.$promise.then(this.onPlanLoad.bind(this, mode));
        }

        private reloadFirstActions() {
            this.$timeout(() => {
                this.$scope.current.plan.subPlans.forEach(plan => this.$scope.reConfigureAction(plan.activities[0]));
            }, 1500);
        }

        private onPlanLoad(mode: string, curPlan: interfaces.IPlanFullDTO) {
            this.AuthService.setCurrentPlan(<interfaces.IPlanVM>curPlan.plan);
            this.AuthService.clear();

            this.$scope.mode = mode;
            this.$scope.current.plan = curPlan.plan;
            //this.$scope.currentSubroute = curRoute.subroutes[0];
            if (curPlan.plan.subPlans.length > 1) {
                this.setAdvancedEditingMode();
            }
            this.renderPlan(<interfaces.IPlanVM>curPlan.plan);
            this.$state.go('planBuilder', { id: curPlan.plan.id, viewMode: mode }).then(this.reloadFirstActions.bind(this));
        }

        /*
            Mapping of incoming messages to handlers
        */
        private setupMessageProcessing() {
            //Process Configure Action Pane events
            this.$scope.$on(pca.MessageType[pca.MessageType.PaneConfigureAction_ActionUpdated],
                (event: ng.IAngularEvent, eventArgs: model.ActivityDTO) => this.PaneConfigureAction_ActionUpdated(eventArgs));
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


        //This function filters activities by checking if they contain specified StandardConfigurationControls
        //crate with given label
        private filterActivitiesByUICrate(activities: Array<model.ActivityDTO>, uiCrateLabel: string): Array<model.ActivityDTO> {
            var filteredList: Array<model.ActivityDTO>;
            //if our view parameter is set - we should make sure we render only activities with given crates
            if (uiCrateLabel) {
                filteredList = [];
                for (var i = 0; i < activities.length; i++) {
                    var foundUiCrate = this.CrateHelper.findByManifestTypeAndLabel(
                        activities[i].crateStorage, 'Standard UI Controls', this.$scope.view
                    );
                    if (foundUiCrate !== null) {
                        filteredList.push(activities[i]);
                    }
                }
            } else {
                filteredList = activities;
            }

            return filteredList;
        }

        private renderPlan(curPlan: interfaces.IPlanVM) {

            this.LayoutService.resetLayout();

            if (curPlan.subPlans.length === 0) return;

            this.$scope.processedSubPlans = [];
            for (var subPlan of curPlan.subPlans) {
                var activities: Array<model.ActivityDTO> = this.filterActivitiesByUICrate(subPlan.activities, this.$scope.view);
                var actionGroups = this.LayoutService.placeActions(activities, subPlan.subPlanId);
                this.$scope.processedSubPlans.push({ subPlan: subPlan, actionGroups: actionGroups });
            }
        }

        private renderActions(activitiesCollection: model.ActivityDTO[]) {
            activitiesCollection = this.filterActivitiesByUICrate(activitiesCollection, this.$scope.view);
            if (activitiesCollection != null && activitiesCollection.length !== 0) {
                this.$scope.actionGroups = this.LayoutService.placeActions(activitiesCollection,
                    this.$scope.current.plan.startingSubPlanId);
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
            var promise = this.PlanBuilderService.saveCurrent(this.$scope.current);
            promise.then((result: model.PlanBuilderState) => {
                //we should just raise an event for this
                self.$scope.$broadcast(psa.MessageType[psa.MessageType.PaneSelectAction_ActionAdd], new psa.ActionAddEventArgs(group));
            });
        }

        private reloadPlan() {
            //this.$scope.actionGroups = [];
            this.$scope.current = new model.PlanBuilderState();
            this.loadPlan();
        }

        private getDownstreamActions(currentAction: model.ActivityDTO) {
            var results: Array<model.ActivityDTO> = [];
            this.$scope.actionGroups.forEach(group => {
                group.envelopes.filter((envelope: model.ActivityEnvelope) => {
                    return envelope.activity.parentPlanNodeId === currentAction.parentPlanNodeId && envelope.activity.ordering > currentAction.ordering;
                }).forEach(envelope => {
                    results.push(envelope.activity);
                });
            });
            return results;

        }

        private chooseAuthToken(action: model.ActivityDTO) {
            var self = this;

            var modalScope = <IAuthenticationDialogScope>self.$scope.$new(true);
            modalScope.activities = [action];

            self.$modal.open({
                animation: true,
                templateUrl: '/AngularTemplate/AuthenticationDialog',
                controller: 'AuthenticationDialogController',
                scope: modalScope
            })
            .result
            .then(() => {
                self.$scope.$broadcast(
                    dockyard.directives.paneConfigureAction.MessageType[dockyard.directives.paneConfigureAction.MessageType.PaneConfigureAction_AuthCompleted],
                    new dockyard.directives.paneConfigureAction.AuthenticationCompletedEventArgs(<interfaces.IActivityDTO>({ id: action.id }))
                );
            });
        }

        private deleteAction(action: model.ActivityDTO) {
            var self = this;
            self.startLoader();
            self.ActionService.deleteById({ id: action.id }).$promise.then((response) => {
                self.reloadPlan();
                self.stopLoader();
            }, (error) => {
                //TODO check error status while completing DO-1335

                var alertMessage = new model.AlertDTO();
                alertMessage.title = "Please confirm";
                alertMessage.body = "Are you sure you want to delete this Activity? You will have to reconfigure all downstream Actions.";

                this.uiHelperService
                    .openConfirmationModal(alertMessage)
                    .then(() => {
                        self.startLoader();
                        self.ActionService.deleteById({ id: action.id }).$promise.then(() => {
                            self.reloadPlan();
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

            action.name = activityTemplate.label;
            // Add action to Workflow Designer.
            this.$scope.current.activities = action.toActionVM();
            this.$scope.current.activities.activityTemplate = activityTemplate;
            this.selectAction(action, eventArgs.group);
        }

        private allowsChildren(action: model.ActivityDTO) {
            return action.activityTemplate.type === 'Loop';
        }

        private addActionToUI(action: model.ActivityDTO, group: model.ActionGroup) {
            this.$scope.current.activities = action;

            var parentAction = this.findActionById(action.parentPlanNodeId);
            if (parentAction != null) {
                parentAction.childrenActivities.push(action);
            } else {
                var subPlan = this.findSubPlanById(action.parentPlanNodeId);
                subPlan.activities.push(action);
            }

            //TODO we need to change rendering code

            if (this.allowsChildren(action)) {
                this.renderPlan(<interfaces.IPlanVM>this.$scope.current.plan);
            } else {
                for (var i = 0; i < this.$scope.processedSubPlans.length; i++) {
                    var curSubPlan = this.$scope.processedSubPlans[i];
                    for (var j = 0; j < curSubPlan.actionGroups.length; j++) {
                        var curActionGroup = <model.ActionGroup>curSubPlan.actionGroups[j];
                        if (curActionGroup.parentId === action.parentPlanNodeId) {
                            curActionGroup.envelopes.push(new model.ActivityEnvelope(action));
                        }
                    }
                }
            }
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
            var promise = this.PlanBuilderService.saveCurrent(this.$scope.current);

            promise.then((result: model.PlanBuilderState) => {

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
            console.log("PlanBuilderController: template selected");
            this.PlanBuilderService.saveCurrent(this.$scope.current)
                .then((result: model.PlanBuilderState) => {
                    // Notity interested parties of action update and update $scope
                    this.handleActionUpdate(result.activities);

                });
        }

        /*
            Handles message 'ConfigureActionPane_ActionUpdated'
            TODO : we should update entire activity
        */
        private PaneConfigureAction_ActionUpdated(updatedAction: model.ActivityDTO) {
            var action = this.findActionById(updatedAction.id);
            action.name = updatedAction.name;
            action.label = updatedAction.label;
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
            var promise = this.PlanBuilderService.saveCurrent(this.$scope.current);
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
                this.loadPlan("solution");
            }
        }

        private PaneConfigureAction_ChildActionsDetected() {
            this.loadPlan();
        }

        private PaneConfigureAction_ExecutePlan() {
            var self = this;

            ++self._longRunningActionsCounter;

            this.PlanService.runAndProcessClientAction(this.$scope.current.plan.id)
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
            var subplan = this.getActionSubPlan(callConfigureResponseEventArgs.action);
            if (subplan) {
                results = this.getAgressiveReloadingActions(subplan.actionGroups, callConfigureResponseEventArgs.action);
            }

            for (var index = 0; index < results.length; index++) {
                if (this.$scope.curAggReloadingActions.indexOf(results[index].id) === -1) {
                    this.$scope.curAggReloadingActions.push(results[index].id);
                } else {
                    //var positionToRemove = this.$scope.curAggReloadingActions.indexOf(results[index].id);
                    //this.$scope.curAggReloadingActions.splice(positionToRemove, 1);
                    continue;
                    //return;
                }
            }
            
            // scan all actions to find actions with tag AgressiveReload in ActivityTemplate
            this.reConfigure(results);

            // Reconfigure also child activities of the activity which initiated reconfiguration. 
            if (callConfigureResponseEventArgs.action && callConfigureResponseEventArgs.action.childrenActivities.length > 0) {
                this.reConfigure(<model.ActivityDTO[]>callConfigureResponseEventArgs.action.childrenActivities);
            }


            //wait UI to finish rendering
            this.$timeout(() => {
                if (callConfigureResponseEventArgs.focusElement != null) {
                    //broadcast to control to set focus on current element        
                    this.$scope.$broadcast(<any>planEvents.ON_FIELD_FOCUS, callConfigureResponseEventArgs);
                }
            }, 300);
        }

        private getActionSubPlan(activity: interfaces.IActivityDTO): any {
            for (var i = 0; i < this.$scope.processedSubPlans.length; ++i) {
                var subPlan = this.$scope.processedSubPlans[i];
                if (!subPlan.actionGroups) {
                    continue;
                }

                for (var j = 0; j < subPlan.actionGroups.length; ++j) {
                    var actionGroup = subPlan.actionGroups[j];
                    if (!actionGroup.envelopes) {
                        continue;
                    }


                    for (var k = 0; k < actionGroup.envelopes.length; ++k) {
                        var envelope = actionGroup.envelopes[k];
                        if (envelope.activity.id === activity.id) {
                            return subPlan;
                        }
                    }
                }
            }

            return null;
        }

        private getAgressiveReloadingActions(
            actionGroups: Array<model.ActionGroup>,
            currentAction: interfaces.IActivityDTO) {
                         
            var results: Array<model.ActivityDTO> = [];
            var currentGroupArray = actionGroups.filter(group => _.any<model.ActivityEnvelope>(group.envelopes, envelope => envelope.activity.id == currentAction.id));
            if (currentGroupArray.length == 0) {
                return [];
            }
            var currentGroup = currentGroupArray[0]; // max one item is possible.
            currentGroup.envelopes.filter(envelope => 
                 /* envelope.activity.activityTemplate.tags !== null 
                && envelope.activity.activityTemplate.tags.indexOf('AggressiveReload') !== -1 && */
                envelope.activity.ordering > currentAction.ordering
            ).forEach(env => {
                results.push(env.activity);
            });

            return results;
        }
    }
    app.controller('PlanBuilderController', PlanBuilderController);
    app.controller('ActivityLabelModalController', ['$scope', '$modalInstance','label', ($scope: any, $modalInstance: any, label: string): void => {

        $scope.label = label;

        $scope.submitForm = () => {
            $modalInstance.close($scope.label);
        };

        $scope.cancel = () => {
            $modalInstance.dismiss();
        };

    }]);
}