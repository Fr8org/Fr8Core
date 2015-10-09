/// <reference path="../../_all.ts" />
var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    __.prototype = b.prototype;
    d.prototype = new __();
};
var dockyard;
(function (dockyard) {
    var directives;
    (function (directives) {
        var paneSelectAction;
        (function (paneSelectAction) {
            'use strict';
            (function (MessageType) {
                MessageType[MessageType["PaneSelectAction_ActionUpdated"] = 0] = "PaneSelectAction_ActionUpdated";
                MessageType[MessageType["PaneSelectAction_Render"] = 1] = "PaneSelectAction_Render";
                MessageType[MessageType["PaneSelectAction_Hide"] = 2] = "PaneSelectAction_Hide";
                MessageType[MessageType["PaneSelectAction_UpdateAction"] = 3] = "PaneSelectAction_UpdateAction";
                MessageType[MessageType["PaneSelectAction_ActionTypeSelected"] = 4] = "PaneSelectAction_ActionTypeSelected";
                MessageType[MessageType["PaneSelectAction_InitiateSaveAction"] = 5] = "PaneSelectAction_InitiateSaveAction";
            })(paneSelectAction.MessageType || (paneSelectAction.MessageType = {}));
            var MessageType = paneSelectAction.MessageType;
            var ActionTypeSelectedEventArgs = (function () {
                function ActionTypeSelectedEventArgs(action) {
                    this.action = angular.extend({}, action);
                }
                return ActionTypeSelectedEventArgs;
            })();
            paneSelectAction.ActionTypeSelectedEventArgs = ActionTypeSelectedEventArgs;
            var ActionUpdatedEventArgs = (function (_super) {
                __extends(ActionUpdatedEventArgs, _super);
                function ActionUpdatedEventArgs(criteriaId, actionId, isTempId, actionName) {
                    _super.call(this, criteriaId, actionId);
                    this.isTempId = isTempId;
                    this.actionName = actionName;
                }
                return ActionUpdatedEventArgs;
            })(directives.ActionEventArgsBase);
            paneSelectAction.ActionUpdatedEventArgs = ActionUpdatedEventArgs;
            var RenderEventArgs = (function () {
                function RenderEventArgs(processNodeTemplateId, id, isTemp, actionListId) {
                    this.processNodeTemplateId = processNodeTemplateId;
                    this.id = id;
                    this.isTempId = isTemp;
                    this.actionListId = actionListId;
                }
                return RenderEventArgs;
            })();
            paneSelectAction.RenderEventArgs = RenderEventArgs;
            var UpdateActionEventArgs = (function (_super) {
                __extends(UpdateActionEventArgs, _super);
                function UpdateActionEventArgs(criteriaId, actionId, isTempId) {
                    _super.call(this, criteriaId, actionId);
                    this.isTempId = isTempId;
                }
                return UpdateActionEventArgs;
            })(directives.ActionEventArgsBase);
            paneSelectAction.UpdateActionEventArgs = UpdateActionEventArgs;
            var ActionRemovedEventArgs = (function () {
                function ActionRemovedEventArgs(id, isTempId) {
                    this.id = id;
                    this.isTempId = isTempId;
                }
                return ActionRemovedEventArgs;
            })();
            paneSelectAction.ActionRemovedEventArgs = ActionRemovedEventArgs;
        })(paneSelectAction = directives.paneSelectAction || (directives.paneSelectAction = {}));
    })(directives = dockyard.directives || (dockyard.directives = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=PaneSelectAction.js.map