var dockyard;
(function (dockyard) {
    var directives;
    (function (directives) {
        var paneSelectTemplate;
        (function (paneSelectTemplate) {
            (function (MessageType) {
                MessageType[MessageType["PaneSelectTemplate_ProcessTemplateUpdated"] = 0] = "PaneSelectTemplate_ProcessTemplateUpdated";
                MessageType[MessageType["PaneSelectTemplate_Render"] = 1] = "PaneSelectTemplate_Render";
                MessageType[MessageType["PaneSelectTemplate_Hide"] = 2] = "PaneSelectTemplate_Hide";
            })(paneSelectTemplate.MessageType || (paneSelectTemplate.MessageType = {}));
            var MessageType = paneSelectTemplate.MessageType;
            var RenderEventArgs = (function () {
                function RenderEventArgs(processTemplateId) {
                    this.processTemplateId = processTemplateId;
                }
                return RenderEventArgs;
            })();
            paneSelectTemplate.RenderEventArgs = RenderEventArgs;
        })(paneSelectTemplate = directives.paneSelectTemplate || (directives.paneSelectTemplate = {}));
    })(directives = dockyard.directives || (dockyard.directives = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=messagetype.js.map