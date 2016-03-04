/// <reference path="../../_all.ts" />
module dockyard.directives.paneConfigureAction {
    'use strict';

    //enum planType {
    //    textplan,
    //    checkboxplan,
    //    routing,
    //}

    interface IRoutingControlScope extends ng.IScope {
        plan: model.Plan;
        uniqueDirectiveId: number;
        //ChangeSelection: (scope: IRoutingControlScope) => void;
        //ChangeSelection: (plan: model.Plan) => void;
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    export function RoutingControl(): ng.IDirective {
        var uniqueDirectiveId = 1;
        var controller = ['$scope', '$element','$attrs', ($scope: IRoutingControlScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => {
            $scope.uniqueDirectiveId = ++uniqueDirectiveId;
            //var ChangeSelection = function (route: model.Route) {
            //    $scope.plan.selection = plan.selection;
            //    //plan.selection
            //};
        }];

        //The factory function returns Directive object as per Angular requirements
        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/RoutingControl',
            controller: controller,
            scope: {
                plan: '='
            }
        };
    }

    app.directive('routingControl', RoutingControl);
}