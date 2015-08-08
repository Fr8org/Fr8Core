/// <reference path="../../_all.ts" />
var dockyard;
(function (dockyard) {
    var directives;
    (function (directives) {
        var paneConfigureAction;
        (function (paneConfigureAction) {
            'use strict';
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
//# sourceMappingURL=CancelledEventArgs.js.map