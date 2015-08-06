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
        })(paneSelectAction = directives.paneSelectAction || (directives.paneSelectAction = {}));
    })(directives = dockyard.directives || (dockyard.directives = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=MessageType.js.map