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
            $rootScope: interfaces.IAppRootScope,
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
                $rootScope: interfaces.IAppRootScope,
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
                    // mark plan as Active
                    $scope.route.routeState = 2;
                    var promise = RouteService.runAndProcessClientAction($scope.route.id);
                    promise.finally(() => {
                        var subRoute = $scope.route.subroutes[0];
                        var initialActivity = subRoute ? subRoute.activities[0] : null;
                        if (initialActivity && initialActivity.activityTemplate.category.toLowerCase() !== "monitors") {
                            // mark plan as Inactive
                            $scope.route.routeState = 1;
                        }
                        // This is to notify dashboad/view all page to reArrangeRoutes themselves so that plans get rendered in desired sections i.e Running or Plans Library
                        // This is required when user Run a plan and immediately navigates(before run completion) to dashboad or view all page in order 
                        // to make sure plans get rendered in desired sections
                        if (location.href.indexOf('/builder') === -1) {
                            $rootScope.$broadcast("planExecutionCompleted-rearrangePlans", $scope.route);
                            //$scope.$root.$broadcast("planExecutionCompleted", $scope.route);
                        }
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

            DesignerHeader.prototype.controller['$inject'] = ['$rootScope', '$scope', '$element', '$attrs', 'ngToast', 'RouteService'];
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