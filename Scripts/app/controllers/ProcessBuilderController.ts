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
            '$state'
        ];

        private _scope: interfaces.IProcessBuilderScope;

        constructor(
            private $rootScope: interfaces.IAppRootScope,
            private $scope: interfaces.IProcessBuilderScope,
            private StringService: services.IStringService,
            private LocalIdentityGenerator: services.ILocalIdentityGenerator,
            private $state: ng.ui.IState) {
            this._scope = $scope;

            this.setupMessageProcessing();

            // Dummy value for processTemplateId;
            this._scope.processTemplateId = 0;
            this._scope.criteria = [];
            this._scope.fields = [
                new model.Field('envelope.name', '[Envelope].Name'),
                new model.Field('envelope.date', '[Envelope].Date')
            ];
        }

        /*
            Mapping of incoming messages to handlers
        */
        private setupMessageProcessing() {

            //Process Designer Pane events
            this._scope.$on(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_CriteriaAdding],
                (event: ng.IAngularEvent, eventArgs: pwd.CriteriaAddingEventArgs) => this.PaneWorkflowDesigner_CriteriaAdding(eventArgs));
            this._scope.$on(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_CriteriaSelecting],
                (event: ng.IAngularEvent, eventArgs: pwd.CriteriaSelectingEventArgs) => this.PaneWorkflowDesigner_CriteriaSelecting(eventArgs));
            this._scope.$on(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ActionAdding],
                (event: ng.IAngularEvent, eventArgs: pwd.ActionAddingEventArgs) => this.PaneWorkflowDesigner_ActionAdding(eventArgs));
            this._scope.$on(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ActionSelecting],
                (event: ng.IAngularEvent, eventArgs: pwd.ActionSelectingEventArgs) => this.PaneWorkflowDesigner_ActionSelecting(eventArgs));
            this._scope.$on(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_TemplateSelecting],
                (event: ng.IAngularEvent, eventArgs: pwd.TemplateSelectedEventArgs) => this.PaneWorkflowDesigner_TemplateSelecting(eventArgs));

            //Define Criteria Pane events
            this._scope.$on(pdc.MessageType[pdc.MessageType.PaneDefineCriteria_CriteriaRemoving],
                (event: ng.IAngularEvent, eventArgs: pdc.CriteriaRemovingEventArgs) => this.PaneDefineCriteria_CriteriaRemoving(eventArgs));

            //Process Configure Action Pane events
            this._scope.$on(pca.MessageType[pca.MessageType.PaneConfigureAction_Cancelled],
                (event: ng.IAngularEvent, eventArgs: pca.CancelledEventArgs) => this.PaneConfigureAction_Cancelled(eventArgs));
            this._scope.$on(pca.MessageType[pca.MessageType.PaneConfigureAction_ActionUpdated],
                (event: ng.IAngularEvent, eventArgs: pca.ActionUpdatedEventArgs) => this.PaneConfigureAction_ActionUpdated(eventArgs));

            //Process Select Action Pane events
            this._scope.$on(psa.MessageType[psa.MessageType.PaneSelectAction_ActionTypeSelected],
                (event: ng.IAngularEvent, eventArgs: psa.ActionTypeSelectedEventArgs) => this.PaneSelectAction_ActionTypeSelected(eventArgs));
            this._scope.$on(psa.MessageType[psa.MessageType.PaneSelectAction_ActionUpdated],
                (event: ng.IAngularEvent, eventArgs: psa.ActionTypeSelectedEventArgs) => this.PaneSelectAction_ActionUpdated(eventArgs));

            //Process Select Template Pane events
            this._scope.$on(pst.MessageType[pst.MessageType.PaneSelectTemplate_ProcessTemplateUpdated],
                (event: ng.IAngularEvent, eventArgs: pst.ProcessTemplateUpdatedEventArgs) => {
                    this.$state.data.pageSubTitle = eventArgs.processTemplateName
                });
        }
         
        // Find criteria by Id.
        private findCriteria(criteriaId: number): model.Criteria {
            var i;
            for (i = 0; i < this._scope.criteria.length; ++i) {
                if (this._scope.criteria[i].id === criteriaId) {
                    return this._scope.criteria[i];
                }
            }
            return null;
        }


        /*
            Handles message 'PaneDefineCriteria_CriteriaRemoving'
        */
        private PaneDefineCriteria_CriteriaRemoving(eventArgs: pdc.CriteriaRemovingEventArgs) {
            console.log('ProcessBuilderController::PaneDefineCriteria_CriteriaRemoving', eventArgs);

            // Tell Workflow Designer to remove criteria.
            this._scope.$broadcast(
                pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_CriteriaRemoved],
                new pwd.CriteriaRemovedEventArgs(eventArgs.criteriaId)
                );

            // Hide Define Criteria pane.
            this._scope.$broadcast(pdc.MessageType[pdc.MessageType.PaneDefineCriteria_Hide]);
        }

        /*
            Handles message 'PaneDefineCriteria_CriteriaUpdating'
        */
        private PaneDefineCriteria_CriteriaUpdated(eventArgs: pdc.CriteriaRemovingEventArgs) {
            console.log('ProcessBuilderController::PaneDefineCriteria_CriteriaRemoving', eventArgs);

            // Tell Workflow Designer to remove criteria.
            this._scope.$broadcast(
                pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_CriteriaRemoved],
                new pwd.CriteriaRemovedEventArgs(eventArgs.criteriaId)
                );

            //Added by Alexei Avrutin
            //An event to enable consistency with Design Document (part 3, rule 4)
            var eArgs = new pwd.UpdateCriteriaNameEventArgs(eventArgs.criteriaId)

            this._scope.$broadcast(
                pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_UpdateCriteriaName], eArgs);


            // Hide Define Criteria pane.
            this._scope.$broadcast(pdc.MessageType[pdc.MessageType.PaneDefineCriteria_Hide]);
        }
            
        /*
            Handles message 'WorkflowDesignerPane_CriteriaAdding'
        */
        private PaneWorkflowDesigner_CriteriaAdding(eventArgs: pwd.CriteriaAddingEventArgs) {
            console.log('ProcessBuilderController::PaneWorkflowDesigner_CriteriaAdding', eventArgs);

            // Generate next id.
            var id = this.LocalIdentityGenerator.getNextId();

            // Create criteria with tempId.
            var criteria = new model.Criteria(
                id,
                true,
                'Criteria #' + id.toString(),
                model.CriteriaExecutionMode.WithConditions
                );

            // Add criteria to list.
            this._scope.criteria.push(criteria);

            // Make Workflow Designer add newly created criteria.
            this._scope.$broadcast(
                pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_CriteriaAdded],
                new pwd.CriteriaAddedEventArgs(criteria.clone())
                );
        }

        /*
            Handles message 'WorkflowDesignerPane_CriteriaSelected'
        */
        private PaneWorkflowDesigner_CriteriaSelecting(eventArgs: pwd.CriteriaSelectingEventArgs) {
            console.log("ProcessBuilderController::PaneWorkflowDesigner_CriteriaSelected", eventArgs);

            var criteria = this.findCriteria(eventArgs.criteriaId);

            var scope = this._scope;
            scope.$apply(function () {
                // Show Define Criteria Pane
                scope.$broadcast(
                    pdc.MessageType[pdc.MessageType.PaneDefineCriteria_Render],
                    new pdc.RenderEventArgs(scope.fields, criteria.clone())
                    );

                //Hide Select Action Pane
                scope.$broadcast(psa.MessageType[psa.MessageType.PaneSelectAction_Hide]);
                
                //Hide Configure Action Pane
                scope.$broadcast(pca.MessageType[pca.MessageType.PaneConfigureAction_Hide]);
            });
        }

        /*
            Handles message 'PaneWorkflowDesigner_ActionAdding'
        */
        private PaneWorkflowDesigner_ActionAdding(eventArgs: pwd.ActionAddingEventArgs) {
            console.log('ProcessBuilderController::PaneWorkflowDesigner_ActionAdding', eventArgs);

            // Generate next Id.
            var id = this.LocalIdentityGenerator.getNextId();

            // Create action object.
            var action = new model.Action(
                id,
                id,
                eventArgs.criteriaId
                );

            action.name = 'Action #' + id.toString();

            // Add action to criteria.
            var criteria = this.findCriteria(eventArgs.criteriaId);
            criteria.actions.push(action);

            // Add action to Workflow Designer.
            this._scope.$broadcast(
                pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ActionAdded],
                new pwd.ActionAddedEventArgs(eventArgs.criteriaId, action.clone())
                );
        }

        /*
            Handles message 'WorkflowDesignerPane_ActionSelecting'
        */
        private PaneWorkflowDesigner_ActionSelecting(eventArgs: pwd.ActionSelectingEventArgs) {
            console.log("ProcessBuilderController: action selected");

            //Render Select Action Pane
            var eArgs = new psa.RenderEventArgs(
                eventArgs.criteriaId,
                eventArgs.actionId,
                true); // eventArgs.isTempId,

            var scope = this._scope;
            this._scope.$apply(function () {
                scope.$broadcast(pdc.MessageType[pdc.MessageType.PaneDefineCriteria_Hide]);
                scope.$broadcast(psa.MessageType[psa.MessageType.PaneSelectAction_Render], eArgs);
            });
        }

        /*
            Handles message 'WorkflowDesignerPane_TemplateSelected'
        */
        private PaneWorkflowDesigner_TemplateSelecting(eventArgs: pwd.TemplateSelectedEventArgs) {
            console.log("ProcessBuilderController: template selected");

            //Show Select Template Pane
            var eArgs = new directives.paneSelectTemplate.RenderEventArgs();
            this._scope.$broadcast(pst.MessageType[pst.MessageType.PaneSelectTemplate_Render]);
        }

        /*
            Handles message 'ConfigureActionPane_ActionUpdated'
        */
        private PaneConfigureAction_ActionUpdated(eventArgs: pca.ActionUpdatedEventArgs) {
            //Force update on Select Action Pane (FOR DEMO ONLY, NOT IN DESIGN DOCUMENT)
            var eArgs = new directives.paneSelectAction.UpdateActionEventArgs(
                eventArgs.criteriaId, eventArgs.actionId, eventArgs.actionTempId);
            this._scope.$broadcast(psa.MessageType[psa.MessageType.PaneSelectAction_UpdateAction], eArgs);

            //Update Action on Designer
            eArgs = new pwd.UpdateActionEventArgs(
                eventArgs.criteriaId,
                eventArgs.actionId,
                eventArgs.actionTempId,
                null);

            this._scope.$broadcast(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_UpdateAction], eArgs);
        }

        /*
            Handles message 'ConfigureActionPane_Cancelled'
        */
        private PaneConfigureAction_Cancelled(eventArgs: pca.CancelledEventArgs) {
            //Hide Select Action Pane
            this._scope.$broadcast(psa.MessageType[psa.MessageType.PaneSelectAction_Hide]);

            //Hide Configure Mapping Pane
            this._scope.$broadcast(pcm.MessageType[pcm.MessageType.PaneConfigureMapping_Hide]);
        }

        /*
            Handles message 'SelectActionPane_ActionTypeSelected'
        */
        private PaneSelectAction_ActionTypeSelected(eventArgs: psa.ActionTypeSelectedEventArgs) {
            //Render Pane Configure Action 
            var eArgs = new pca.RenderEventArgs(
                eventArgs.criteriaId,
                eventArgs.actionId > 0 ? eventArgs.actionId : eventArgs.tempActionId, //either permanent or temp id
                eventArgs.actionId < 0); //is it a temporary id
                
            this._scope.$broadcast(pca.MessageType[pca.MessageType.PaneConfigureAction_Render], eArgs);

            //Render Pane Configure Mapping 
            eArgs = new pcm.RenderEventArgs(
                eventArgs.criteriaId,
                eventArgs.actionId > 0 ? eventArgs.actionId : eventArgs.tempActionId, //either permanent or temp id
                eventArgs.actionId < 0); //is it a temporary id

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
                eventArgs.tempActionId,
                eventArgs.actionName);
            this._scope.$broadcast(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_UpdateAction], eArgs);
        }


    }
    app.controller('ProcessBuilderController', ProcessBuilderController);
} 