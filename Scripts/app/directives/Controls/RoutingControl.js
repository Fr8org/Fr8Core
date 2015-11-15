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
            function RoutingControl() {
                var uniqueDirectiveId = 1;
                var controller = function ($scope, element, attrs) {
                    $scope.uniqueDirectiveId = ++uniqueDirectiveId;
                    //var ChangeSelection = function (route: model.Route) {
                    //    $scope.route.selection = route.selection;
                    //    //route.selection
                    //};
                };
                //The factory function returns Directive object as per Angular requirements
                return {
                    restrict: 'E',
                    templateUrl: '/AngularTemplate/RoutingControl',
                    controller: controller,
                    scope: {
                        route: '='
                    }
                };
            }
            paneConfigureAction.RoutingControl = RoutingControl;
            app.directive('routingControl', RoutingControl);
        })(paneConfigureAction = directives.paneConfigureAction || (directives.paneConfigureAction = {}));
    })(directives = dockyard.directives || (dockyard.directives = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=RoutingControl.js.map