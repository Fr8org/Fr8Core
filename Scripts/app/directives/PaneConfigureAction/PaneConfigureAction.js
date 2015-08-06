/// <reference path="../../_all.ts" />
var dockyard;
(function (dockyard) {
    var directives;
    (function (directives) {
        var paneConfigureAction;
        (function (paneConfigureAction) {
            'use strict';
            (function (MessageType) {
                MessageType[MessageType["PaneConfigureAction_ActionUpdated"] = 0] = "PaneConfigureAction_ActionUpdated";
                MessageType[MessageType["PaneConfigureAction_Render"] = 1] = "PaneConfigureAction_Render";
                MessageType[MessageType["PaneConfigureAction_Hide"] = 2] = "PaneConfigureAction_Hide";
                MessageType[MessageType["PaneConfigureAction_Cancelled"] = 3] = "PaneConfigureAction_Cancelled";
            })(paneConfigureAction.MessageType || (paneConfigureAction.MessageType = {}));
            var MessageType = paneConfigureAction.MessageType;
            var ActionUpdatedEventArgs = (function () {
                function ActionUpdatedEventArgs(criteriaId, actionId, actionTempId, processTemplateId) {
                    this.actionId = actionId;
                    this.criteriaId = criteriaId;
                    this.actionTempId = actionTempId;
                    this.processTemplateId = processTemplateId;
                }
                return ActionUpdatedEventArgs;
            })();
            paneConfigureAction.ActionUpdatedEventArgs = ActionUpdatedEventArgs;
            var RenderEventArgs = (function () {
                function RenderEventArgs(criteriaId, actionId, isTempId, processTemplateId) {
                    this.actionId = actionId;
                    this.criteriaId = criteriaId;
                    this.isTempId = isTempId;
                    this.processTemplateId = processTemplateId;
                }
                return RenderEventArgs;
            })();
            paneConfigureAction.RenderEventArgs = RenderEventArgs;
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
            var PaneConfigureAction = (function () {
                function PaneConfigureAction($rootScope) {
                    var _this = this;
                    this.$rootScope = $rootScope;
                    this.templateUrl = '/AngularTemplate/PaneConfigureAction';
                    this.scope = {};
                    this.restrict = 'E';
                    PaneConfigureAction.prototype.link = function (scope, element, attrs) {
                    };
                    PaneConfigureAction.prototype.controller = function ($scope, $element, $attrs) {
                        //Template function goes here
                        $scope.cancel = function (event) {
                            $scope.isVisible = false;
                            var eventArgs = new CancelledEventArgs($scope.action.criteriaId, $scope.action.id > 0 ? $scope.action.id : $scope.action.tempId, $scope.action.id < 0, 0);
                            $scope.$emit(MessageType[MessageType.PaneConfigureAction_Cancelled], eventArgs);
                        };
                        $scope.save = function (event) {
                            var eventArgs = new ActionUpdatedEventArgs($scope.action.criteriaId, $scope.action.id, $scope.action.tempId, 0);
                            $scope.$emit(MessageType[MessageType.PaneConfigureAction_ActionUpdated], eventArgs);
                            $.notify("Thank you, Action saved!", "success");
                        };
                        $scope.$watch(function (scope) { return scope.action; }, _this.onActionChanged, true);
                        $scope.$on(MessageType[MessageType.PaneConfigureAction_Render], _this.onRender);
                        $scope.$on(MessageType[MessageType.PaneConfigureAction_Hide], _this.onHide);
                    };
                }
                PaneConfigureAction.prototype.onActionChanged = function (newValue, oldValue, scope) {
                };
                PaneConfigureAction.prototype.onRender = function (event, eventArgs) {
                    var scope = event.currentScope;
                    scope.isVisible = true;
                    scope.action = new dockyard.model.Action(eventArgs.isTempId ? 0 : eventArgs.actionId, eventArgs.isTempId ? eventArgs.actionId : 0, eventArgs.criteriaId);
                };
                PaneConfigureAction.prototype.onHide = function (event, eventArgs) {
                    event.currentScope.isVisible = false;
                };
                PaneConfigureAction.Factory = function () {
                    var directive = function ($rootScope) {
                        return new PaneConfigureAction($rootScope);
                    };
                    directive['$inject'] = ['$rootScope'];
                    return directive;
                };
                return PaneConfigureAction;
            })();
            app.directive('paneConfigureAction', PaneConfigureAction.Factory());
        })(paneConfigureAction = directives.paneConfigureAction || (directives.paneConfigureAction = {}));
    })(directives = dockyard.directives || (dockyard.directives = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=paneconfigureaction.js.map