/// <reference path="../../_all.ts" />
var dockyard;
(function (dockyard) {
    var directives;
    (function (directives) {
        var paneConfigureAction;
        (function (paneConfigureAction) {
            'use strict';
            //More detail on creating directives in TypeScript: 
            //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
            var PaneConfigureAction = (function () {
                function PaneConfigureAction($rootScope) {
                    var _this = this;
                    this.$rootScope = $rootScope;
                    this.templateUrl = '/AngularTemplate/PaneConfigureAction';
                    this.scope = {};
                    this.restrict = 'E';
                    PaneConfigureAction.prototype.link = function (scope, element, attrs) {
                        //Link function goes here
                    };
                    PaneConfigureAction.prototype.controller = function ($scope, $element, $attrs) {
                        //Template function goes here
                        $scope.cancel = function (event) {
                            $scope.isVisible = false;
                            var eventArgs = new paneConfigureAction.CancelledEventArgs($scope.action.criteriaId, $scope.action.id > 0 ? $scope.action.id : $scope.action.tempId, $scope.action.id < 0, 0);
                            $scope.$emit(paneConfigureAction.MessageType[paneConfigureAction.MessageType.PaneConfigureAction_Cancelled], eventArgs);
                        };
                        $scope.save = function (event) {
                            var eventArgs = new paneConfigureAction.ActionUpdatedEventArgs($scope.action.criteriaId, $scope.action.id, $scope.action.tempId, 0);
                            $scope.$emit(paneConfigureAction.MessageType[paneConfigureAction.MessageType.PaneConfigureAction_ActionUpdated], eventArgs);
                            $.notify("Thank you, Action saved!", "success");
                        };
                        $scope.$watch(function (scope) { return scope.action; }, _this.onActionChanged, true);
                        $scope.$on(paneConfigureAction.MessageType[paneConfigureAction.MessageType.PaneConfigureAction_Render], _this.onRender);
                        $scope.$on(paneConfigureAction.MessageType[paneConfigureAction.MessageType.PaneConfigureAction_Hide], _this.onHide);
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
                //The factory function returns Directive object as per Angular requirements
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
//# sourceMappingURL=PaneConfigureAction.js.map