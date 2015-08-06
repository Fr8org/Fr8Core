/// <reference path="../_all.ts" />
var dockyard;
(function (dockyard) {
    var directives;
    (function (directives) {
        'use strict';
        //More detail on creating directives in TypeScript: 
        //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
        var PaneSelectAction = (function () {
            function PaneSelectAction($rootScope) {
                var _this = this;
                this.$rootScope = $rootScope;
                this.templateUrl = '/AngularTemplate/PaneSelectAction';
                this.scope = {
                    criteriaList: '='
                };
                this.restrict = 'E';
                PaneSelectAction.prototype.link = function (scope, element, attrs) {
                    //Link function goes here
                };
                PaneSelectAction.prototype.controller = function ($scope, $element, $attrs) {
                    $scope.$watch(function (scope) { return scope.action; }, _this.onActionChanged, true);
                };
            }
            PaneSelectAction.prototype.onActionChanged = function (newValue, oldValue, scope) {
                alert('action changed');
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
    })(directives = dockyard.directives || (dockyard.directives = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=PaneSelectAction.js.map