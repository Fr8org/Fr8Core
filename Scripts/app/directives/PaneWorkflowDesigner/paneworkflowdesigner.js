/// <reference path="../../_all.ts" />
var dockyard;
(function (dockyard) {
    var directives;
    (function (directives) {
        var paneWorkflowDesigner;
        (function (paneWorkflowDesigner) {
            function PaneWorkflowDesigner() {
                var onRender = function (eventArgs, scope) {
                    console.log('PaneWorkflowDesigner::onRender', eventArgs);
                };
                var onProcessNodeTemplateAdded = function (eventArgs, scope) {
                    console.log('PaneWorkflowDesigner::onCriteriaAdded', eventArgs);
                    scope.widget.addCriteria({
                        id: eventArgs.id,
                        isTempId: eventArgs.isTempId,
                        name: eventArgs.name
                    });
                    scope.$emit(paneWorkflowDesigner.MessageType[paneWorkflowDesigner.MessageType.PaneWorkflowDesigner_CriteriaSelected], new paneWorkflowDesigner.CriteriaSelectedEventArgs(eventArgs.id, eventArgs.isTempId));
                };
                var onProcessNodeTemplateRemoved = function (eventArgs, scope) {
                    console.log('PaneWorkflowDesigner::onCriteriaRemoved', eventArgs);
                    scope.widget.removeCriteria(eventArgs.id, eventArgs.isTempId);
                };
                var onActionAdded = function (eventArgs, scope) {
                    console.log('PaneWorkflowDesigner::onActionAdded', eventArgs);
                    var actionObj = eventArgs.action;
                    scope.widget.addAction(eventArgs.criteriaId, eventArgs.action, eventArgs.actionListType);
                    scope.$emit(paneWorkflowDesigner.MessageType[paneWorkflowDesigner.MessageType.PaneWorkflowDesigner_ActionSelected], new paneWorkflowDesigner.ActionSelectedEventArgs(eventArgs.criteriaId, eventArgs.action.id, eventArgs.actionListType, 0));
                };
                var onActionRemoved = function (eventArgs, scope) {
                    console.log('PaneWorkflowDesigner::onActionRemove', eventArgs);
                    scope.widget.removeAction(eventArgs.id, eventArgs.isTempId);
                };
                var onProcessNodeTemplateTempIdReplaced = function (eventArgs, scope) {
                    scope.widget.replaceCriteriaTempId(eventArgs.tempId, eventArgs.id);
                };
                var onProcessNodeTemplateRenamed = function (eventArgs, scope) {
                    scope.widget.renameCriteria(eventArgs.id, eventArgs.text);
                };
                var onActionTempIdReplaced = function (eventArgs, scope) {
                    scope.widget.replaceActionTempId(eventArgs.tempId, eventArgs.id);
                };
                var onActionRenamed = function (eventArgs, scope) {
                    scope.widget.renameAction(eventArgs.id, eventArgs.name);
                };
                var onUpdateActivityTemplateIdForAction = function (eventArgs, scope) {
                    scope.widget.updateActivityTemplateId(eventArgs.id, eventArgs.activityTemplateId);
                };
                return {
                    restrict: 'E',
                    template: '<div style="overflow: auto;"></div>',
                    scope: {},
                    link: function (scope, element, attrs) {
                        var factory = new ProcessBuilder.FabricJsFactory();
                        var widget = Core.create(ProcessBuilder.Widget, element.children()[0], factory, attrs.width, attrs.height);
                        widget.on('startNode:click', function () {
                            scope.$apply(function () {
                                scope.$emit(paneWorkflowDesigner.MessageType[paneWorkflowDesigner.MessageType.PaneWorkflowDesigner_TemplateSelected], new paneWorkflowDesigner.TemplateSelectedEventArgs());
                            });
                        });
                        widget.on('addCriteriaNode:click', function () {
                            scope.$apply(function () {
                                scope.$emit(paneWorkflowDesigner.MessageType[paneWorkflowDesigner.MessageType.PaneWorkflowDesigner_ProcessNodeTemplateAdding], new paneWorkflowDesigner.CriteriaAddingEventArgs());
                            });
                        });
                        widget.on('criteriaNode:click', function (e, criteriaId, isTempId) {
                            scope.$apply(function () {
                                scope.$emit(paneWorkflowDesigner.MessageType[paneWorkflowDesigner.MessageType.PaneWorkflowDesigner_CriteriaSelected], new paneWorkflowDesigner.CriteriaSelectedEventArgs(criteriaId, isTempId));
                            });
                        });
                        widget.on('addActionNode:click', function (e, criteriaId, actionType) {
                            scope.$apply(function () {
                                scope.$emit(paneWorkflowDesigner.MessageType[paneWorkflowDesigner.MessageType.PaneWorkflowDesigner_ActionAdding], new paneWorkflowDesigner.ActionAddingEventArgs(criteriaId, actionType));
                            });
                        });
                        widget.on('actionNode:click', function (e, criteriaId, actionId, actionType, activityTemplateId) {
                            scope.$apply(function () {
                                scope.$emit(paneWorkflowDesigner.MessageType[paneWorkflowDesigner.MessageType.PaneWorkflowDesigner_ActionSelected], new paneWorkflowDesigner.ActionSelectedEventArgs(criteriaId, actionId, actionType, activityTemplateId));
                            });
                        });
                        scope.widget = widget;
                        // Event handlers.
                        scope.$on(paneWorkflowDesigner.MessageType[paneWorkflowDesigner.MessageType.PaneWorkflowDesigner_Render], function (event, eventArgs) { return onRender(eventArgs, scope); });
                        scope.$on(paneWorkflowDesigner.MessageType[paneWorkflowDesigner.MessageType.PaneWorkflowDesigner_AddCriteria], function (event, eventArgs) { return onProcessNodeTemplateAdded(eventArgs, scope); });
                        scope.$on(paneWorkflowDesigner.MessageType[paneWorkflowDesigner.MessageType.PaneWorkflowDesigner_RemoveCriteria], function (event, eventArgs) { return onProcessNodeTemplateRemoved(eventArgs, scope); });
                        scope.$on(paneWorkflowDesigner.MessageType[paneWorkflowDesigner.MessageType.PaneWorkflowDesigner_AddAction], function (event, eventArgs) { return onActionAdded(eventArgs, scope); });
                        scope.$on(paneWorkflowDesigner.MessageType[paneWorkflowDesigner.MessageType.PaneWorkflowDesigner_ActionRemoved], function (event, eventArgs) { return onActionRemoved(eventArgs, scope); });
                        scope.$on(paneWorkflowDesigner.MessageType[paneWorkflowDesigner.MessageType.PaneWorkflowDesigner_ReplaceTempIdForProcessNodeTemplate], function (event, eventArgs) { return onProcessNodeTemplateTempIdReplaced(eventArgs, scope); });
                        scope.$on(paneWorkflowDesigner.MessageType[paneWorkflowDesigner.MessageType.PaneWorkflowDesigner_ActionTempIdReplaced], function (event, eventArgs) { return onActionTempIdReplaced(eventArgs, scope); });
                        scope.$on(paneWorkflowDesigner.MessageType[paneWorkflowDesigner.MessageType.PaneWorkflowDesigner_UpdateProcessNodeTemplateName], function (event, eventArgs) { return onProcessNodeTemplateRenamed(eventArgs, scope); });
                        scope.$on(paneWorkflowDesigner.MessageType[paneWorkflowDesigner.MessageType.PaneWorkflowDesigner_ActionNameUpdated], function (event, eventArgs) { return onActionRenamed(eventArgs, scope); });
                        scope.$on(paneWorkflowDesigner.MessageType[paneWorkflowDesigner.MessageType.PaneWorkflowDesigner_UpdateActivityTemplateId], function (event, eventArgs) { return onUpdateActivityTemplateIdForAction(eventArgs, scope); });
                    }
                };
            }
            paneWorkflowDesigner.PaneWorkflowDesigner = PaneWorkflowDesigner;
        })(paneWorkflowDesigner = directives.paneWorkflowDesigner || (directives.paneWorkflowDesigner = {}));
    })(directives = dockyard.directives || (dockyard.directives = {}));
})(dockyard || (dockyard = {}));
app.directive('paneWorkflowDesigner', dockyard.directives.paneWorkflowDesigner.PaneWorkflowDesigner);
//# sourceMappingURL=paneworkflowdesigner.js.map