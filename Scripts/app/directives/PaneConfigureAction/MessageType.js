var dockyard;
(function (dockyard) {
    var directives;
    (function (directives) {
        var paneConfigureAction;
        (function (paneConfigureAction) {
            (function (MessageType) {
                MessageType[MessageType["PaneConfigureAction_ActionUpdated"] = 0] = "PaneConfigureAction_ActionUpdated";
                MessageType[MessageType["PaneConfigureAction_Render"] = 1] = "PaneConfigureAction_Render";
                MessageType[MessageType["PaneConfigureAction_Hide"] = 2] = "PaneConfigureAction_Hide";
                MessageType[MessageType["PaneConfigureAction_Cancelled"] = 3] = "PaneConfigureAction_Cancelled";
            })(paneConfigureAction.MessageType || (paneConfigureAction.MessageType = {}));
            var MessageType = paneConfigureAction.MessageType;
            var ActionUpdatedEventArgs = (function () {
                function ActionUpdatedEventArgs(criteriaId, actionId, actionTempId, processTemplateId) {
                    this.actionId = actionId;
                    this.criteriaId = criteriaId;
                    this.actionTempId = actionTempId;
                    this.processTemplateId = processTemplateId;
                }
                return ActionUpdatedEventArgs;
            })();
            paneConfigureAction.ActionUpdatedEventArgs = ActionUpdatedEventArgs;
            var RenderEventArgs = (function () {
                function RenderEventArgs(criteriaId, actionId, isTempId, processTemplateId) {
                    this.actionId = actionId;
                    this.criteriaId = criteriaId;
                    this.isTempId = isTempId;
                    this.processTemplateId = processTemplateId;
                }
                return RenderEventArgs;
            })();
            paneConfigureAction.RenderEventArgs = RenderEventArgs;
            var CancelledEventArgs = (function () {
                function CancelledEventArgs(criteriaId, actionId, isTemp, processTemplateId) {
                    this.actionId = actionId;
                    this.criteriaId = criteriaId;
                    this.isTempId = isTemp;
                    this.processTemplateId = processTemplateId;
                }
                return CancelledEventArgs;
            })();
            paneConfigureAction.CancelledEventArgs = CancelledEventArgs;
        })(paneConfigureAction = directives.paneConfigureAction || (directives.paneConfigureAction = {}));
    })(directives = dockyard.directives || (dockyard.directives = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=messagetype.js.map