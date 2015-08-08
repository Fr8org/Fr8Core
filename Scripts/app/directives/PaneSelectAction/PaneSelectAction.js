/// <reference path="../../_all.ts" />
var dockyard;
(function (dockyard) {
    var directives;
    (function (directives) {
        var paneSelectAction;
        (function (paneSelectAction) {
            'use strict';
            //More detail on creating directives in TypeScript: 
            //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
            var PaneSelectAction = (function () {
                function PaneSelectAction($rootScope) {
                    var _this = this;
                    this.$rootScope = $rootScope;
                    this.templateUrl = '/AngularTemplate/PaneSelectAction';
                    this.scope = {};
                    this.restrict = 'E';
                    PaneSelectAction.prototype.link = function (scope, element, attrs) {
                        //Link function goes here
                    };
                    PaneSelectAction.prototype.controller = function ($scope, $element, $attrs) {
                        _this.PupulateSampleData($scope);
                        $scope.$watch(function (scope) { return scope.action; }, _this.onActionChanged, true);
                        $scope.ActionTypeSelected = function () {
                            var eventArgs = new paneSelectAction.ActionTypeSelectedEventArgs($scope.action.criteriaId, $scope.action.id, $scope.action.tempId, $scope.action.actionTypeId, 0);
                            $scope.$emit(paneSelectAction.MessageType[paneSelectAction.MessageType.PaneSelectAction_ActionTypeSelected], eventArgs);
                        };
                        $scope.$on(paneSelectAction.MessageType[paneSelectAction.MessageType.PaneSelectAction_Render], _this.onRender);
                        $scope.$on(paneSelectAction.MessageType[paneSelectAction.MessageType.PaneSelectAction_Hide], _this.onHide);
                        $scope.$on(paneSelectAction.MessageType[paneSelectAction.MessageType.PaneSelectAction_UpdateAction], _this.onUpdate);
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
                //The factory function returns Directive object as per Angular requirements
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
//# sourceMappingURL=PaneSelectAction.js.map