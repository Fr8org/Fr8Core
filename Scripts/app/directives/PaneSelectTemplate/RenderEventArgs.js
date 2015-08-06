/// <reference path="../../_all.ts" />
var dockyard;
(function (dockyard) {
    var directives;
    (function (directives) {
        var paneSelectTemplate;
        (function (paneSelectTemplate) {
            'use strict';
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
//# sourceMappingURL=RenderEventArgs.js.map