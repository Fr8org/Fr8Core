/// <reference path="../_all.ts" />
var dockyard;
(function (dockyard) {
    var controllers;
    (function (controllers) {
        'use strict';
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
            ProcessBuilderController.prototype.setupMessageProcessing = function () {
                var _this = this;
                this._scope.$on(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_CriteriaAdding], function (event, eventArgs) { return _this.PaneWorkflowDesigner_CriteriaAdding(eventArgs); });
                this._scope.$on(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_CriteriaSelecting], function (event, eventArgs) { return _this.PaneWorkflowDesigner_CriteriaSelecting(eventArgs); });
                this._scope.$on(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ActionAdding], function (event, eventArgs) { return _this.PaneWorkflowDesigner_ActionAdding(eventArgs); });
                this._scope.$on(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ActionSelecting], function (event, eventArgs) { return _this.PaneWorkflowDesigner_ActionSelecting(eventArgs); });
                this._scope.$on(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_TemplateSelected], function (event, eventArgs) { return _this.PaneWorkflowDesigner_TemplateSelected(eventArgs); });
                this._scope.$on(pdc.MessageType[pdc.MessageType.PaneDefineCriteria_CriteriaRemoved], function (event, eventArgs) { return _this.PaneDefineCriteria_CriteriaRemoving(eventArgs); });
                this._scope.$on(pca.MessageType[pca.MessageType.PaneConfigureAction_Cancelled], function (event, eventArgs) { return _this.PaneConfigureAction_Cancelled(eventArgs); });
                this._scope.$on(pca.MessageType[pca.MessageType.PaneConfigureAction_ActionUpdated], function (event, eventArgs) { return _this.PaneConfigureAction_ActionUpdated(eventArgs); });
                this._scope.$on(psa.MessageType[psa.MessageType.PaneSelectAction_ActionTypeSelected], function (event, eventArgs) { return _this.PaneSelectAction_ActionTypeSelected(eventArgs); });
            };
            ProcessBuilderController.prototype.findCriteria = function (criteriaId) {
                var i;
                for (i = 0; i < this._scope.criteria.length; ++i) {
                    if (this._scope.criteria[i].id === criteriaId) {
                        return this._scope.criteria[i];
                    }
                }
                return null;
            };
            ProcessBuilderController.prototype.PaneDefineCriteria_CriteriaRemoving = function (eventArgs) {
                console.log('ProcessBuilderController::PaneDefineCriteria_CriteriaRemoving', eventArgs);
                this._scope.$broadcast(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_CriteriaRemoved], new pwd.CriteriaRemovedEventArgs(eventArgs.criteriaId));
                this._scope.$broadcast(pdc.MessageType[pdc.MessageType.PaneDefineCriteria_Hide]);
            };
            ProcessBuilderController.prototype.PaneWorkflowDesigner_CriteriaAdding = function (eventArgs) {
                console.log('ProcessBuilderController::PaneWorkflowDesigner_CriteriaAdding', eventArgs);
                var id = this.LocalIdentityGenerator.getNextId();
                var criteria = new dockyard.model.Criteria(id, true, 'Criteria #' + id.toString(), dockyard.model.CriteriaExecutionMode.WithConditions);
                this._scope.criteria.push(criteria);
                this._scope.$broadcast(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_CriteriaAdded], new pwd.CriteriaAddedEventArgs(criteria.clone()));
            };
            ProcessBuilderController.prototype.PaneWorkflowDesigner_CriteriaSelecting = function (eventArgs) {
                console.log("ProcessBuilderController::PaneWorkflowDesigner_CriteriaSelected", eventArgs);
                var criteria = this.findCriteria(eventArgs.criteriaId);
                var scope = this._scope;
                scope.$apply(function () {
                    scope.$broadcast(pdc.MessageType[pdc.MessageType.PaneDefineCriteria_Render], new pdc.RenderEventArgs(scope.fields, criteria.clone()));
                    scope.$broadcast(psa.MessageType[psa.MessageType.PaneSelectAction_Hide]);
                    scope.$broadcast(pca.MessageType[pca.MessageType.PaneConfigureAction_Hide]);
                });
            };
            ProcessBuilderController.prototype.PaneWorkflowDesigner_ActionAdding = function (eventArgs) {
                console.log('ProcessBuilderController::PaneWorkflowDesigner_ActionAdding', eventArgs);
                var id = this.LocalIdentityGenerator.getNextId();
                var action = new dockyard.model.Action(id, id, eventArgs.criteriaId);
                action.name = 'Action #' + id.toString();
                var criteria = this.findCriteria(eventArgs.criteriaId);
                criteria.actions.push(action);
                this._scope.$broadcast(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ActionAdded], new pwd.ActionAddedEventArgs(eventArgs.criteriaId, action.clone()));
            };
            ProcessBuilderController.prototype.PaneWorkflowDesigner_ActionSelecting = function (eventArgs) {
                console.log("ProcessBuilderController: action selected");
                var eArgs = new psa.RenderEventArgs(eventArgs.criteriaId, eventArgs.actionId, true, 0);
                var scope = this._scope;
                this._scope.$apply(function () {
                    scope.$broadcast(pdc.MessageType[pdc.MessageType.PaneDefineCriteria_Hide]);
                    scope.$broadcast(psa.MessageType[psa.MessageType.PaneSelectAction_Render], eArgs);
                });
            };
            ProcessBuilderController.prototype.PaneWorkflowDesigner_TemplateSelected = function (eventArgs) {
                console.log("ProcessBuilderController: template selected");
                var eArgs = new dockyard.directives.paneSelectTemplate.RenderEventArgs(eventArgs.processTemplateId);
                this._scope.$broadcast(pst.MessageType[pst.MessageType.PaneSelectTemplate_Render]);
            };
            ProcessBuilderController.prototype.PaneConfigureAction_ActionUpdated = function (eventArgs) {
                var eArgs = new dockyard.directives.paneSelectAction.UpdateActionEventArgs(eventArgs.criteriaId, eventArgs.actionId, eventArgs.actionTempId, 0);
                this._scope.$broadcast(psa.MessageType[psa.MessageType.PaneSelectAction_UpdateAction], eArgs);
            };
            ProcessBuilderController.prototype.PaneConfigureAction_Cancelled = function (eventArgs) {
                this._scope.$broadcast(psa.MessageType[psa.MessageType.PaneSelectAction_Hide]);
            };
            ProcessBuilderController.prototype.PaneSelectAction_ActionTypeSelected = function (eventArgs) {
                console.log("action type selected");
                var eArgs = new psa.RenderEventArgs(eventArgs.criteriaId, eventArgs.actionId > 0 ? eventArgs.actionId : eventArgs.tempActionId, eventArgs.actionId < 0, eventArgs.processTemplateId);
                this._scope.$broadcast(pca.MessageType[pca.MessageType.PaneConfigureAction_Render], eArgs);
            };
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
//# sourceMappingURL=processbuildercontroller.js.map