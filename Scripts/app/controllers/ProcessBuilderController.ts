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
            '$q'
        ];

        private _scope: interfaces.IProcessBuilderScope;

        constructor(
            private $rootScope: interfaces.IAppRootScope,
            private $scope: interfaces.IProcessBuilderScope,
            private StringService: services.IStringService,
            private LocalIdentityGenerator: services.ILocalIdentityGenerator,
            private $state: ng.ui.IState,
            private ActionService: services.IActionService,
            private $q: ng.IQService
            ) {
            this._scope = $scope;

            this.setupMessageProcessing();

            // Dummy value for processTemplateId;
            this._scope.processTemplateId = 0;
            this._scope.criteria = [];
            this._scope.fields = [
                new model.Field('envelope.name', '[Envelope].Name'),
                new model.Field('envelope.date', '[Envelope].Date')
            ];
            this._scope.Cancel = angular.bind(this, this.Cancel);
            this._scope.Save = angular.bind(this, this.SaveAction);
        }

        /*
            Mapping of incoming messages to handlers
        */
        private setupMessageProcessing() {

            //Process Designer Pane events
            this._scope.$on(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_CriteriaAdding],
                (event: ng.IAngularEvent, eventArgs: pwd.CriteriaAddingEventArgs) => this.PaneWorkflowDesigner_CriteriaAdding(eventArgs));
            this._scope.$on(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_CriteriaSelecting],
                (event: ng.IAngularEvent, eventArgs: pwd.CriteriaSelectingEventArgs) => this.PaneWorkflowDesigner_CriteriaSelected(eventArgs));
            this._scope.$on(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ActionAdding],
                (event: ng.IAngularEvent, eventArgs: pwd.ActionAddingEventArgs) => this.PaneWorkflowDesigner_ActionAdding(eventArgs));
            this._scope.$on(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ActionSelecting],
                (event: ng.IAngularEvent, eventArgs: pwd.ActionSelectingEventArgs) => this.PaneWorkflowDesigner_ActionSelected(eventArgs));
            this._scope.$on(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_TemplateSelected],
                (event: ng.IAngularEvent, eventArgs: pwd.TemplateSelectedEventArgs) => this.PaneWorkflowDesigner_TemplateSelected(eventArgs));

            //Define Criteria Pane events
            this._scope.$on(pdc.MessageType[pdc.MessageType.PaneDefineCriteria_CriteriaRemoving],
                (event: ng.IAngularEvent, eventArgs: pdc.CriteriaRemovingEventArgs) => this.PaneDefineCriteria_CriteriaRemoving(eventArgs));

            //Process Configure Action Pane events
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
                'New Criteria #' + Math.abs(id).toString(),
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
        private PaneWorkflowDesigner_CriteriaSelected(eventArgs: pwd.CriteriaSelectingEventArgs) {
            console.log("ProcessBuilderController::PaneWorkflowDesigner_CriteriaSelected", eventArgs);

            this.SaveAction();

            this._scope.currentAction = null; // the prev action is apparently unselected

            var criteria = this.findCriteria(eventArgs.criteriaId);

            var scope = this._scope;
            // Hide Select Template Pane
            scope.$broadcast(pst.MessageType[pst.MessageType.PaneSelectTemplate_Hide]);

            if (criteria != null) { // by Aleksei Avrutin: for unit testing
                // Show Define Criteria Pane
                scope.$broadcast(
                    pdc.MessageType[pdc.MessageType.PaneDefineCriteria_Render],
                    new pdc.RenderEventArgs(scope.fields, criteria.clone())
                    );
            }

            // Hide Select Action Pane
            scope.$broadcast(psa.MessageType[psa.MessageType.PaneSelectAction_Hide]);
                
            // Hide Configure Action Pane
            scope.$broadcast(pca.MessageType[pca.MessageType.PaneConfigureAction_Hide]);
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
                true,
                eventArgs.criteriaId
            );

            action.userLabel = 'New Action #' + Math.abs(id).toString();

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
        private PaneWorkflowDesigner_ActionSelected(eventArgs: pwd.ActionSelectingEventArgs) {
            console.log("ProcessBuilderController: action selected");
            //Render Select Action Pane
            var eArgs = new psa.RenderEventArgs(
                eventArgs.criteriaId,
                eventArgs.actionId,
                false); // eventArgs.isTempId,

            this.SaveAction();
            this._scope.currentAction = this.ActionService.get({ id: eventArgs.criteriaId });

            var scope = this._scope;
            scope.$broadcast(pst.MessageType[pst.MessageType.PaneSelectTemplate_Hide]);
                scope.$broadcast(pdc.MessageType[pdc.MessageType.PaneDefineCriteria_Hide]);
            scope.$broadcast(
                psa.MessageType[psa.MessageType.PaneSelectAction_Render],
                eArgs
            );
        }

        /*
            Handles message 'WorkflowDesignerPane_TemplateSelected'
        */
        private PaneWorkflowDesigner_TemplateSelected(eventArgs: pwd.TemplateSelectedEventArgs) {
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