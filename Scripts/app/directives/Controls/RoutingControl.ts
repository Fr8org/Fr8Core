/// <reference path="../../_all.ts" />
module dockyard.directives.paneConfigureAction {
    'use strict';

    //enum routeType {
    //    textroute,
    //    checkboxroute,
    //    routing,
    //}

    interface IRoutingControlScope extends ng.IScope {
        route: model.Route;
        uniqueDirectiveId: number;
        //ChangeSelection: (scope: IRoutingControlScope) => void;
        //ChangeSelection: (route: model.Route) => void;
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    export function RoutingControl(): ng.IDirective {
        var uniqueDirectiveId = 1;
        var controller = ['$scope', '$element','$attrs', ($scope: IRoutingControlScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => {
            $scope.uniqueDirectiveId = ++uniqueDirectiveId;
            //var ChangeSelection = function (route: model.Route) {
            //    $scope.route.selection = route.selection;
            //    //route.selection
            //};
        }];

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

    app.directive('routingControl', RoutingControl);
}