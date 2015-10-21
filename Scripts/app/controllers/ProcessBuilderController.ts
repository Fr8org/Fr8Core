/// <reference path="../_all.ts" />

/*
    Detail (view/add/edit) controller
*/

module dockyard.controllers {
    'use strict';

    export interface IProcessBuilderScope extends ng.IScope {
        routeId: number;
        subroutes: Array<model.SubrouteDTO>;
        fields: Array<model.Field>;
        currentSubroute: model.SubrouteDTO;

        // Identity of currently edited processNodeTemplate.
        //curNodeId: number;
        //// Flag, that indicates if currently edited processNodeTemplate has temporary identity.
        //curNodeIsTempId: boolean;
        current: model.ProcessBuilderState;
        actionGroups: model.ActionGroup[]

        addAction(): void;
        deleteAction: (action: model.ActionDTO) => void;
        selectAction(action): void;
    }

    //Setup aliases
    import pwd = dockyard.directives.paneWorkflowDesigner;
    import pca = dockyard.directives.paneConfigureAction;
    import psa = dockyard.directives.paneSelectAction;

    class ProcessBuilderController {
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
            'ProcessBuilderService',
            'CrateHelper',
            '$filter',
            '$modal',
            '$window',
            'UIHelperService',
            'LayoutService'
        ];

        constructor(
            private $scope: IProcessBuilderScope,
            private LocalIdentityGenerator: services.ILocalIdentityGenerator,
            private $state: ng.ui.IState,
            private ActionService: services.IActionService,
            private $http: ng.IHttpService,
            private RouteService: services.IRouteService,
            private $timeout: ng.ITimeoutService,
            private ProcessBuilderService: services.IProcessBuilderService,
            private CrateHelper: services.CrateHelper,
            private $filter: ng.IFilterService,
            private $modal,
            private $window: ng.IWindowService,
            private uiHelperService: services.IUIHelperService,
            private LayoutService: services.ILayoutService
            ) {

            this.$scope.routeId = $state.params.id;
            this.$scope.current = new model.ProcessBuilderState();
            this.$scope.actionGroups = [];

            this.setupMessageProcessing();
            $timeout(() => this.loadProcessTemplate(), 500, true);

            this.$scope.addAction = () => {
                this.addAction();
            }

            $scope.deleteAction = <() => void> angular.bind(this, this.deleteAction);

            this.$scope.selectAction = (action: model.ActionDTO) => {
                if (!this.$scope.current.action || this.$scope.current.action.id !== action.id)
                    this.selectAction(action);
            }
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
            this.$scope.$on(pca.MessageType[pca.MessageType.PaneConfigureAction_InternalAuthentication],
                (event: ng.IAngularEvent, eventArgs: pca.InternalAuthenticationArgs) => this.PaneConfigureAction_InternalAuthentication(eventArgs));
            this.$scope.$on(pca.MessageType[pca.MessageType.PaneConfigureAction_ExternalAuthentication],
                (event: ng.IAngularEvent, eventArgs: pca.InternalAuthenticationArgs) => this.PaneConfigureAction_ExternalAuthentication(eventArgs));

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
        }

        private loadProcessTemplate() {
            var processTemplatePromise = this.RouteService.getFull({ id: this.$scope.routeId });
            var self = this;
            processTemplatePromise.$promise.then((curProcessTemplate: interfaces.IRouteVM) => {
                this.$scope.current.route = curProcessTemplate;
                this.$scope.currentSubroute = curProcessTemplate.subroutes[0];
                this.renderProcessTemplate(curProcessTemplate);
            });
        }

        private renderProcessTemplate(curProcessTemplate: interfaces.IRouteVM) {
            if (curProcessTemplate.subroutes.length == 0) return;

            var actions = [];
            for (var subroute of curProcessTemplate.subroutes) {
                for (var action of subroute.actions) {
                    actions.push(action);
                }
            }

            this.$scope.actionGroups = this.LayoutService.placeActions(actions, curProcessTemplate.startingSubrouteId);  
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

        private addAction() {
            console.log('Add action');
            var self = this;
            var promise = this.ProcessBuilderService.saveCurrent(this.$scope.current);
            promise.then((result: model.ProcessBuilderState) => {
                //we should just raise an event for this
                self.$scope.$broadcast(psa.MessageType[psa.MessageType.PaneSelectAction_ActionAdd],new psa.ActionAddEventArgs());
            });
        }

        private deleteAction(action: model.ActionDTO) {
            //TODO -> should we generate an event for delete event?
            var self = this;
            this.uiHelperService
                .openConfirmationModal('Are you sure you want to delete this Action? You will have to reconfigure all downstream Actions.')
                .then(() => {

                self.ActionService.deleteById({ id: action.id }).$promise.then(() => {
                    //lets reload process template
                    self.$scope.actionGroups = [];
                    self.$scope.current = new model.ProcessBuilderState();
                    self.loadProcessTemplate();
                });

            });
            
        }

        private PaneSelectAction_ActivityTypeSelected(eventArgs: psa.ActivityTypeSelectedEventArgs) {

            var activityTemplate = eventArgs.activityTemplate;
            // Generate next Id.
            var id = this.LocalIdentityGenerator.getNextId();                

            // Create new action object.
            var action = new model.ActionDTO(this.$scope.currentSubroute.id, id, true);
            action.name = activityTemplate.name;
            action.label = activityTemplate.label;
            // Add action to Workflow Designer.
            this.$scope.current.action = action.toActionVM();
            this.$scope.current.action.activityTemplateId = activityTemplate.id;
            this.$scope.actionGroups[0].actions.push(action);

            this.selectAction(action);
        }

        /*
            Handles message 'WorkflowDesignerPane_ActionSelected'. 
            This message is sent when user is selecting an existing action or after addng a new action. 
        */
        private selectAction(action: model.ActionDTO) {
            console.log("Action selected: " + action.id);
            var originalId,
                actionId = action.id,
                canBypassActionLoading = false; // Whether we can avoid reloading an action from the backend

            if (this.$scope.current.action) {
                originalId = this.$scope.current.action.id;
            }

            // Save previously selected action (and associated entities)
            // If a new action has just been added, it will be saved. 
            var promise = this.ProcessBuilderService.saveCurrent(this.$scope.current);

            promise.then((result: model.ProcessBuilderState) => {

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
                    var actionChanged = action.id != originalId
                
                    // Determine if we need to load action from the db or we can just use 
                    // the one returned from the above saveCurrent operation.
                    canBypassActionLoading = idChangedFromTempToPermanent || !actionChanged
                }
                
                if (actionId < 1) {
                    throw Error('Action has not been persisted. Process Builder cannot proceed ' +
                        'to action type selection for an unpersisted action.');
                }
                if (canBypassActionLoading) {
                    this.$scope.current.action = result.action;
                    var actions = this.$scope.actionGroups[0].actions
                    actions[actions.length - 1] = result.action;
                }
                else {
                    this.ActionService.get({ id: actionId }).$promise.then(action => {
                        this.$scope.current.action = action;
                    });
                }
            });
        }

        /*
            Handles message 'WorkflowDesignerPane_TemplateSelected'
        */
        private PaneWorkflowDesigner_TemplateSelected(eventArgs: pwd.TemplateSelectedEventArgs) {
            console.log("ProcessBuilderController: template selected");

            var scope = this.$scope,
                that = this;

            this.ProcessBuilderService.saveCurrent(this.$scope.current)
                .then((result: model.ProcessBuilderState) => {
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
            var pwdEventArs = new pwd.UpdateActivityTemplateIdEventArgs(eventArgs.action.id, eventArgs.action.activityTemplateId);
            this.$scope.$broadcast(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_UpdateActivityTemplateId], pwdEventArs);
        }

        /*
           Handles message 'SelectActionPane_InitiateSaveAction'
       */
        private PaneSelectAction_InitiateSaveAction(eventArgs: psa.ActionTypeSelectedEventArgs) {
            var promise = this.ProcessBuilderService.saveCurrent(this.$scope.current);
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

        private PaneConfigureAction_InternalAuthentication(eventArgs: pca.InternalAuthenticationArgs) {
            var self = this;

            var modalScope = <any>this.$scope.$new(true);
            modalScope.activityTemplateId = eventArgs.activityTemplateId;

            this.$modal.open({
                animation: true,
                templateUrl: 'AngularTemplate/InternalAuthentication',
                controller: 'InternalAuthenticationController',
                scope: modalScope
            })
            .result
            .then(function () {
                self.$scope.$broadcast(
                    pca.MessageType[pca.MessageType.PaneConfigureAction_Reconfigure]
                );
            });
        }

        private PaneConfigureAction_ExternalAuthentication(
            eventArgs: pca.ExternalAuthenticationArgs) {

            var self = this;

            var messageListener = function (event) {
                debugger;

                if (!self.$scope || !event.data || event.data != 'external-auth-success') {
                    return;
                }

                self.$scope.$broadcast(pca.MessageType[pca.MessageType.PaneConfigureAction_Reconfigure]);
            };

            this.$http
                .get('/actions/auth_url?id=' + eventArgs.activityTemplateId)
                .then(function (res) {
                    var url = (<any>res.data).url;

                    var childWindow = self.$window.open(url, 'AuthWindow', 'width=400, height=500, location=no, status=no');

                    window.addEventListener('message', messageListener);

                    var isClosedHandler = function () {
                        if (childWindow.closed) {
                            window.removeEventListener('message', messageListener);
                        }
                        else {
                            setTimeout(isClosedHandler, 500);
                        }
                    };
                    
                    setTimeout(isClosedHandler, 500);
                });
        }
    }

    app.run([
        "$httpBackend", "urlPrefix", ($httpBackend, urlPrefix) => {
            var actions: interfaces.IActionDTO =
                {
                    name: "test action type",
                    configurationControls: new model.ControlsList(),
                    crateStorage: new model.CrateStorage(),
                    parentRouteNodeId: 1,
                    activityTemplateId: 1,
                    id: 1,
                    isTempId: false,
                    actionListId: 0
                };

            $httpBackend
                .whenGET(urlPrefix + "/Action/1")
                .respond(actions);

            $httpBackend
                .whenPOST(urlPrefix + "/Action/1")
                .respond(function (method, url, data) {
                    return data;
                });
        }
    ]);

    app.controller('ProcessBuilderController', ProcessBuilderController);
} 