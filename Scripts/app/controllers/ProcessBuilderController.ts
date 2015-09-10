/// <reference path="../_all.ts" />

/*
    Detail (view/add/edit) controller
*/

module dockyard.controllers {
    'use strict';

    export interface IProcessBuilderScope extends ng.IScope {
        processTemplateId: number;
        processNodeTemplates: Array<model.ProcessNodeTemplateDTO>,
        fields: Array<model.Field>;

        // Identity of currently edited processNodeTemplate.
        //curNodeId: number;
        //// Flag, that indicates if currently edited processNodeTemplate has temporary identity.
        //curNodeIsTempId: boolean;
        current: model.ProcessBuilderState,
        save: Function;
        cancel: Function;
    }

    //Setup aliases
    import pwd = dockyard.directives.paneWorkflowDesigner;
    import pdc = dockyard.directives.paneDefineCriteria;
    import psa = dockyard.directives.paneSelectAction;
    import pca = dockyard.directives.paneConfigureAction;
    import pst = dockyard.directives.paneSelectTemplate;
    import pcm = dockyard.directives.paneConfigureMapping;

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
            'CriteriaServiceWrapper',
            'ProcessBuilderService',
            'ActionListService'
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
            private CriteriaServiceWrapper: services.ICriteriaServiceWrapper,
            private ProcessBuilderService: services.IProcessBuilderService,
            private ActionListService: services.IActionListService
            ) {
            this._scope = $scope;
            this._scope.processTemplateId = $state.params.id;


            this._scope.processNodeTemplates = [];
            this._scope.fields = [];
            this._scope.current = new model.ProcessBuilderState();

            this._scope.cancel = angular.bind(this, this.Cancel);
            this._scope.save = angular.bind(this, this.onSave);

            this.setupMessageProcessing();
            this.loadProcessTemplate();
        }

        /*
            Mapping of incoming messages to handlers
        */
        private setupMessageProcessing() {

            //Process Designer Pane events
            this._scope.$on(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ProcessNodeTemplateAdding],
                (event: ng.IAngularEvent, eventArgs: pwd.CriteriaAddingEventArgs) => this.PaneWorkflowDesigner_CriteriaAdding(eventArgs));
            this._scope.$on(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_CriteriaSelected],
                (event: ng.IAngularEvent, eventArgs: pwd.CriteriaSelectedEventArgs) => this.PaneWorkflowDesigner_CriteriaSelected(eventArgs));
            this._scope.$on(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ActionAdding],
                (event: ng.IAngularEvent, eventArgs: pwd.ActionAddingEventArgs) => this.PaneWorkflowDesigner_ActionAdding(eventArgs));
            this._scope.$on(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ActionSelected],
                (event: ng.IAngularEvent, eventArgs: pwd.ActionSelectedEventArgs) => this.PaneWorkflowDesigner_ActionSelected(eventArgs));
            this._scope.$on(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_TemplateSelected],
                (event: ng.IAngularEvent, eventArgs: pwd.TemplateSelectedEventArgs) => this.PaneWorkflowDesigner_TemplateSelecting(eventArgs));

            //Define Criteria Pane events
            this._scope.$on(pdc.MessageType[pdc.MessageType.PaneDefineCriteria_ProcessNodeTemplateRemoving],
                (event: ng.IAngularEvent, eventArgs: pdc.ProcessNodeTemplateRemovingEventArgs) => this.PaneDefineCriteria_ProcessNodeTemplateRemoving(eventArgs));
            this._scope.$on(pdc.MessageType[pdc.MessageType.PaneDefineCriteria_Cancelling],
                (event: ng.IAngularEvent) => this.PaneDefineCriteria_Cancelled());
            this._scope.$on(pdc.MessageType[pdc.MessageType.PaneDefineCriteria_ProcessNodeTemplateUpdated],
                (event: ng.IAngularEvent, eventArgs: pdc.ProcessNodeTemplateUpdatedEventArgs) => this.PaneDefineCriteria_ProcessNodeTemplateUpdated(eventArgs));

            //Process Configure Action Pane events
            this._scope.$on(pca.MessageType[pca.MessageType.PaneConfigureAction_ActionUpdated],
                (event: ng.IAngularEvent, eventArgs: pca.ActionUpdatedEventArgs) => this.PaneConfigureAction_ActionUpdated(eventArgs));
            this._scope.$on(pca.MessageType[pca.MessageType.PaneConfigureAction_MapFieldsClicked],
                (event: ng.IAngularEvent, eventArgs: pca.MapFieldsClickedEventArgs) => this.PaneConfigureAction_MapFieldsClicked(eventArgs));

            //Select Template Pane events
            this._scope.$on(pst.MessageType[pst.MessageType.PaneSelectTemplate_ProcessTemplateUpdated],
                (event: ng.IAngularEvent, eventArgs: pst.ProcessTemplateUpdatedEventArgs) => {
                    //avoid infinite loop if the message was sent by this controller
                    if (event.currentScope == event.targetScope) return;
                    this.PaneSelectTemplate_ProcessTemplateUpdated(eventArgs);
                });

            //Process Select Action Pane events
            this._scope.$on(psa.MessageType[psa.MessageType.PaneSelectAction_ActionTypeSelected],
                (event: ng.IAngularEvent, eventArgs: psa.ActionTypeSelectedEventArgs) => this.PaneSelectAction_ActionTypeSelected(eventArgs));
            // TODO: do we need this any more?
            // this._scope.$on(psa.MessageType[psa.MessageType.PaneSelectAction_ActionUpdated],
            //     (event: ng.IAngularEvent, eventArgs: psa.ActionUpdatedEventArgs) => this.PaneSelectAction_ActionUpdated(eventArgs));
            this._scope.$on(psa.MessageType[psa.MessageType.PaneSelectAction_ActionRemoved],
                (event: ng.IAngularEvent, eventArgs: psa.ActionRemovedEventArgs) => this.PaneSelectAction_ActionRemoved(eventArgs));
        }

        private loadProcessTemplate() {
            var processTemplatePromise = this.ProcessTemplateService.get({ id: this._scope.processTemplateId });
            processTemplatePromise.$promise.then(() => this.displaySelectTemplatePane());
            this._scope.current.processTemplate = processTemplatePromise;
        }
         
        // Find criteria by Id.
        private findCriteria(id: number): model.ProcessNodeTemplateDTO {
            var i;
            for (i = 0; i < this._scope.processNodeTemplates.length; ++i) {
                if (this._scope.processNodeTemplates[i].id === id) {
                    return this._scope.processNodeTemplates[i];
                }
            }
            return null;
        }

        // If action updated, notify interested parties
        private handleActionUpdate(action: model.ActionDesignDTO) {

            if (!action) return;

            //TODO: Check the presense of new id
            if (this._scope.current.action.isTempId) {
                this._scope.$broadcast(
                    pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ActionTempIdReplaced],
                    new pwd.ActionTempIdReplacedEventArgs(this._scope.current.action.id, action.id)
                    );

                // If a new action created, set new id
                if (action) {
                    this._scope.current.action = action;
                    //self._scope.current.action.id = result.action.id;
                    //self._scope.current.action.isTempId = false;

                    //Notify workflow designer of action update
                    this._scope.$broadcast(
                        pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ActionNameUpdated],
                        new pwd.ActionNameUpdatedEventArgs(action.id, action.name)
                        );
                }
            }
        }

        private displaySelectTemplatePane() {
            //Show Select Template Pane
            var eArgs = new directives.paneSelectTemplate.RenderEventArgs();
            this._scope.$broadcast(pst.MessageType[pst.MessageType.PaneSelectTemplate_Render]);
        }

        /*
            Handles message 'PaneDefineCriteria_ProcessNodeTemplateUpdating'
        */
        private PaneDefineCriteria_ProcessNodeTemplateUpdated(eventArgs: pdc.ProcessNodeTemplateUpdatedEventArgs) {
            console.log('ProcessBuilderController::PaneDefineCriteria_ProcessNodeTemplateUpdating', eventArgs);

            if (eventArgs.processNodeTemplateTempId) {
                this._scope.$broadcast(
                    pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ReplaceTempIdForProcessNodeTemplate],
                    new pwd.ReplaceTempIdForProcessNodeTemplateEventArgs(eventArgs.processNodeTemplateTempId, eventArgs.processNodeTemplateId)
                    );
            }

            this._scope.$broadcast(
                pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_UpdateProcessNodeTemplateName],
                new pwd.UpdateProcessNodeTemplateNameEventArgs(eventArgs.processNodeTemplateId, eventArgs.name)
                );
        }

        /*
            Handles message 'PaneDefineCriteria_CriteriaRemoving'
        */
        private PaneDefineCriteria_ProcessNodeTemplateRemoving(eventArgs: pdc.ProcessNodeTemplateRemovingEventArgs) {
            console.log('ProcessBuilderController::PaneDefineCriteria_ProcessNodeTemplateRemoving', eventArgs);

            // Tell Workflow Designer to remove criteria.
            this._scope.$broadcast(
                pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_RemoveCriteria],
                new pwd.RemoveCriteriaEventArgs(eventArgs.processNodeTemplateId, eventArgs.isTempId)
                );

            // Hide Define Criteria pane.
            this._scope.$broadcast(pdc.MessageType[pdc.MessageType.PaneDefineCriteria_Hide]);
        }

        /*
            Handles message 'PaneDefineCriteria_Cancelled'
        */
        private PaneDefineCriteria_Cancelled() {
            console.log('ProcessBuilderController::PaneDefineCriteria_Cancelled');

            // If user worked with temporary (not saved criteria), remove criteria from Workflow Designer.
            if (this._scope.current.processNodeTemplate)
                this._scope.$broadcast(
                    pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_RemoveCriteria],
                    new pwd.RemoveCriteriaEventArgs(
                        this._scope.current.processNodeTemplate.id,
                        this._scope.current.processNodeTemplate.isTempId)
                    );        

            // Hide DefineCriteria pane.
            this._scope.$broadcast(
                pdc.MessageType[pdc.MessageType.PaneDefineCriteria_Hide]
                );

            // Set currentCriteria to null, marking that no criteria is currently selected.
            this._scope.current.processNodeTemplate = null;
        }

        //private saveCriteria(): ng.IPromise<model.ProcessBuilderState> {
        //    var self = this;

        //    //Save currently selected Criteria (if any) and add a new one
        //    var promise = this.ProcessBuilderService.saveCurrent(this._scope.current);
        //    promise.then((result: model.ProcessBuilderState) => {
        //        self._scope.current.criteria = result.criteria;
        //        this._scope.current.processNodeTemplate = null;
        //    });
        //    return promise;
        //}


        /*
            Handles message 'WorkflowDesignerPane_CriteriaAdding'
        */
        private PaneWorkflowDesigner_CriteriaAdding(eventArgs: pwd.CriteriaAddingEventArgs) {
            console.log('ProcessBuilderController::PaneWorkflowDesigner_CriteriaAdding', eventArgs);

            //Save currently selected Criteria (if any) and add a new one
            var promise = this.ProcessBuilderService.saveCurrent(this._scope.current);
            promise.then((result: model.ProcessBuilderState) => {
                this._scope.current.criteria = null;
                this._scope.current.processNodeTemplate = null;

                // Have Workflow Designer add newly created criteria.
                this._scope.$broadcast(
                    pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_AddCriteria],
                    new pwd.AddProcessNodeTemplateEventArgs(this.LocalIdentityGenerator.getNextId(), true, 'New criteria')
                );
            });
        }

        /*
            Handles message 'WorkflowDesignerPane_CriteriaSelected'
        */
        private PaneWorkflowDesigner_CriteriaSelected(eventArgs: pwd.CriteriaSelectedEventArgs) {
            console.log("ProcessBuilderController::PaneWorkflowDesigner_CriteriaSelected", eventArgs);
            this._scope.current.criteria = new model.CriteriaDTO(eventArgs.id, eventArgs.isTempId, 0, model.CriteriaExecutionType.NoSet);

            // TODO: Do not react on clicking on the currently visible Criteria
            var promise = this.ProcessBuilderService.saveCurrent(this._scope.current);
            promise.then((result: model.ProcessBuilderState) => {
                debugger;

                // Notity interested parties of action update event and update $scope
                this.handleActionUpdate(result.action);

                this._scope.current.action = null; // the prev action is apparently unselected

                // Hide Select Template Pane
                this._scope.$broadcast(pst.MessageType[pst.MessageType.PaneSelectTemplate_Hide]);

                // Hide Define Criteria Pane (another Criteria might have been opened there)
                // Note: Hide triggers Save
                this._scope.$broadcast(
                    pdc.MessageType[pdc.MessageType.PaneDefineCriteria_Hide],
                    new pdc.HideEventArgs(this._scope.processTemplateId, eventArgs.id, eventArgs.isTempId)
                    );

                // Hide Select Action Pane
                this._scope.$broadcast(psa.MessageType[psa.MessageType.PaneSelectAction_Hide]);
                
                // Hide Configure Action Pane
                this._scope.$broadcast(pca.MessageType[pca.MessageType.PaneConfigureAction_Hide]);
                    
                // Show Define Criteria Pane
                this._scope.$broadcast(
                    pdc.MessageType[pdc.MessageType.PaneDefineCriteria_Render],
                    new pdc.RenderEventArgs(this._scope.fields, this._scope.processTemplateId, eventArgs.id, eventArgs.isTempId)
                    );
            });
        }

        /*
            Handles message 'PaneWorkflowDesigner_ActionAdding'
        */
        private PaneWorkflowDesigner_ActionAdding(eventArgs: pwd.ActionAddingEventArgs) {
            console.log('ProcessBuilderController::PaneWorkflowDesigner_ActionAdding', eventArgs);

            var processNodeTemplateId: number,
                id: number;

            ////Need this check to happen on top since saveProcessNodeTemplate() changes currentCriteria value
            //if (this._scope.currentCriteria && (this._scope.currentCriteria.id != eventArgs.processNodeTemplateId)  ) {
            //    this._scope.$broadcast(pdc.MessageType[pdc.MessageType.PaneDefineCriteria_Hide]);
            //}
            var self = this;

            // Make sure that current Action is null to prevent an action save 
            // request from being unnecessarily sent to web api 
            this._scope.current.action = null;            

            // TODO: Do not react on clicking on currently visible Criteria
            var promise = this.ProcessBuilderService.saveCurrent(this._scope.current);
            promise.then((result: model.ProcessBuilderState) => {

                // Generate next Id.
                var id = self.LocalIdentityGenerator.getNextId();
                                
                // Create new action object.
                var action = new model.ActionDesignDTO(null, id, true, null);

                action.name = 'New Action #' + Math.abs(id).toString();

                // Add action to Workflow Designer.
                self._scope.current.action = action.toActionVM();
                self._scope.$broadcast(
                    pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_AddAction],
                    new pwd.AddActionEventArgs(action.processNodeTemplateId, action.clone(), eventArgs.actionListType)
                    );                
            });
        }

        /*
            Handles message 'WorkflowDesignerPane_ActionSelected'
        */
        private PaneWorkflowDesigner_ActionSelected(eventArgs: pwd.ActionSelectedEventArgs) {
            console.log("ProcessBuilderController: action selected", eventArgs);

            debugger;
            var originalId;
            if (this._scope.current.action) {
                originalId = this._scope.current.action.id;
            }
            var promise = this.ProcessBuilderService.saveCurrent(this._scope.current);
            promise.then((result: model.ProcessBuilderState) => {

                //Assume that Criteria is persisted so we always have a permanent id)
                this._scope.current.criteria = new model.CriteriaDTO(
                    eventArgs.processNodeTemplateId,
                    false,
                    eventArgs.processNodeTemplateId,
                    model.CriteriaExecutionType.NoSet
                    );

                // Notity interested parties of action update and update $scope
                this.handleActionUpdate(result.action[0]);

                //if (this._scope.current.action != null) {
                //    this._scope.$broadcast(
                //        pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ActionNameUpdated],
                //        new pwd.ActionNameUpdatedEventArgs(this._scope.current.action.id, this._scope.current.action.name)
                //        );
                //}

                var wasTemporaryAction = (originalId == eventArgs.actionId);

                var actionId = wasTemporaryAction && result.action[0]
                    ? result.action[0].id
                    : eventArgs.actionId;

                this._scope.current.action = this.ActionService.get({ id: actionId });

                //Render Select Action Pane
                var eArgs = new psa.RenderEventArgs(
                    eventArgs.processNodeTemplateId,
                    actionId,
                    false,
                    eventArgs.actionListId);

                this._scope.$broadcast(pst.MessageType[pst.MessageType.PaneSelectTemplate_Hide]);
                this._scope.$broadcast(pdc.MessageType[pdc.MessageType.PaneDefineCriteria_Hide]);
                this._scope.$broadcast(
                    psa.MessageType[psa.MessageType.PaneSelectAction_Render],
                    eArgs);
            });
        }

        /*
            Handles message 'WorkflowDesignerPane_TemplateSelected'
        */
        private PaneWorkflowDesigner_TemplateSelecting(eventArgs: pwd.TemplateSelectedEventArgs) {
            console.log("ProcessBuilderController: template selected");

            var scope = this._scope,
                that = this;

            var promise = this.ProcessBuilderService.saveCurrent(this._scope.current);
            promise.then((result: model.ProcessBuilderState) => {

                // Notity interested parties of action update and update $scope
                this.handleActionUpdate(result.action);

                //Hide Define Criteria Pane
                scope.$broadcast(pdc.MessageType[pdc.MessageType.PaneDefineCriteria_Hide]);

                //Hide Select Action Pane
                scope.$broadcast(psa.MessageType[psa.MessageType.PaneSelectAction_Hide]);
                
                //Hide Configure Action Pane
                scope.$broadcast(pca.MessageType[pca.MessageType.PaneConfigureAction_Hide]);

                //Show Select Template Pane
                that.displaySelectTemplatePane();
            });
        }

        /*
            Handles message 'PaneConfigureAction_MapFieldsClicked'
        */
        private PaneConfigureAction_MapFieldsClicked(eventArgs: pca.MapFieldsClickedEventArgs) {
            var scope = this._scope;

            var promise = this.ProcessBuilderService.saveCurrent(this._scope.current);
            promise.then((result: model.ProcessBuilderState) => {

                // Notity interested parties of action update and update $scope
                this.handleActionUpdate(result.action);

                //Render Pane Configure Mapping 
                var pcmEventArgs = new pcm.RenderEventArgs(
                    eventArgs.action.processNodeTemplateId,
                    eventArgs.action.id,
                    eventArgs.action.isTempId);
                scope.$broadcast(pcm.MessageType[pcm.MessageType.PaneConfigureMapping_Render], pcmEventArgs);
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
            // Render Pane Configure Action 
            var pcaEventArgs = new pca.RenderEventArgs(eventArgs.action);
            this._scope.$broadcast(pca.MessageType[pca.MessageType.PaneConfigureAction_Render], pcaEventArgs);
        }
         
        // TODO: do we need this?
        // /*
        //     Handles message 'PaneSelectAction_ActionUpdated'
        // */
        // private PaneSelectAction_ActionUpdated(eventArgs: psa.ActionUpdatedEventArgs) {
        //     //Update Pane Workflow Designer
        //     var eArgs = new pwd.ActionNameUpdatedEventArgs(
        //         eventArgs.actionId,
        //         eventArgs.actionName);
        //     this._scope.$broadcast(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ActionNameUpdated], eArgs);
        // }

        /*
            Handles message 'PaneSelectAction_ActionRemoved'
        */
        private PaneSelectAction_ActionRemoved(eventArgs: psa.ActionRemovedEventArgs) {
            this._scope.$broadcast(
                pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ActionRemoved],
                new pwd.ActionRemovedEventArgs(eventArgs.id, eventArgs.isTempId)
                );
        }

        private onSave() {
            var promise = this.ProcessBuilderService.saveCurrent(this._scope.current);
            promise.then((result: model.ProcessBuilderState) => {

                // Notity interested parties of action update and update $scope
                this.handleActionUpdate(result.action);

            });
        }

        private PaneSelectTemplate_ProcessTemplateUpdated(eventArgs: pst.ProcessTemplateUpdatedEventArgs) {
            console.log('changed');

            //Update scope variable
            var currentProcessTemplate = this._scope.current.processTemplate || <interfaces.IProcessTemplateVM>{}
            currentProcessTemplate.name = eventArgs.processTemplateName;
            currentProcessTemplate.subscribedDocuSignTemplates = eventArgs.subscribedDocuSignTemplates;

            //Relay the message to all other panes
            this._scope.$broadcast(
                pst.MessageType[pst.MessageType.PaneSelectTemplate_ProcessTemplateUpdated],
                new pst.ProcessTemplateUpdatedEventArgs(
                    this._scope.current.processTemplate.id,
                    this._scope.current.processTemplate.name,
                    this._scope.current.processTemplate.subscribedDocuSignTemplates)
                );
        }

        public Cancel() {
            this._scope.current.action = null;
            this.HideActionPanes();
        }

        private HideActionPanes() {
            //Hide Select Action Pane
            this._scope.$broadcast(psa.MessageType[psa.MessageType.PaneSelectAction_Hide]);

            //Hide Configure Mapping Pane
            this._scope.$broadcast(pcm.MessageType[pcm.MessageType.PaneConfigureMapping_Hide]);

            //Hide Configure Action Pane
            this._scope.$broadcast(pca.MessageType[pca.MessageType.PaneConfigureAction_Hide]);
        }
    }

    app.run([
        "$httpBackend", "urlPrefix", ($httpBackend, urlPrefix) => {
            var actions: interfaces.IActionDesignDTO =
                {
                    name: "test action type",
                    crateStorage: new model.CrateStorage(),
                    processNodeTemplateId: 1,
                    actionTemplateId: 1,
                    id: 1,
                    isTempId: false,
                    fieldMappingSettings: new model.FieldMappingSettings(),
                    userLabel: "test",
                    tempId: 0,
                    actionListId: 0,
                    actionTemplate: new model.ActionTemplate(1, "Write to SQL", "1")
                };

            $httpBackend
                .whenGET(urlPrefix + "/Action/1")
                .respond(actions);

            $httpBackend
                .whenPOST(urlPrefix + "/Action/1")
                .respond(function (method, url, data) {
                    return data;
                })
        }
    ]);

    app.controller('ProcessBuilderController', ProcessBuilderController);
} 