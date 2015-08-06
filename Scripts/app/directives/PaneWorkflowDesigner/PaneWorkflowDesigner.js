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
                var onCriteriaAdded = function (eventArgs, scope) {
                    console.log('PaneWorkflowDesigner::onCriteriaAdded', eventArgs);
                    scope.widget.addCriteria(eventArgs.criteria);
                    scope.$emit(paneWorkflowDesigner.MessageType[paneWorkflowDesigner.MessageType.PaneWorkflowDesigner_CriteriaSelecting], new paneWorkflowDesigner.CriteriaSelectingEventArgs(eventArgs.criteria.id));
                };
                var onCriteriaRemoved = function (eventArgs, scope) {
                    console.log('PaneWorkflowDesigner::onCriteriaRemoved', eventArgs);
                    scope.widget.removeCriteria(eventArgs.criteriaId);
                };
                var onActionAdded = function (eventArgs, scope) {
                    console.log('PaneWorkflowDesigner::onActionAdded', eventArgs);
                    scope.widget.addAction(eventArgs.criteriaId, eventArgs.action);
                    scope.$emit(paneWorkflowDesigner.MessageType[paneWorkflowDesigner.MessageType.PaneWorkflowDesigner_ActionSelecting], new paneWorkflowDesigner.ActionSelectingEventArgs(eventArgs.criteriaId, eventArgs.action.id));
                };
                var onActionRemoved = function (eventArgs, scope) {
                    console.log('PaneWorkflowDesigner::onActionRemove', eventArgs);
                    scope.widget.removeAction(eventArgs.criteriaId, eventArgs.actionId);
                };
                return {
                    restrict: 'E',
                    template: '<div style="overflow: auto;"></div>',
                    scope: {},
                    link: function (scope, element, attrs) {
                        var factory = new ProcessBuilder.FabricJsFactory();
                        var widget = Core.create(ProcessBuilder.Widget, element.children()[0], factory, attrs.width, attrs.height);
                        widget.on('addCriteriaNode:click', function () {
                            scope.$emit(paneWorkflowDesigner.MessageType[paneWorkflowDesigner.MessageType.PaneWorkflowDesigner_CriteriaAdding], new paneWorkflowDesigner.CriteriaAddingEventArgs());
                        });
                        widget.on('criteriaNode:click', function (e, criteriaId) {
                            scope.$emit(paneWorkflowDesigner.MessageType[paneWorkflowDesigner.MessageType.PaneWorkflowDesigner_CriteriaSelecting], new paneWorkflowDesigner.CriteriaSelectingEventArgs(criteriaId));
                        });
                        widget.on('addActionNode:click', function (e, criteriaId) {
                            scope.$emit(paneWorkflowDesigner.MessageType[paneWorkflowDesigner.MessageType.PaneWorkflowDesigner_ActionAdding], new paneWorkflowDesigner.ActionAddingEventArgs(criteriaId));
                        });
                        widget.on('actionNode:click', function (e, criteriaId, actionId) {
                            scope.$emit(paneWorkflowDesigner.MessageType[paneWorkflowDesigner.MessageType.PaneWorkflowDesigner_ActionSelecting], new paneWorkflowDesigner.ActionSelectingEventArgs(criteriaId, actionId));
                        });
                        scope.widget = widget;
                        // Event handlers.
                        scope.$on(paneWorkflowDesigner.MessageType[paneWorkflowDesigner.MessageType.PaneWorkflowDesigner_Render], function (event, eventArgs) { return onRender(eventArgs, scope); });
                        scope.$on(paneWorkflowDesigner.MessageType[paneWorkflowDesigner.MessageType.PaneWorkflowDesigner_CriteriaAdded], function (event, eventArgs) { return onCriteriaAdded(eventArgs, scope); });
                        scope.$on(paneWorkflowDesigner.MessageType[paneWorkflowDesigner.MessageType.PaneWorkflowDesigner_CriteriaRemoved], function (event, eventArgs) { return onCriteriaRemoved(eventArgs, scope); });
                        scope.$on(paneWorkflowDesigner.MessageType[paneWorkflowDesigner.MessageType.PaneWorkflowDesigner_ActionAdded], function (event, eventArgs) { return onActionAdded(eventArgs, scope); });
                    }
                };
            }
            paneWorkflowDesigner.PaneWorkflowDesigner = PaneWorkflowDesigner;
        })(paneWorkflowDesigner = directives.paneWorkflowDesigner || (directives.paneWorkflowDesigner = {}));
    })(directives = dockyard.directives || (dockyard.directives = {}));
})(dockyard || (dockyard = {}));
app.directive('paneWorkflowDesigner', dockyard.directives.paneWorkflowDesigner.PaneWorkflowDesigner);
//# sourceMappingURL=paneworkflowdesigner.js.map