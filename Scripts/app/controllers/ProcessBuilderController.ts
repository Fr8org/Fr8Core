/// <reference path="../_all.ts" />

/*
    Detail (view/add/edit) controller
*/

module dockyard.controllers {
    'use strict';

    export interface IProcessBuilderScope extends ng.IScope {
        processTemplateId: number;
        processNodeTemplates: Array<model.ProcessNodeTemplateDTO>;
        fields: Array<model.Field>;
        immediateActionListVM: interfaces.IActionListVM;
        // Identity of currently edited processNodeTemplate.
        //curNodeId: number;
        //// Flag, that indicates if currently edited processNodeTemplate has temporary identity.
        //curNodeIsTempId: boolean;
        current: model.ProcessBuilderState;

        //demo
        textAreaField: model.TextAreaField;
    }

    //Setup aliases
    import pwd = dockyard.directives.paneWorkflowDesigner;
    import psa = dockyard.directives.paneSelectAction;
    import pca = dockyard.directives.paneConfigureAction;

    class ProcessBuilderController {
        // $inject annotation.
        // It provides $injector with information about dependencies to be injected into constructor
        // it is better to have it close to the constructor, because the parameters must match in count and type.
        // See http://docs.angularjs.org/guide/di


        public static $inject = [
            '$rootScope',
            '$scope',
            'StringService',
            'LocalIdentityGenerator',
            '$state',
            'ActionService',
            '$q',
            '$http',
            'urlPrefix',
            'ProcessTemplateService',
            '$timeout',
            'ProcessBuilderService',
            'CrateHelper',
            'ActivityTemplateService',
            '$filter',
            '$modal'
        ];

        private _scope: IProcessBuilderScope;

        constructor(
            private $rootScope: interfaces.IAppRootScope,
            private $scope: IProcessBuilderScope,
            private StringService: services.IStringService,
            private LocalIdentityGenerator: services.ILocalIdentityGenerator,
            private $state: ng.ui.IState,
            private ActionService: services.IActionService,
            private $q: ng.IQService,
            private $http: ng.IHttpService,
            private urlPrefix: string,
            private ProcessTemplateService: services.IProcessTemplateService,
            private $timeout: ng.ITimeoutService,
            private ProcessBuilderService: services.IProcessBuilderService,
            private CrateHelper: services.CrateHelper,
            private ActivityTemplateService: services.IActivityTemplateService,
            private $filter: ng.IFilterService,
            private $modal
            ) {
            this._scope = $scope;
            this._scope.processTemplateId = $state.params.id;
            this._scope.processNodeTemplates = [];
            this._scope.fields = [];
            this._scope.current = new model.ProcessBuilderState();

            this.setupMessageProcessing();
            this.loadProcessTemplate();
            var self = this;

            var textAreaField = new model.TextAreaField();
            textAreaField.value = '<h1>testt</h1>';
            this._scope.textAreaField = textAreaField;
        }

        /*
            Mapping of incoming messages to handlers
        */
        private setupMessageProcessing() {

            //Process Designer Pane events
            this._scope.$on(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ActionAdding],
                (event: ng.IAngularEvent, eventArgs: pwd.ActionAddingEventArgs) => this.PaneWorkflowDesigner_ActionAdding(eventArgs));
            this._scope.$on(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ActionSelected],
                (event: ng.IAngularEvent, eventArgs: pwd.ActionSelectedEventArgs) => this.PaneWorkflowDesigner_ActionSelected(eventArgs));
            this._scope.$on(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_TemplateSelected],
                (event: ng.IAngularEvent, eventArgs: pwd.TemplateSelectedEventArgs) => this.PaneWorkflowDesigner_TemplateSelected(eventArgs));

            //Process Configure Action Pane events
            this._scope.$on(pca.MessageType[pca.MessageType.PaneConfigureAction_ActionUpdated],
                (event: ng.IAngularEvent, eventArgs: pca.ActionUpdatedEventArgs) => this.PaneConfigureAction_ActionUpdated(eventArgs));
            this._scope.$on(pca.MessageType[pca.MessageType.PaneConfigureAction_ActionRemoved],
                (event: ng.IAngularEvent, eventArgs: pca.ActionRemovedEventArgs) => this.PaneConfigureAction_ActionRemoved(eventArgs));
            this._scope.$on(pca.MessageType[pca.MessageType.PaneConfigureAction_InternalAuthentication],
                (event: ng.IAngularEvent, eventArgs: pca.InternalAuthenticationArgs) => this.PaneConfigureAction_InternalAuthentication(eventArgs));

            //Process Select Action Pane events
            this._scope.$on(psa.MessageType[psa.MessageType.PaneSelectAction_ActionTypeSelected],
                (event: ng.IAngularEvent, eventArgs: psa.ActionTypeSelectedEventArgs) => this.PaneSelectAction_ActionTypeSelected(eventArgs));
            // TODO: do we need this any more?
            // this._scope.$on(psa.MessageType[psa.MessageType.PaneSelectAction_ActionUpdated],
            //     (event: ng.IAngularEvent, eventArgs: psa.ActionUpdatedEventArgs) => this.PaneSelectAction_ActionUpdated(eventArgs));
            //Handles Save Request From PaneSelectAction
            this._scope.$on(psa.MessageType[psa.MessageType.PaneSelectAction_InitiateSaveAction],
                (event: ng.IAngularEvent, eventArgs: psa.ActionTypeSelectedEventArgs) => this.PaneSelectAction_InitiateSaveAction(eventArgs));
        }

        private loadProcessTemplate() {
            var self = this;
            var processTemplatePromise = this.ProcessTemplateService.getFull({ id: this._scope.processTemplateId });

            processTemplatePromise.$promise.then((curProcessTemplate: interfaces.IProcessTemplateVM) => {
                self._scope.current.processTemplate = curProcessTemplate;
                var actionLists = curProcessTemplate.processNodeTemplates[0].actionLists
                self._scope.immediateActionListVM = self.$filter('filter')(actionLists, { actionListType: 1 }, true)[0]

                self.renderProcessTemplate(curProcessTemplate);
            });
        }

        private renderProcessTemplate(curProcessTemplate: interfaces.IProcessTemplateVM) {
            if (curProcessTemplate.processNodeTemplates.length == 0) return;

            for (var curProcessNodeTemplate of curProcessTemplate.processNodeTemplates);
            {
                for (var curActionList of curProcessNodeTemplate.actionLists) {
                    for (var curAction of curActionList.actions) {
                        this._scope.$broadcast(
                            pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_AddAction],
                            new pwd.AddActionEventArgs(curAction.processNodeTemplateId, angular.extend({}, curAction), curActionList.actionListType) //TODO: set real action type
                        );
        }
                }
            }
        }

        // If action updated, notify interested parties and update $scope.current.action
        private handleActionUpdate(action: model.ActionDTO) {
            if (!action) return;

            if (this._scope.current.action.isTempId) {
                this._scope.$broadcast(
                    pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ActionTempIdReplaced],
                    new pwd.ActionTempIdReplacedEventArgs(this._scope.current.action.id, action.id)
                    );
            }

                this._scope.current.action = action;
                //self._scope.current.action.id = result.action.id;
                //self._scope.current.action.isTempId = false;

                //Notify workflow designer of action update
                this._scope.$broadcast(
                    pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ActionNameUpdated],
                    new pwd.ActionNameUpdatedEventArgs(action.id, action.name)
                    );

                if (this.CrateHelper.hasControlListCrate(action.crateStorage)) {
                    action.configurationControls = this.CrateHelper
                        .createControlListFromCrateStorage(action.crateStorage);
                }
            }

        /*
            Handles message 'PaneWorkflowDesigner_ActionAdding'
        */
        private PaneWorkflowDesigner_ActionAdding(eventArgs: pwd.ActionAddingEventArgs) {
            console.log('ProcessBuilderController::PaneWorkflowDesigner_ActionAdding', eventArgs);
            var processNodeTemplateId: number,
                id: number;

            var self = this;

            var promise = this.ProcessBuilderService.saveCurrent(this._scope.current);
            promise.then((result: model.ProcessBuilderState) => {
                // Generate next Id.
                var id = self.LocalIdentityGenerator.getNextId();                

                // Create new action object.
                var action = new model.ActionDTO(null, id, true, self._scope.immediateActionListVM.id);
                action.name = 'New Action #' + Math.abs(id).toString();

                // Add action to Workflow Designer.
                self._scope.current.action = action.toActionVM();
                self._scope.$broadcast(
                    pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_AddAction],
                    new pwd.AddActionEventArgs(action.processNodeTemplateId, action.clone(), eventArgs.actionListType)
                    );

                this.$modal.open({
                    animation: true,
                    templateUrl: 'AngularTemplate/PaneSelectAction',
                    controller: 'PaneSelectActionController',
                    windowClass: 'select-action-modal'
                }).result.then(function (data: model.ActivityTemplate) {
                    self._scope.current.action.activityTemplateId = data.id;
                    self._scope.current.action.activityTemplate = data;
                    var pcaEventArgs = new pca.RenderEventArgs(self._scope.current.action);
                    var pwdEventArs = new pwd.UpdateActivityTemplateIdEventArgs(self._scope.current.action.id, data.id);
                    self._scope.$broadcast(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_UpdateActivityTemplateId], pwdEventArs);
                    self._scope.$broadcast(pca.MessageType[pca.MessageType.PaneConfigureAction_Render], pcaEventArgs);                    
                });
            });
        }

        /*
            Handles message 'WorkflowDesignerPane_ActionSelected'. 
            This message is sent when user is selecting an existing action or after addng a new action. 
        */
        private PaneWorkflowDesigner_ActionSelected(eventArgs: pwd.ActionSelectedEventArgs) {
            console.log("Action selected: " + eventArgs.actionId);
            var originalId,
                actionId = eventArgs.actionId,
                canBypassActionLoading = false; // Whether we can avoid reloading an action from the backend

            if (this._scope.current.action) {
                originalId = this._scope.current.action.id;
            }

            // Save previously selected action (and associated entities)
            // If a new action has just been added, it will be saved. 
            var promise = this.ProcessBuilderService.saveCurrent(this._scope.current);

            promise.then((result: model.ProcessBuilderState) => {
                //Assume that Criteria is persisted so we always have a permanent id)
                this._scope.current.criteria = new model.CriteriaDTO(
                    eventArgs.processNodeTemplateId,
                    false,
                    eventArgs.processNodeTemplateId,
                    model.CriteriaExecutionType.NoSet
                );

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
                        : eventArgs.actionId;

                    //Whether user selected a new action or just clicked on the current one
                    var actionChanged = eventArgs.actionId != originalId
                
                    // Determine if we need to load action from the db or we can just use 
                    // the one returned from the above saveCurrent operation.
                    var canBypassActionLoading = idChangedFromTempToPermanent || !actionChanged
                }
                
                //if (this._scope.current.action != null) {
                //    this._scope.$broadcast(
                //        pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ActionNameUpdated],
                //        new pwd.ActionNameUpdatedEventArgs(this._scope.current.action.id, this._scope.current.action.name)
                //        );
                //}

                if (actionId < 1) {
                    throw Error('Action has not been persisted. Process Builder cannot proceed ' +
                        'to action type selection for an unpersisted action.');
                }
                if (canBypassActionLoading) {
                    this._scope.current.action = result.action;
                    this.visualizeCurrentAction();
                }
                else {
                    this.ActionService.get({ id: actionId }).$promise.then(action => {
                        this._scope.current.action = action;
                        this.visualizeCurrentAction();
                    });
                }
            });
        }

        private visualizeCurrentAction() {
            var eArgs;

            if (this._scope.current.action.activityTemplateId == null
                || this._scope.current.action.activityTemplateId === 0) {
                
                //Render Select Action Pane
                eArgs = new psa.RenderEventArgs(
                    this._scope.current.action.processNodeTemplateId,
                    this._scope.current.action.id,
                    (this._scope.current.action.id > 0) ? false : true,
                    this._scope.current.action.actionListId);
                this._scope.$broadcast(pca.MessageType[pca.MessageType.PaneConfigureAction_Hide]);
            }
            else {

                //Render Configure Action Pane
                eArgs = new pca.RenderEventArgs(new model.ActionDTO(
                    this._scope.current.action.processNodeTemplateId,
                    this._scope.current.action.id,
                    (this._scope.current.action.id > 0) ? false : true,
                    this._scope.current.action.actionListId));
                this._scope.$broadcast(pca.MessageType[pca.MessageType.PaneConfigureAction_Render], eArgs);
            }
        }

        /*
            Handles message 'WorkflowDesignerPane_TemplateSelected'
        */
        private PaneWorkflowDesigner_TemplateSelected(eventArgs: pwd.TemplateSelectedEventArgs) {
            console.log("ProcessBuilderController: template selected");

            var scope = this._scope,
                that = this;

            this.ProcessBuilderService.saveCurrent(this._scope.current)
                .then((result: model.ProcessBuilderState) => {
                // Notity interested parties of action update and update $scope
                this.handleActionUpdate(result.action);

                //Hide Configure Action Pane
                scope.$broadcast(pca.MessageType[pca.MessageType.PaneConfigureAction_Hide]);
            });
        }

        /*
            Handles message 'ConfigureActionPane_ActionUpdated'
        */
        private PaneConfigureAction_ActionUpdated(eventArgs: pca.ActionUpdatedEventArgs) {
            // Force update on Select Action Pane (FOR DEMO ONLY, NOT IN DESIGN DOCUMENT)
            var psaArgs = new psa.UpdateActionEventArgs(
                eventArgs.criteriaId,
                eventArgs.actionId,
                eventArgs.isTempId);

            this._scope.$broadcast(
                psa.MessageType[psa.MessageType.PaneSelectAction_UpdateAction],
                psaArgs);
        }

        /*
            Handles message 'SelectActionPane_ActionTypeSelected'
        */
        private PaneSelectAction_ActionTypeSelected(eventArgs: psa.ActionTypeSelectedEventArgs) {
            var pcaEventArgs = new pca.RenderEventArgs(eventArgs.action);
            var pwdEventArs = new pwd.UpdateActivityTemplateIdEventArgs(eventArgs.action.id, eventArgs.action.activityTemplateId);
            this._scope.$broadcast(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_UpdateActivityTemplateId], pwdEventArs);
            this._scope.$broadcast(pca.MessageType[pca.MessageType.PaneConfigureAction_Render], pcaEventArgs);
        }

        /*
           Handles message 'SelectActionPane_InitiateSaveAction'
       */
        private PaneSelectAction_InitiateSaveAction(eventArgs: psa.ActionTypeSelectedEventArgs) {
            var promise = this.ProcessBuilderService.saveCurrent(this._scope.current);
        }

        /*
            Handles message 'PaneSelectAction_ActionRemoved'
        */
        private PaneConfigureAction_ActionRemoved(eventArgs: pca.ActionRemovedEventArgs) {
            this._scope.$broadcast(psa.MessageType[psa.MessageType.PaneSelectAction_Hide]);
            this._scope.$broadcast(
                pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ActionRemoved],
                new pwd.ActionRemovedEventArgs(eventArgs.id, eventArgs.isTempId)
                );
        }

        private PaneConfigureAction_InternalAuthentication(eventArgs: pca.InternalAuthenticationArgs) {
            var self = this;

            var modalScope = <any>this.$rootScope.$new(true);
            modalScope.activityTemplateId = eventArgs.activityTemplateId;

            this.$modal.open({
                animation: true,
                templateUrl: 'AngularTemplate/InternalAuthentication',
                controller: 'InternalAuthenticationController',
                scope: modalScope
            })
            .result
            .then(function () {
                var pcaEventArgs = new pca.RenderEventArgs(self._scope.current.action);
                self._scope.$broadcast(pca.MessageType[pca.MessageType.PaneConfigureAction_Render], pcaEventArgs);
            });
        }

        private HideActionPanes() {
            //Hide Configure Action Pane
            this._scope.$broadcast(pca.MessageType[pca.MessageType.PaneConfigureAction_Hide]);
        }
    }

    app.run([
        "$httpBackend", "urlPrefix", ($httpBackend, urlPrefix) => {
            var actions: interfaces.IActionDTO =
                {
                    name: "test action type",
                    configurationControls: new model.ControlsList(),
                    crateStorage: new model.CrateStorage(),
                    processNodeTemplateId: 1,
                    activityTemplateId: 1,
                    id: 1,
                    isTempId: false,
                    actionListId: 0,
                    activityTemplate: new model.ActivityTemplate(1, "Write to SQL", "1", "", "")
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