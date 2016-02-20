/// <reference path="../../_all.ts" />

module dockyard.directives.designerHeader {
    'use strict';

    export interface IDesignerHeaderScope extends ng.IScope {
        editing: boolean;
        editTitle(): void;
        onTitleChange(): void;
        runRoute(): void;
        deactivatePlan(): void;

        route: model.RouteDTO;
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    class DesignerHeader implements ng.IDirective {
        public link: (scope: IDesignerHeaderScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public controller: (
            $scope: IDesignerHeaderScope,
            element: ng.IAugmentedJQuery,
            attrs: ng.IAttributes,
            ngToast: any,
            RouteService: services.IRouteService
        ) => void;

        public templateUrl = '/AngularTemplate/DesignerHeader';
        public scope = {
            route: '='
        };
        public restrict = 'E';

        constructor(private RouteService: services.IRouteService) {
            DesignerHeader.prototype.link = (
                scope: IDesignerHeaderScope,
                element: ng.IAugmentedJQuery,
                attrs: ng.IAttributes) => {

                //Link function goes here
            };

            DesignerHeader.prototype.controller = (
                $scope: IDesignerHeaderScope,
                $element: ng.IAugmentedJQuery,
                $attrs: ng.IAttributes,
                ngToast: any,
                RouteService: services.IRouteService) => {

                $scope.editTitle = () => {
                    $scope.editing = true;
                };

                $scope.onTitleChange = () => {
                    $scope.editing = false;
                    var result = RouteService.update({ id: $scope.route.id, name: $scope.route.name });
                    result.$promise.then(() => { });
                };

                $scope.runRoute = () => {
                    var promise = RouteService.runAndProcessClientAction($scope.route.id);
                    promise.then(() => {
                        // mark plan as active
                        $scope.route.routeState = 2;
                    });
                };

                $scope.deactivatePlan = () => {
                    var result = RouteService.deactivate({ planId: $scope.route.id });
                    result.$promise.then((data) => {
                        // mark plan as inactive
                        $scope.route.routeState = 1;
                        var messageToShow = "Plan successfully deactivated";
                        ngToast.success(messageToShow);
                    })
                        .catch((err: any) => {
                            var messageToShow = "Failed to toggle Plan Status";
                            ngToast.danger(messageToShow);
                        });
                };
            };

            DesignerHeader.prototype.controller['$inject'] = ['$scope', '$element', '$attrs', 'ngToast', 'RouteService'];
        }

        //The factory function returns Directive object as per Angular requirements
        public static Factory() {
            var directive = (RouteService: services.IRouteService) => {
                return new DesignerHeader(RouteService);
            };

            directive['$inject'] = ['RouteService'];
            return directive;
        }
    }

    app.directive('designerHeader', DesignerHeader.Factory());
}