/// <reference path="../../_all.ts" />
var dockyard;
(function (dockyard) {
    var directives;
    (function (directives) {
        var paneSelectAction;
        (function (paneSelectAction) {
            'use strict';
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
        })(paneSelectAction = directives.paneSelectAction || (directives.paneSelectAction = {}));
    })(directives = dockyard.directives || (dockyard.directives = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=ActionTypeSelectedEventArgs.js.map