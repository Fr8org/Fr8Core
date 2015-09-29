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
                MessageType[MessageType["PaneWorkflowDesigner_ProcessNodeTemplateAdding"] = 2] = "PaneWorkflowDesigner_ProcessNodeTemplateAdding";
                MessageType[MessageType["PaneWorkflowDesigner_AddCriteria"] = 3] = "PaneWorkflowDesigner_AddCriteria";
                MessageType[MessageType["PaneWorkflowDesigner_CriteriaSelected"] = 4] = "PaneWorkflowDesigner_CriteriaSelected";
                MessageType[MessageType["PaneWorkflowDesigner_RemoveCriteria"] = 5] = "PaneWorkflowDesigner_RemoveCriteria";
                MessageType[MessageType["PaneWorkflowDesigner_UpdateProcessNodeTemplateName"] = 6] = "PaneWorkflowDesigner_UpdateProcessNodeTemplateName";
                MessageType[MessageType["PaneWorkflowDesigner_ActionAdding"] = 7] = "PaneWorkflowDesigner_ActionAdding";
                MessageType[MessageType["PaneWorkflowDesigner_AddAction"] = 8] = "PaneWorkflowDesigner_AddAction";
                MessageType[MessageType["PaneWorkflowDesigner_ActionSelected"] = 9] = "PaneWorkflowDesigner_ActionSelected";
                MessageType[MessageType["PaneWorkflowDesigner_ActionRemoved"] = 10] = "PaneWorkflowDesigner_ActionRemoved";
                MessageType[MessageType["PaneWorkflowDesigner_ActionNameUpdated"] = 11] = "PaneWorkflowDesigner_ActionNameUpdated";
                MessageType[MessageType["PaneWorkflowDesigner_ReplaceTempIdForProcessNodeTemplate"] = 12] = "PaneWorkflowDesigner_ReplaceTempIdForProcessNodeTemplate";
                MessageType[MessageType["PaneWorkflowDesigner_ActionTempIdReplaced"] = 13] = "PaneWorkflowDesigner_ActionTempIdReplaced";
                MessageType[MessageType["PaneWorkflowDesigner_UpdateActivityTemplateId"] = 14] = "PaneWorkflowDesigner_UpdateActivityTemplateId";
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
            var AddCriteriaEventArgs = (function () {
                function AddCriteriaEventArgs(id, isTempId, name) {
                    this.id = id;
                    this.isTempId = isTempId;
                    this.name = name;
                }
                return AddCriteriaEventArgs;
            })();
            paneWorkflowDesigner.AddCriteriaEventArgs = AddCriteriaEventArgs;
            var CriteriaSelectedEventArgs = (function () {
                function CriteriaSelectedEventArgs(id, isTempId) {
                    this.id = id;
                    this.isTempId = isTempId;
                }
                return CriteriaSelectedEventArgs;
            })();
            paneWorkflowDesigner.CriteriaSelectedEventArgs = CriteriaSelectedEventArgs;
            var RemoveCriteriaEventArgs = (function () {
                function RemoveCriteriaEventArgs(id, isTempId) {
                    this.id = id;
                    this.isTempId = isTempId;
                }
                return RemoveCriteriaEventArgs;
            })();
            paneWorkflowDesigner.RemoveCriteriaEventArgs = RemoveCriteriaEventArgs;
            var UpdateProcessNodeTemplateNameEventArgs = (function () {
                function UpdateProcessNodeTemplateNameEventArgs(id, text) {
                    this.id = id;
                    this.text = text;
                }
                return UpdateProcessNodeTemplateNameEventArgs;
            })();
            paneWorkflowDesigner.UpdateProcessNodeTemplateNameEventArgs = UpdateProcessNodeTemplateNameEventArgs;
            var ActionAddingEventArgs = (function () {
                function ActionAddingEventArgs(processNodeTemplateId, actionListType) {
                    this.processNodeTemplateId = processNodeTemplateId;
                    this.actionListType = actionListType;
                }
                return ActionAddingEventArgs;
            })();
            paneWorkflowDesigner.ActionAddingEventArgs = ActionAddingEventArgs;
            var AddActionEventArgs = (function () {
                function AddActionEventArgs(criteriaId, action, actionListType) {
                    this.criteriaId = criteriaId;
                    this.action = action;
                    this.actionListType = actionListType;
                }
                return AddActionEventArgs;
            })();
            paneWorkflowDesigner.AddActionEventArgs = AddActionEventArgs;
            var ActionSelectedEventArgs = (function () {
                function ActionSelectedEventArgs(processNodeTemplateId, actionId, actionListId, activityTemplateId) {
                    this.processNodeTemplateId = processNodeTemplateId;
                    this.actionId = actionId;
                    this.actionListId = actionListId;
                    this.activityTemplateId = activityTemplateId;
                }
                return ActionSelectedEventArgs;
            })();
            paneWorkflowDesigner.ActionSelectedEventArgs = ActionSelectedEventArgs;
            var ActionRemovedEventArgs = (function () {
                function ActionRemovedEventArgs(id, isTempId) {
                    this.id = id;
                    this.isTempId = isTempId;
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
            var ActionNameUpdatedEventArgs = (function () {
                function ActionNameUpdatedEventArgs(id, name) {
                    this.id = id;
                    this.name = name;
                }
                return ActionNameUpdatedEventArgs;
            })();
            paneWorkflowDesigner.ActionNameUpdatedEventArgs = ActionNameUpdatedEventArgs;
            var ReplaceTempIdForProcessNodeTemplateEventArgs = (function () {
                function ReplaceTempIdForProcessNodeTemplateEventArgs(tempId, id) {
                    this.tempId = tempId;
                    this.id = id;
                }
                return ReplaceTempIdForProcessNodeTemplateEventArgs;
            })();
            paneWorkflowDesigner.ReplaceTempIdForProcessNodeTemplateEventArgs = ReplaceTempIdForProcessNodeTemplateEventArgs;
            var ActionTempIdReplacedEventArgs = (function () {
                function ActionTempIdReplacedEventArgs(tempId, id) {
                    this.tempId = tempId;
                    this.id = id;
                }
                return ActionTempIdReplacedEventArgs;
            })();
            paneWorkflowDesigner.ActionTempIdReplacedEventArgs = ActionTempIdReplacedEventArgs;
            var UpdateActivityTemplateIdEventArgs = (function () {
                function UpdateActivityTemplateIdEventArgs(id, activityTemplateId) {
                    this.id = id;
                    this.activityTemplateId = activityTemplateId;
                }
                return UpdateActivityTemplateIdEventArgs;
            })();
            paneWorkflowDesigner.UpdateActivityTemplateIdEventArgs = UpdateActivityTemplateIdEventArgs;
        })(paneWorkflowDesigner = directives.paneWorkflowDesigner || (directives.paneWorkflowDesigner = {}));
    })(directives = dockyard.directives || (dockyard.directives = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=messages.js.map