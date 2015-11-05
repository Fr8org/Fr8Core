/// <reference path="../../_all.ts" />

module dockyard.directives.designerHeader {
    'use strict';

    export interface IDesignerHeaderScope extends ng.IScope {
        onStateChange(): void;
        route: model.RouteDTO
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    class DesignerHeader implements ng.IDirective {
        public link: (scope: IDesignerHeaderScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public controller: ($scope: IDesignerHeaderScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;

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
                $attrs: ng.IAttributes) => {

                $scope.onStateChange = () => {
                    if ($scope.route.routeState === model.RouteState.Inactive) {
                        RouteService.deactivate($scope.route);
                    } else {
                        RouteService.activate($scope.route);
                    }
                };

                var unregister = $scope.$watch(function () {
                    if ($scope.route) {
                        var input: HTMLInputElement = <HTMLInputElement>$($element).find('input[data-bootstrap-switch]').get(0);
                        $($element).find('[data-bootstrap-switch]').bootstrapSwitch({
                            state: $scope.route.routeState === model.RouteState.Active,
                            onSwitchChange: () => {
                                if (input.checked) {
                                    RouteService.activate($scope.route);
                                } else {
                                    RouteService.deactivate($scope.route);
                                }
                            }
                        });

                        unregister();
                    }
                });
            };
        }

        //The factory function returns Directive object as per Angular requirements
        public static Factory() {
            var directive = (ProcessTemplateService: services.IRouteService) => {
                return new DesignerHeader(ProcessTemplateService);
            };

            directive['$inject'] = ['RouteService'];
            return directive;
        }
    }

    app.directive('designerHeader', DesignerHeader.Factory());
}