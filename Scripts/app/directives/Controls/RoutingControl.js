/// <reference path="../../_all.ts" />
var dockyard;
(function (dockyard) {
    var directives;
    (function (directives) {
        var paneConfigureAction;
        (function (paneConfigureAction) {
            'use strict';
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
                    };
                }
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