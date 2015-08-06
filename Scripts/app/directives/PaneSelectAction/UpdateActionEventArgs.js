/// <reference path="../../_all.ts" />
var dockyard;
(function (dockyard) {
    var directives;
    (function (directives) {
        var paneSelectAction;
        (function (paneSelectAction) {
            'use strict';
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
