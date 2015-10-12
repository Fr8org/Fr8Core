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
                var onActionAdded = function (eventArgs, scope) {
                    console.log('PaneWorkflowDesigner::onActionAdded', eventArgs);
                    var actionObj = eventArgs.action;
                    scope.widget.addAction(eventArgs.criteriaId, eventArgs.action, 1);
                    if (eventArgs.doNotRaiseSelectedEvent)
                        return;
                    scope.$emit(paneWorkflowDesigner.MessageType[paneWorkflowDesigner.MessageType.PaneWorkflowDesigner_ActionSelected], new paneWorkflowDesigner.ActionSelectedEventArgs(eventArgs.criteriaId, eventArgs.action.id, 0));
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
                        widget.on('addActionNode:click', function (e, criteriaId, actionType) {
                            scope.$apply(function () {
                                scope.$emit(paneWorkflowDesigner.MessageType[paneWorkflowDesigner.MessageType.PaneWorkflowDesigner_ActionAdding], new paneWorkflowDesigner.ActionAddingEventArgs(criteriaId));
                            });
                        });
                        widget.on('actionNode:click', function (e, criteriaId, actionId, actionType, activityTemplateId) {
                            scope.$apply(function () {
                                scope.$emit(paneWorkflowDesigner.MessageType[paneWorkflowDesigner.MessageType.PaneWorkflowDesigner_ActionSelected], new paneWorkflowDesigner.ActionSelectedEventArgs(criteriaId, actionId, activityTemplateId));
                            });
                        });
                        scope.widget = widget;
                        // Event handlers.
                        scope.$on(paneWorkflowDesigner.MessageType[paneWorkflowDesigner.MessageType.PaneWorkflowDesigner_Render], function (event, eventArgs) { return onRender(eventArgs, scope); });
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