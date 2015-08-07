var dockyard;
(function (dockyard) {
    var directives;
    (function (directives) {
        var paneWorkflowDesigner;
        (function (paneWorkflowDesigner) {
            (function (MessageType) {
                MessageType[MessageType["PaneWorkflowDesigner_CriteriaSelected"] = 0] = "PaneWorkflowDesigner_CriteriaSelected";
                MessageType[MessageType["PaneWorkflowDesigner_TemplateSelected"] = 1] = "PaneWorkflowDesigner_TemplateSelected";
                MessageType[MessageType["PaneWorkflowDesigner_ActionSelected"] = 2] = "PaneWorkflowDesigner_ActionSelected";
                MessageType[MessageType["PaneWorkflowDesigner_RefreshElement"] = 3] = "PaneWorkflowDesigner_RefreshElement";
                MessageType[MessageType["PaneWorkflowDesigner_UpdateAction"] = 4] = "PaneWorkflowDesigner_UpdateAction";
                MessageType[MessageType["PaneWorkflowDesigner_UpdateCriteriaName"] = 5] = "PaneWorkflowDesigner_UpdateCriteriaName";
            })(paneWorkflowDesigner.MessageType || (paneWorkflowDesigner.MessageType = {}));
            var MessageType = paneWorkflowDesigner.MessageType;
        })(paneWorkflowDesigner = directives.paneWorkflowDesigner || (directives.paneWorkflowDesigner = {}));
    })(directives = dockyard.directives || (dockyard.directives = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=MessageType.js.map