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
            var RoutingControl = (function () {
                function RoutingControl() {
                    var _this = this;
                    this.scope = {
                        route: '='
                    };
                    this.templateUrl = '/AngularTemplate/RoutingControl';
                    this.restrict = 'E';
                    RoutingControl.prototype.link = function ($scope, $element, $attrs) {
                    };
                    RoutingControl.prototype.controller = function ($scope, $element, $attrs) {
                        _this._$element = $element;
                        _this._$scope = $scope;
                        //$scope.ChangeSelection = <(scope: IRoutingControlScope) => void> angular.bind(this, this.ChangeSelection);
                        //$scope.ChangeSelection = <(route: model.Route) => void> angular.bind(this, this.ChangeSelection);
                    };
                }
                //private ChangeSelection(route: model.Route) {
                //    debugger;
                //    this._$scope.route.selection = route.selection;
                //    //route.selection
                //}
                //The factory function returns Directive object as per Angular requirements
                RoutingControl.Factory = function () {
                    var directive = function () {
                        return new RoutingControl();
                    };
                    directive['$inject'] = [];
                    return directive;
                };
                return RoutingControl;
            })();
            app.directive('routingControl', RoutingControl.Factory());
        })(paneConfigureAction = directives.paneConfigureAction || (directives.paneConfigureAction = {}));
    })(directives = dockyard.directives || (dockyard.directives = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=RoutingControl.js.map