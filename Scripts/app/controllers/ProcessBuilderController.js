/// <reference path="../_all.ts" />
/*
    Detail (view/add/edit) controller
*/
var dockyard;
(function (dockyard) {
    var controllers;
    (function (controllers) {
        'use strict';
        //Setup aliases
        var pwd = dockyard.directives.paneWorkflowDesigner;
        var pdc = dockyard.directives.paneDefineCriteria;
        var psa = dockyard.directives.paneSelectAction;
        var pca = dockyard.directives.paneConfigureAction;
        var pst = dockyard.directives.paneSelectTemplate;
        var ProcessBuilderController = (function () {
            function ProcessBuilderController($rootScope, $scope, StringService, LocalIdentityGenerator) {
                this.$rootScope = $rootScope;
                this.$scope = $scope;
                this.StringService = StringService;
                this.LocalIdentityGenerator = LocalIdentityGenerator;
                this._scope = $scope;
                this.setupMessageProcessing();
                this._scope.criteria = [];
                this._scope.fields = [
                    new dockyard.model.Field('envelope.name', '[Envelope].Name'),
                    new dockyard.model.Field('envelope.date', '[Envelope].Date')
                ];
            }
            /*
                Mapping of incoming messages to handlers
            */
            ProcessBuilderController.prototype.setupMessageProcessing = function () {
                var _this = this;
                //Process Designer Pane events
                this._scope.$on(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_CriteriaAdding], function (event, eventArgs) { return _this.PaneWorkflowDesigner_CriteriaAdding(eventArgs); });
                this._scope.$on(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_CriteriaSelecting], function (event, eventArgs) { return _this.PaneWorkflowDesigner_CriteriaSelecting(eventArgs); });
                this._scope.$on(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ActionAdding], function (event, eventArgs) { return _this.PaneWorkflowDesigner_ActionAdding(eventArgs); });
                this._scope.$on(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ActionSelecting], function (event, eventArgs) { return _this.PaneWorkflowDesigner_ActionSelecting(eventArgs); });
                this._scope.$on(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_TemplateSelected], function (event, eventArgs) { return _this.PaneWorkflowDesigner_TemplateSelected(eventArgs); });
                //Define Criteria pane events
                this._scope.$on(pdc.MessageType[pdc.MessageType.PaneDefineCriteria_CriteriaRemoved], function (event, eventArgs) { return _this.PaneDefineCriteria_CriteriaRemoving(eventArgs); });
                //Process Configure Action Pane events
                this._scope.$on(pca.MessageType[pca.MessageType.PaneConfigureAction_Cancelled], function (event, eventArgs) { return _this.PaneConfigureAction_Cancelled(eventArgs); });
                this._scope.$on(pca.MessageType[pca.MessageType.PaneConfigureAction_ActionUpdated], function (event, eventArgs) { return _this.PaneConfigureAction_ActionUpdated(eventArgs); });
                //Process Select Action Pane events
                this._scope.$on(psa.MessageType[psa.MessageType.PaneSelectAction_ActionTypeSelected], function (event, eventArgs) { return _this.PaneSelectAction_ActionTypeSelected(eventArgs); });
            };
            // Find criteria by Id.
            ProcessBuilderController.prototype.findCriteria = function (criteriaId) {
                var i;
                for (i = 0; i < this._scope.criteria.length; ++i) {
                    if (this._scope.criteria[i].id === criteriaId) {
                        return this._scope.criteria[i];
                    }
                }
                return null;
            };
            /*
                Handles message 'PaneDefineCriteria_CriteriaRemoving'
            */
            ProcessBuilderController.prototype.PaneDefineCriteria_CriteriaRemoving = function (eventArgs) {
                console.log('ProcessBuilderController::PaneDefineCriteria_CriteriaRemoving', eventArgs);
                // Tell Workflow Designer to remove criteria.
                this._scope.$broadcast(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_CriteriaRemoved], new pwd.CriteriaRemovedEventArgs(eventArgs.criteriaId));
                // Hide Define Criteria pane.
                this._scope.$broadcast(pdc.MessageType[pdc.MessageType.PaneDefineCriteria_Hide]);
            };
            /*
                Handles message 'WorkflowDesignerPane_CriteriaAdding'
            */
            ProcessBuilderController.prototype.PaneWorkflowDesigner_CriteriaAdding = function (eventArgs) {
                console.log('ProcessBuilderController::PaneWorkflowDesigner_CriteriaAdding', eventArgs);
                // Generate next id.
                var id = this.LocalIdentityGenerator.getNextId();
                // Create criteria with tempId.
                var criteria = new dockyard.model.Criteria(id, true, 'Criteria #' + id.toString(), dockyard.model.CriteriaExecutionMode.WithConditions);
                // Add criteria to list.
                this._scope.criteria.push(criteria);
                // Make Workflow Designer add newly created criteria.
                this._scope.$broadcast(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_CriteriaAdded], new pwd.CriteriaAddedEventArgs(criteria.clone()));
            };
            /*
                Handles message 'WorkflowDesignerPane_CriteriaSelected'
            */
            ProcessBuilderController.prototype.PaneWorkflowDesigner_CriteriaSelecting = function (eventArgs) {
                console.log("ProcessBuilderController::PaneWorkflowDesigner_CriteriaSelected", eventArgs);
                var criteria = this.findCriteria(eventArgs.criteriaId);
                var scope = this._scope;
                scope.$apply(function () {
                    // Show Define Criteria Pane
                    scope.$broadcast(pdc.MessageType[pdc.MessageType.PaneDefineCriteria_Render], new pdc.RenderEventArgs(scope.fields, criteria.clone()));
                    //Hide Select Action Pane
                    scope.$broadcast(psa.MessageType[psa.MessageType.PaneSelectAction_Hide]);
                    //Hide Configure Action Pane
                    scope.$broadcast(pca.MessageType[pca.MessageType.PaneConfigureAction_Hide]);
                });
            };
            /*
                Handles message 'PaneWorkflowDesigner_ActionAdding'
            */
            ProcessBuilderController.prototype.PaneWorkflowDesigner_ActionAdding = function (eventArgs) {
                console.log('ProcessBuilderController::PaneWorkflowDesigner_ActionAdding', eventArgs);
                // Generate next Id.
                var id = this.LocalIdentityGenerator.getNextId();
                // Create action object.
                var action = new dockyard.model.Action(id, id, eventArgs.criteriaId);
                action.name = 'Action #' + id.toString();
                // Add action to criteria.
                var criteria = this.findCriteria(eventArgs.criteriaId);
                criteria.actions.push(action);
                // Add action to Workflow Designer.
                this._scope.$broadcast(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ActionAdded], new pwd.ActionAddedEventArgs(eventArgs.criteriaId, action.clone()));
            };
            /*
                Handles message 'WorkflowDesignerPane_ActionSelecting'
            */
            ProcessBuilderController.prototype.PaneWorkflowDesigner_ActionSelecting = function (eventArgs) {
                console.log("ProcessBuilderController: action selected");
                //Render Select Action Pane
                var eArgs = new psa.RenderEventArgs(eventArgs.criteriaId, eventArgs.actionId, true, 0); // eventArgs.processTemplateId);
                var scope = this._scope;
                this._scope.$apply(function () {
                    scope.$broadcast(pdc.MessageType[pdc.MessageType.PaneDefineCriteria_Hide]);
                    scope.$broadcast(psa.MessageType[psa.MessageType.PaneSelectAction_Render], eArgs);
                });
            };
            /*
                Handles message 'WorkflowDesignerPane_TemplateSelected'
            */
            ProcessBuilderController.prototype.PaneWorkflowDesigner_TemplateSelected = function (eventArgs) {
                console.log("ProcessBuilderController: template selected");
                //Show Select Template Pane
                var eArgs = new dockyard.directives.paneSelectTemplate.RenderEventArgs(eventArgs.processTemplateId);
                this._scope.$broadcast(pst.MessageType[pst.MessageType.PaneSelectTemplate_Render]);
            };
            /*
                Handles message 'ConfigureActionPane_ActionUpdated'
            */
            ProcessBuilderController.prototype.PaneConfigureAction_ActionUpdated = function (eventArgs) {
                //Force update on Select Action Pane 
                var eArgs = new dockyard.directives.paneSelectAction.UpdateActionEventArgs(eventArgs.criteriaId, eventArgs.actionId, eventArgs.actionTempId, 0);
                this._scope.$broadcast(psa.MessageType[psa.MessageType.PaneSelectAction_UpdateAction], eArgs);
                // Update Action on Designer
                // var eArgs: pwd.UpdateActionEventArgs = {
                //     criteriaId: eventArgs.criteriaId,
                //     actionId: eventArgs.actionId,
                //     actionTempId: eventArgs.actionTempId,
                //     processTemplateId: 0
                // };
                // this._scope.$broadcast(psa.MessageType[pwd.MessageType.PaneWorkflowDesigner_UpdateAction], eArgs);
            };
            /*
                Handles message 'ConfigureActionPane_Cancelled'
            */
            ProcessBuilderController.prototype.PaneConfigureAction_Cancelled = function (eventArgs) {
                //Hide Select Action Pane
                this._scope.$broadcast(psa.MessageType[psa.MessageType.PaneSelectAction_Hide]);
            };
            /*
                Handles message 'SelectActionPane_ActionTypeSelected'
            */
            ProcessBuilderController.prototype.PaneSelectAction_ActionTypeSelected = function (eventArgs) {
                console.log("action type selected");
                //Render Configure Action Pane
                var eArgs = new psa.RenderEventArgs(eventArgs.criteriaId, eventArgs.actionId > 0 ? eventArgs.actionId : eventArgs.tempActionId, eventArgs.actionId < 0, eventArgs.processTemplateId);
                this._scope.$broadcast(pca.MessageType[pca.MessageType.PaneConfigureAction_Render], eArgs);
            };
            // $inject annotation.
            // It provides $injector with information about dependencies to be injected into constructor
            // it is better to have it close to the constructor, because the parameters must match in count and type.
            // See http://docs.angularjs.org/guide/di
            ProcessBuilderController.$inject = [
                '$rootScope',
                '$scope',
                'StringService',
                'LocalIdentityGenerator'
            ];
            return ProcessBuilderController;
        })();
        app.controller('ProcessBuilderController', ProcessBuilderController);
    })(controllers = dockyard.controllers || (dockyard.controllers = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=ProcessBuilderController.js.map