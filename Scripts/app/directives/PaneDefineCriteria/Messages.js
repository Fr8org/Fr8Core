var dockyard;
(function (dockyard) {
    var directives;
    (function (directives) {
        var paneDefineCriteria;
        (function (paneDefineCriteria) {
            (function (MessageType) {
                MessageType[MessageType["PaneDefineCriteria_Render"] = 0] = "PaneDefineCriteria_Render";
                MessageType[MessageType["PaneDefineCriteria_Hide"] = 1] = "PaneDefineCriteria_Hide";
                MessageType[MessageType["PaneDefineCriteria_CriteriaRemoved"] = 2] = "PaneDefineCriteria_CriteriaRemoved";
                MessageType[MessageType["PaneDefineCriteria_CriteriaUpdated"] = 3] = "PaneDefineCriteria_CriteriaUpdated";
                MessageType[MessageType["PaneDefineCriteria_Cancelled"] = 4] = "PaneDefineCriteria_Cancelled";
            })(paneDefineCriteria.MessageType || (paneDefineCriteria.MessageType = {}));
            var MessageType = paneDefineCriteria.MessageType;
            var RenderEventArgs = (function () {
                function RenderEventArgs(fields, criteria) {
                    this.fields = fields;
                    this.criteria = criteria;
                }
                return RenderEventArgs;
            })();
            paneDefineCriteria.RenderEventArgs = RenderEventArgs;
            var CriteriaRemovingEventArgs = (function () {
                function CriteriaRemovingEventArgs(criteriaId) {
                    this.criteriaId = criteriaId;
                }
                return CriteriaRemovingEventArgs;
            })();
            paneDefineCriteria.CriteriaRemovingEventArgs = CriteriaRemovingEventArgs;
        })(paneDefineCriteria = directives.paneDefineCriteria || (directives.paneDefineCriteria = {}));
    })(directives = dockyard.directives || (dockyard.directives = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=Messages.js.map