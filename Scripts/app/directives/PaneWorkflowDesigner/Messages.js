/// <reference path="../../_all.ts" />
var dockyard;
(function (dockyard) {
    var directives;
    (function (directives) {
        var paneWorkflowDesigner;
        (function (paneWorkflowDesigner) {
            'use strict';
            (function (MessageType) {
                MessageType[MessageType["PaneWorkflowDesigner_Render"] = 0] = "PaneWorkflowDesigner_Render";
                MessageType[MessageType["PaneWorkflowDesigner_TemplateSelected"] = 1] = "PaneWorkflowDesigner_TemplateSelected";
                MessageType[MessageType["PaneWorkflowDesigner_CriteriaAdding"] = 2] = "PaneWorkflowDesigner_CriteriaAdding";
                MessageType[MessageType["PaneWorkflowDesigner_CriteriaAdded"] = 3] = "PaneWorkflowDesigner_CriteriaAdded";
                MessageType[MessageType["PaneWorkflowDesigner_CriteriaSelecting"] = 4] = "PaneWorkflowDesigner_CriteriaSelecting";
                MessageType[MessageType["PaneWorkflowDesigner_CriteriaRemoved"] = 5] = "PaneWorkflowDesigner_CriteriaRemoved";
                MessageType[MessageType["PaneWorkflowDesigner_ActionAdding"] = 6] = "PaneWorkflowDesigner_ActionAdding";
                MessageType[MessageType["PaneWorkflowDesigner_ActionAdded"] = 7] = "PaneWorkflowDesigner_ActionAdded";
                MessageType[MessageType["PaneWorkflowDesigner_ActionSelecting"] = 8] = "PaneWorkflowDesigner_ActionSelecting";
                MessageType[MessageType["PaneWorkflowDesigner_ActionRemoved"] = 9] = "PaneWorkflowDesigner_ActionRemoved";
            })(paneWorkflowDesigner.MessageType || (paneWorkflowDesigner.MessageType = {}));
            var MessageType = paneWorkflowDesigner.MessageType;
            var RenderEventArgs = (function () {
                function RenderEventArgs() {
                }
                return RenderEventArgs;
            })();
            paneWorkflowDesigner.RenderEventArgs = RenderEventArgs;
            var CriteriaAddingEventArgs = (function () {
                function CriteriaAddingEventArgs() {
                }
                return CriteriaAddingEventArgs;
            })();
            paneWorkflowDesigner.CriteriaAddingEventArgs = CriteriaAddingEventArgs;
            var CriteriaAddedEventArgs = (function () {
                function CriteriaAddedEventArgs(criteria) {
                    this.criteria = criteria;
                }
                return CriteriaAddedEventArgs;
            })();
            paneWorkflowDesigner.CriteriaAddedEventArgs = CriteriaAddedEventArgs;
            var CriteriaSelectingEventArgs = (function () {
                function CriteriaSelectingEventArgs(criteriaId) {
                    this.criteriaId = criteriaId;
                }
                return CriteriaSelectingEventArgs;
            })();
            paneWorkflowDesigner.CriteriaSelectingEventArgs = CriteriaSelectingEventArgs;
            var CriteriaRemovedEventArgs = (function () {
                function CriteriaRemovedEventArgs(criteriaId) {
                    this.criteriaId = criteriaId;
                }
                return CriteriaRemovedEventArgs;
            })();
            paneWorkflowDesigner.CriteriaRemovedEventArgs = CriteriaRemovedEventArgs;
            var ActionAddingEventArgs = (function () {
                function ActionAddingEventArgs(criteriaId) {
                    this.criteriaId = criteriaId;
                }
                return ActionAddingEventArgs;
            })();
            paneWorkflowDesigner.ActionAddingEventArgs = ActionAddingEventArgs;
            var ActionAddedEventArgs = (function () {
                function ActionAddedEventArgs(criteriaId, action) {
                    this.criteriaId = criteriaId;
                    this.action = action;
                }
                return ActionAddedEventArgs;
            })();
            paneWorkflowDesigner.ActionAddedEventArgs = ActionAddedEventArgs;
            var ActionSelectingEventArgs = (function () {
                function ActionSelectingEventArgs(criteriaId, actionId) {
                    this.criteriaId = criteriaId;
                    this.actionId = actionId;
                }
                return ActionSelectingEventArgs;
            })();
            paneWorkflowDesigner.ActionSelectingEventArgs = ActionSelectingEventArgs;
            var ActionRemovedEventArgs = (function () {
                function ActionRemovedEventArgs(criteriaId, actionId) {
                    this.criteriaId = criteriaId;
                    this.actionId = actionId;
                }
                return ActionRemovedEventArgs;
            })();
            paneWorkflowDesigner.ActionRemovedEventArgs = ActionRemovedEventArgs;
            var TemplateSelectedEventArgs = (function () {
                function TemplateSelectedEventArgs() {
                }
                return TemplateSelectedEventArgs;
            })();
            paneWorkflowDesigner.TemplateSelectedEventArgs = TemplateSelectedEventArgs;
        })(paneWorkflowDesigner = directives.paneWorkflowDesigner || (directives.paneWorkflowDesigner = {}));
    })(directives = dockyard.directives || (dockyard.directives = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=Messages.js.map