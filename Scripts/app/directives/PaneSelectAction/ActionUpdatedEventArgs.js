/// <reference path="../../_all.ts" />
var dockyard;
(function (dockyard) {
    var directives;
    (function (directives) {
        var paneSelectAction;
        (function (paneSelectAction) {
            'use strict';
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
        })(paneSelectAction = directives.paneSelectAction || (directives.paneSelectAction = {}));
    })(directives = dockyard.directives || (dockyard.directives = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=ActionUpdatedEventArgs.js.map