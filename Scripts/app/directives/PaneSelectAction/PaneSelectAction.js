/// <reference path="../../_all.ts" />
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
            var PaneSelectAction = (function () {
                function PaneSelectAction($rootScope) {
                    var _this = this;
                    this.$rootScope = $rootScope;
                    this.templateUrl = '/AngularTemplate/PaneSelectAction';
                    this.scope = {};
                    this.restrict = 'E';
                    PaneSelectAction.prototype.link = function (scope, element, attrs) {
                    };
                    PaneSelectAction.prototype.controller = function ($scope, $element, $attrs) {
                        _this.PupulateSampleData($scope);
                        $scope.$watch(function (scope) { return scope.action; }, _this.onActionChanged, true);
                        $scope.ActionTypeSelected = function () {
                            var eventArgs = new ActionTypeSelectedEventArgs($scope.action.criteriaId, $scope.action.id, $scope.action.tempId, $scope.action.actionTypeId, 0);
                            $scope.$emit(MessageType[MessageType.PaneSelectAction_ActionTypeSelected], eventArgs);
                        };
                        $scope.$on(MessageType[MessageType.PaneSelectAction_Render], _this.onRender);
                        $scope.$on(MessageType[MessageType.PaneSelectAction_Hide], _this.onHide);
                        $scope.$on(MessageType[MessageType.PaneSelectAction_UpdateAction], _this.onUpdate);
                    };
                }
                PaneSelectAction.prototype.onActionChanged = function (newValue, oldValue, scope) {
                };
                PaneSelectAction.prototype.onRender = function (event, eventArgs) {
                    var scope = event.currentScope;
                    scope.isVisible = true;
                    scope.action = new dockyard.model.Action(eventArgs.isTempId ? 0 : eventArgs.actionId, eventArgs.isTempId ? eventArgs.actionId : 0, eventArgs.criteriaId);
                };
                PaneSelectAction.prototype.onHide = function (event, eventArgs) {
                    event.currentScope.isVisible = false;
                };
                PaneSelectAction.prototype.onUpdate = function (event, eventArgs) {
                    $.notify("Greetings from Select Action Pane. I've got a message about my neighbor saving its data so I saved my data, too.", "success");
                };
                PaneSelectAction.prototype.PupulateSampleData = function ($scope) {
                    $scope.sampleActionTypes = [
                        { name: "Action type 1", value: "1" },
                        { name: "Action type 2", value: "2" },
                        { name: "Action type 3", value: "3" }
                    ];
                };
                PaneSelectAction.Factory = function () {
                    var directive = function ($rootScope) {
                        return new PaneSelectAction($rootScope);
                    };
                    directive['$inject'] = ['$rootScope'];
                    return directive;
                };
                return PaneSelectAction;
            })();
            app.directive('paneSelectAction', PaneSelectAction.Factory());
        })(paneSelectAction = directives.paneSelectAction || (directives.paneSelectAction = {}));
    })(directives = dockyard.directives || (dockyard.directives = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=paneselectaction.js.map