var dockyard;
(function (dockyard) {
    var directives;
    (function (directives) {
        var paneSelectAction;
        (function (paneSelectAction) {
            (function (MessageType) {
                MessageType[MessageType["PaneSelectAction_ActionUpdated"] = 0] = "PaneSelectAction_ActionUpdated";
                MessageType[MessageType["PaneSelectAction_Render"] = 1] = "PaneSelectAction_Render";
                MessageType[MessageType["PaneSelectAction_Hide"] = 2] = "PaneSelectAction_Hide";
                MessageType[MessageType["PaneSelectAction_UpdateAction"] = 3] = "PaneSelectAction_UpdateAction";
                MessageType[MessageType["PaneSelectAction_ActionTypeSelected"] = 4] = "PaneSelectAction_ActionTypeSelected";
            })(paneSelectAction.MessageType || (paneSelectAction.MessageType = {}));
            var MessageType = paneSelectAction.MessageType;
            var ActionTypeSelectedEventArgs = (function () {
                function ActionTypeSelectedEventArgs(criteriaId, actionId, tempActionId, actionTypeId, processTemplateId) {
                    this.actionId = actionId;
                    this.criteriaId = criteriaId;
                    this.tempActionId = tempActionId;
                    this.actionTypeId = actionTypeId;
                    this.processTemplateId = processTemplateId;
                }
                return ActionTypeSelectedEventArgs;
            })();
            paneSelectAction.ActionTypeSelectedEventArgs = ActionTypeSelectedEventArgs;
            var ActionUpdatedEventArgs = (function () {
                function ActionUpdatedEventArgs(criteriaId, actionId, tempActionId, actionName, processTemplateId) {
                    this.actionId = actionId;
                    this.criteriaId = criteriaId;
                    this.tempActionId = tempActionId;
                    this.actionName = actionName;
                    this.processTemplateId = processTemplateId;
                }
                return ActionUpdatedEventArgs;
            })();
            paneSelectAction.ActionUpdatedEventArgs = ActionUpdatedEventArgs;
            var RenderEventArgs = (function () {
                function RenderEventArgs(criteriaId, actionId, isTemp, processTemplateId) {
                    this.actionId = actionId;
                    this.criteriaId = criteriaId;
                    this.isTempId = isTemp;
                    this.processTemplateId = processTemplateId;
                }
                return RenderEventArgs;
            })();
            paneSelectAction.RenderEventArgs = RenderEventArgs;
            var UpdateActionEventArgs = (function () {
                function UpdateActionEventArgs(criteriaId, actionId, actionTempId, processTemplateId) {
                    this.actionId = actionId;
                    this.criteriaId = criteriaId;
                    this.actionTempId = actionTempId;
                    this.processTemplateId = processTemplateId;
                }
                return UpdateActionEventArgs;
            })();
            paneSelectAction.UpdateActionEventArgs = UpdateActionEventArgs;
        })(paneSelectAction = directives.paneSelectAction || (directives.paneSelectAction = {}));
    })(directives = dockyard.directives || (dockyard.directives = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=messagetype.js.map