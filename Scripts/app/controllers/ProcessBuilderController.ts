/// <reference path="../_all.ts" />

/*
    Detail (view/add/edit) controller
*/
module dockyard.controllers {
    'use strict';

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
            'ProcessTemplateService'
        ];

        private _scope: interfaces.IProcessBuilderScope;

        constructor(
            private $rootScope: interfaces.IAppRootScope,
            private $scope: interfaces.IProcessBuilderScope,
            private StringService: services.IStringService,
            private LocalIdentityGenerator: services.ILocalIdentityGenerator,
            private $state: ng.ui.IState,
            private ActionService: services.IActionService,
            private $q: ng.IQService,
            private ProcessTemplateService: services.IProcessTemplateService
        ) {
            this._scope = $scope;
            this._scope.processTemplateId = $state.params.id;


            this._scope.processNodeTemplates = [];
            this._scope.fields = [
                new model.Field('envelope.name', '[Envelope].Name'),
                new model.Field('envelope.date', '[Envelope].Date')
            ];

            this._scope.curNodeId = null;
            this._scope.curNodeIsTempId = false;
            this._scope.Cancel = angular.bind(this, this.Cancel);
            this._scope.Save = angular.bind(this, this.SaveAction);

            this.setupMessageProcessing();
            this.loadProcessTemplate();
        }

        /*
            Mapping of incoming messages to handlers
        */
        private setupMessageProcessing() {

            //Process Designer Pane events
            this._scope.$on(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ProcessNodeTemplateAdding],
                (event: ng.IAngularEvent, eventArgs: pwd.ProcessNodeTemplateAddingEventArgs) => this.PaneWorkflowDesigner_ProcessNodeTemplateAdding(eventArgs));
            this._scope.$on(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ProcessNodeTemplateSelecting],
                (event: ng.IAngularEvent, eventArgs: pwd.ProcessNodeTemplateSelectingEventArgs) => this.PaneWorkflowDesigner_ProcessNodeTemplateSelecting(eventArgs));
            this._scope.$on(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ActionAdding],
                (event: ng.IAngularEvent, eventArgs: pwd.ActionAddingEventArgs) => this.PaneWorkflowDesigner_ActionAdding(eventArgs));
            this._scope.$on(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ActionSelecting],
                (event: ng.IAngularEvent, eventArgs: pwd.ActionSelectingEventArgs) => this.PaneWorkflowDesigner_ActionSelecting(eventArgs));
            this._scope.$on(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_TemplateSelecting],
                (event: ng.IAngularEvent, eventArgs: pwd.TemplateSelectingEventArgs) => this.PaneWorkflowDesigner_TemplateSelecting(eventArgs));

            //Define Criteria Pane events
            this._scope.$on(pdc.MessageType[pdc.MessageType.PaneDefineCriteria_ProcessNodeTemplateRemoving],
                (event: ng.IAngularEvent, eventArgs: pdc.ProcessNodeTemplateRemovingEventArgs) => this.PaneDefineCriteria_ProcessNodeTemplateRemoving(eventArgs));
            this._scope.$on(pdc.MessageType[pdc.MessageType.PaneDefineCriteria_Cancelling],
                (event: ng.IAngularEvent) => this.PaneDefineCriteria_Cancelling());
            this._scope.$on(pdc.MessageType[pdc.MessageType.PaneDefineCriteria_ProcessNodeTemplateUpdating],
                (event: ng.IAngularEvent, eventArgs: pdc.ProcessNodeTemplateUpdatingEventArgs) => this.PaneDefineCriteria_ProcessNodeTemplateUpdating(eventArgs));

            //Process Configure Action Pane events
            this._scope.$on(pca.MessageType[pca.MessageType.PaneConfigureAction_ActionUpdated],
                (event: ng.IAngularEvent, eventArgs: pca.ActionUpdatedEventArgs) => this.PaneConfigureAction_ActionUpdated(eventArgs));
            
            //Select Template Pane events
            this._scope.$on(pst.MessageType[pst.MessageType.PaneSelectTemplate_ProcessTemplateUpdated],
                (event: ng.IAngularEvent, eventArgs: pst.ProcessTemplateUpdatedEventArgs) => this.PaneSelectTemplate_ProcessTemplateUpdated(eventArgs));

            //Process Select Action Pane events
            this._scope.$on(psa.MessageType[psa.MessageType.PaneSelectAction_ActionTypeSelected],
                (event: ng.IAngularEvent, eventArgs: psa.ActionTypeSelectedEventArgs) => this.PaneSelectAction_ActionTypeSelected(eventArgs));
            this._scope.$on(psa.MessageType[psa.MessageType.PaneSelectAction_ActionUpdated],
                (event: ng.IAngularEvent, eventArgs: psa.ActionTypeSelectedEventArgs) => this.PaneSelectAction_ActionUpdated(eventArgs));

            //DEMO (remove before production): watching currentProcessTemplate
            this._scope.$watch('currentProcessTemplate', (newValue, oldValue, scope) => {
                console.log('currentProcessTemplate changed:' + (<any>newValue).SubscribedDocuSignTemplates);
            }, true);
        }

        private loadProcessTemplate() {
            this._scope.currentProcessTemplate = this.ProcessTemplateService.get({ id: this._scope.processTemplateId });
        }
         
        // Find criteria by Id.
        private findProcessNodeTemplate(id: number): model.ProcessNodeTemplate {
            var i;
            for (i = 0; i < this._scope.processNodeTemplates.length; ++i) {
                if (this._scope.processNodeTemplates[i].id === id) {
                    return this._scope.processNodeTemplates[i];
                }
            }
            return null;
        }

        private saveProcessNodeTemplate(callback: (args: pdc.SaveCallbackArgs) => void) {
            if (this._scope.curNodeId != null) {
                this._scope.$broadcast(
                    pdc.MessageType[pdc.MessageType.PaneDefineCriteria_Save],
                    new pdc.SaveEventArgs(callback)
                    );
                this._scope.curNodeId = null;
                this._scope.curNodeIsTempId = false;
            }
            else {
                callback(null);
            }
        }

        /*
            Handles message 'PaneDefineCriteria_ProcessNodeTemplateUpdating'
        */
        private PaneDefineCriteria_ProcessNodeTemplateUpdating(eventArgs: pdc.ProcessNodeTemplateUpdatingEventArgs) {
            console.log('ProcessBuilderController::PaneDefineCriteria_ProcessNodeTemplateUpdating', eventArgs);

            if (eventArgs.processNodeTemplateTempId) {
                this._scope.$broadcast(
                    pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ProcessNodeTemplateTempIdReplaced],
                    new pwd.ProcessNodeTemplateTempIdReplacedEventArgs(eventArgs.processNodeTemplateTempId, eventArgs.processNodeTemplateId)
                    );
            }

            this._scope.$broadcast(
                pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ProcessNodeTemplateNameUpdated],
                new pwd.ProcessNodeTemplateNameUpdatedEventArgs(eventArgs.processNodeTemplateId, eventArgs.name)
                );
        }

        /*
            Handles message 'PaneDefineCriteria_CriteriaRemoving'
        */
        private PaneDefineCriteria_ProcessNodeTemplateRemoving(eventArgs: pdc.ProcessNodeTemplateRemovingEventArgs) {
            console.log('ProcessBuilderController::PaneDefineCriteria_ProcessNodeTemplateRemoving', eventArgs);

            // Tell Workflow Designer to remove criteria.
            this._scope.$broadcast(
                pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ProcessNodeTemplateRemoved],
                new pwd.ProcessNodeTemplateRemovedEventArgs(eventArgs.processNodeTemplateId, eventArgs.isTempId)
            );

            // Hide Define Criteria pane.
            this._scope.$broadcast(pdc.MessageType[pdc.MessageType.PaneDefineCriteria_Hide]);
        }

        /*
            Handles message 'PaneDefineCriteria_Cancelling'
        */
        private PaneDefineCriteria_Cancelling() {
            console.log('ProcessBuilderController::PaneDefineCriteria_Cancelling');

            // If user worked with temporary (not saved criteria), remove criteria from Workflow Designer.
            if (this._scope.curNodeId
                && this._scope.curNodeIsTempId) {

                this._scope.$broadcast(
                    pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ProcessNodeTemplateRemoved],
                    new pwd.ProcessNodeTemplateRemovedEventArgs(
                        this._scope.curNodeId,
                        this._scope.curNodeIsTempId
                    )
                );
            }

            // Hide DefineCriteria pane.
            this._scope.$broadcast(
                pdc.MessageType[pdc.MessageType.PaneDefineCriteria_Hide]
                );

            // Set currentCriteria to null, marking that no criteria is currently selected.
            this._scope.curNodeId = null;
            this._scope.curNodeIsTempId = false;
        }

        /*
            Handles message 'PaneDefineCriteria_CriteriaUpdating'
        */
        /*
        private PaneDefineCriteria_ProcessNodeTemplateUpdated(eventArgs: pdc.ProcessNodeTemplateRemovingEventArgs) {
            console.log('ProcessBuilderController::PaneDefineCriteria_CriteriaRemoving', eventArgs);

            // Tell Workflow Designer to remove criteria.
            // this._scope.$broadcast(
            //     pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_CriteriaRemoved],
            //     new pwd.CriteriaRemovedEventArgs(eventArgs.criteriaId)
            //     );

            //Added by Alexei Avrutin
            //An event to enable consistency with Design Document (part 3, rule 4)
            var eArgs = new pwd.ProcessNodeTemplateNameUpdatedEventArgs(eventArgs.processNodeTemplateId)

            this._scope.$broadcast(
                pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_UpdateCriteriaName], eArgs);

            // Hide Define Criteria pane.
            this._scope.$broadcast(pdc.MessageType[pdc.MessageType.PaneDefineCriteria_Hide]);
        }
        */  
        /*
            Handles message 'WorkflowDesignerPane_CriteriaAdding'
        */
        private PaneWorkflowDesigner_ProcessNodeTemplateAdding(eventArgs: pwd.ProcessNodeTemplateAddingEventArgs) {
            console.log('ProcessBuilderController::PaneWorkflowDesigner_CriteriaAdding', eventArgs);

            var self = this;
            this.saveProcessNodeTemplate(function () {
                // Make Workflow Designer add newly created criteria.
                self._scope.$broadcast(
                    pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ProcessNodeTemplateAdded],
                    new pwd.ProcessNodeTemplateAddedEventArgs(self.LocalIdentityGenerator.getNextId(), true, 'New criteria')
                    );
            });
        }

        /*
            Handles message 'WorkflowDesignerPane_CriteriaSelected'
        */
        private PaneWorkflowDesigner_ProcessNodeTemplateSelecting(eventArgs: pwd.ProcessNodeTemplateSelectingEventArgs) {
            console.log("ProcessBuilderController::PaneWorkflowDesigner_CriteriaSelected", eventArgs);
            var self = this;
            this.saveProcessNodeTemplate(function () {
                self.SaveAction();

                self._scope.currentAction = null; // the prev action is apparently unselected

                // Set current Criteria to currently selected criteria.
                self._scope.curNodeId = eventArgs.id;
                self._scope.curNodeIsTempId = eventArgs.isTempId;

                var scope = self._scope;
                // Hide Select Template Pane
                scope.$broadcast(pst.MessageType[pst.MessageType.PaneSelectTemplate_Hide]);

                // Show Define Criteria Pane
                scope.$broadcast(
                    pdc.MessageType[pdc.MessageType.PaneDefineCriteria_Render],
                    new pdc.RenderEventArgs(scope.fields, self._scope.processTemplateId, eventArgs.id, eventArgs.isTempId)
                    );

                // Hide Select Action Pane
                scope.$broadcast(psa.MessageType[psa.MessageType.PaneSelectAction_Hide]);
                
                // Hide Configure Action Pane
                scope.$broadcast(pca.MessageType[pca.MessageType.PaneConfigureAction_Hide]);
            });
        }

        /*
            Handles message 'PaneWorkflowDesigner_ActionAdding'
        */
        private PaneWorkflowDesigner_ActionAdding(eventArgs: pwd.ActionAddingEventArgs) {
            console.log('ProcessBuilderController::PaneWorkflowDesigner_ActionAdding', eventArgs);

            var self = this;
            this.saveProcessNodeTemplate(function (args: pdc.SaveCallbackArgs) {
                // Generate next Id.
                var id = self.LocalIdentityGenerator.getNextId();

                // Create action object.
                var action = new model.Action(
                    id,
                    true,
                    args == null ? eventArgs.criteriaId : args.id
                    );

                action.userLabel = 'New Action #' + Math.abs(id).toString();

                // Add action to Workflow Designer.
                self._scope.$broadcast(
                    pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ActionAdded],
                    new pwd.ActionAddedEventArgs(action.criteriaId, action.clone())
                    );
            });
        }

        /*
            Handles message 'WorkflowDesignerPane_ActionSelecting'
        */
        private PaneWorkflowDesigner_ActionSelecting(eventArgs: pwd.ActionSelectingEventArgs) {
            console.log("ProcessBuilderController: action selected");

            var self = this;
            this.saveProcessNodeTemplate(function () {
                self.SaveAction();

                //Render Select Action Pane
                var eArgs = new psa.RenderEventArgs(
                    eventArgs.criteriaId,
                    eventArgs.actionId,
                    false); // eventArgs.isTempId,

                self._scope.currentAction = self.ActionService.get({ id: eventArgs.criteriaId });

                var scope = self._scope;
                scope.$broadcast(pst.MessageType[pst.MessageType.PaneSelectTemplate_Hide]);
                scope.$broadcast(pdc.MessageType[pdc.MessageType.PaneDefineCriteria_Hide]);
                scope.$broadcast(
                    psa.MessageType[psa.MessageType.PaneSelectAction_Render],
                    eArgs
                    );
            });
        }

        /*
            Handles message 'WorkflowDesignerPane_TemplateSelected'
        */
        private PaneWorkflowDesigner_TemplateSelecting(eventArgs: pwd.TemplateSelectingEventArgs) {
            console.log("ProcessBuilderController: template selected");
            var scope = this._scope;
            this.SaveAction();
            this._scope.currentAction = null; // action is apparently unselected
            //this._scope.$apply(function () {

            var scope = this._scope;
            //Show Select Template Pane
            var eArgs = new directives.paneSelectTemplate.RenderEventArgs();
            scope.$broadcast(pst.MessageType[pst.MessageType.PaneSelectTemplate_Render]);

            //Hide Define Criteria Pane
            scope.$broadcast(pdc.MessageType[pdc.MessageType.PaneDefineCriteria_Hide]);

            //Hide Select Action Pane
            scope.$broadcast(psa.MessageType[psa.MessageType.PaneSelectAction_Hide]);
                
            //Hide Configure Action Pane
            scope.$broadcast(pca.MessageType[pca.MessageType.PaneConfigureAction_Hide]);
        }

        /*
            Handles message 'ConfigureActionPane_ActionUpdated'
        */
        private PaneConfigureAction_ActionUpdated(eventArgs: pca.ActionUpdatedEventArgs) {
            //Force update on Select Action Pane (FOR DEMO ONLY, NOT IN DESIGN DOCUMENT)
            var eArgs = new directives.paneSelectAction.UpdateActionEventArgs(
                eventArgs.criteriaId, eventArgs.actionId, eventArgs.isTempId);
            this._scope.$broadcast(psa.MessageType[psa.MessageType.PaneSelectAction_UpdateAction], eArgs);
            //Update Action on Designer
            eArgs = new pwd.UpdateActionEventArgs(
                eventArgs.criteriaId,
                eventArgs.actionId,
                eventArgs.isTempId,
                null);

            this._scope.$broadcast(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_UpdateAction], eArgs);
        }

        /*
            Handles message 'SelectActionPane_ActionTypeSelected'
        */
        private PaneSelectAction_ActionTypeSelected(eventArgs: psa.ActionTypeSelectedEventArgs) {
            //Render Pane Configure Action 
            var eArgs = new pca.RenderEventArgs(
                eventArgs.criteriaId,
                eventArgs.actionId,
                eventArgs.isTempId); //is it a temporary id
                
            this._scope.$broadcast(pca.MessageType[pca.MessageType.PaneConfigureAction_Render], eArgs);

            //Render Pane Configure Mapping 
            eArgs = new pcm.RenderEventArgs(
                eventArgs.criteriaId,
                eventArgs.actionId,
                eventArgs.isTempId); //is it a temporary id

            this._scope.$broadcast(pcm.MessageType[pcm.MessageType.PaneConfigureMapping_Render], eArgs);
        }
         
        /*
            Handles message 'PaneSelectAction_ActionUpdated'
        */
        private PaneSelectAction_ActionUpdated(eventArgs: psa.ActionTypeSelectedEventArgs) {
            //Update Pane Workflow Designer
            var eArgs = new pwd.UpdateActionEventArgs(
                eventArgs.criteriaId,
                eventArgs.actionId,
                eventArgs.isTempId,
                eventArgs.actionName);
            this._scope.$broadcast(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_UpdateAction], eArgs);
        }

        private PaneSelectTemplate_ProcessTemplateUpdated(eventArgs: pst.ProcessTemplateUpdatedEventArgs) {
            console.log('changed');

            //Update scope variable
            var currentProcessTemplate = this._scope.currentProcessTemplate || <interfaces.IProcessTemplateVM>{}
            currentProcessTemplate.Name = eventArgs.processTemplateName;
            currentProcessTemplate.SubscribedDocuSignTemplates = eventArgs.subscribedDocuSignTemplates;

            //Relay the message to all other panes
            this._scope.$broadcast(
                pst.MessageType[pst.MessageType.PaneSelectTemplate_ProcessTemplateUpdated],
                new pst.ProcessTemplateUpdatedEventArgs(
                    this._scope.currentProcessTemplate.Id,
                    this._scope.currentProcessTemplate.Name,
                    this._scope.currentProcessTemplate.SubscribedDocuSignTemplates)
                );
        }

        public SaveAction() {
            //If an action is selected, save it
            if (this._scope.currentAction != null) {
                return this.ActionService.save({
                    id: this._scope.currentAction.id
                }, this._scope.currentAction, null, null).$promise;
            }
        }

        public Cancel() {
            this._scope.currentAction = null;
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
            var actions: interfaces.IAction =
                {
                    actionType: "test action type",
                    configurationSettings: "",
                    criteriaId: 1,
                    id: 1,
                    isTempId: false,
                    fieldMappingSettings: "",
                    userLabel: "test",
                    tempId: 0,
                    actionListId: 0
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