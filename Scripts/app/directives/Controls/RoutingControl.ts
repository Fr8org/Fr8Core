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
        //ChangeSelection: (scope: IRoutingControlScope) => void;
        ChangeSelection: (route: model.Route) => void;
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    class RoutingControl implements ng.IDirective {
        public link: (scope: IRoutingControlScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public controller: ($scope: IRoutingControlScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public scope = {
            route: '='
        };
        public templateUrl = '/AngularTemplate/RoutingControl';
        public restrict = 'E';
        private _$element: ng.IAugmentedJQuery;
        private _$scope: IRoutingControlScope;

        constructor() {
            RoutingControl.prototype.link = (
                $scope: IRoutingControlScope,
                $element: ng.IAugmentedJQuery,
                $attrs: ng.IAttributes) => {
            };

            RoutingControl.prototype.controller = (
                $scope: IRoutingControlScope,
                $element: ng.IAugmentedJQuery,
                $attrs: ng.IAttributes) => {

                this._$element = $element;
                this._$scope = $scope;

                //$scope.ChangeSelection = <(scope: IRoutingControlScope) => void> angular.bind(this, this.ChangeSelection);
                $scope.ChangeSelection = <(route: model.Route) => void> angular.bind(this, this.ChangeSelection);

            };
        }

        private ChangeSelection(route: model.Route) {
            debugger;
            this._$scope.route.selection = route.selection;
            //route.selection
        }

        //The factory function returns Directive object as per Angular requirements
        public static Factory() {
            var directive = () => {
                return new RoutingControl();
            };
            
            directive['$inject'] = [];
            return directive;
        }
    }

    app.directive('routingControl', RoutingControl.Factory());
}