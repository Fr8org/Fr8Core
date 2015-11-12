/// <reference path="../../_all.ts" />
module dockyard.directives.counter {
    'use strict';

    export interface ICounterAttributes extends ng.IAttributes {
        minValue: number;
    }

    export interface ICounterScope extends ng.IScope {
        counterValue: number;
        counterTooltip: string;
        increment(): void;
        decrement(): void;
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    class Counter implements ng.IDirective {
        public link: (scope: ICounterScope, element: ng.IAugmentedJQuery, attrs: ICounterAttributes) => void;
        public controller: ($scope: ICounterScope, element: ng.IAugmentedJQuery, attrs: ICounterAttributes) => void;

        public scope = {
            counterValue: '=',
            counterTooltip: '=',
        }

        public templateUrl = '/AngularTemplate/Counter';
        public restrict = 'E';

        private minValue: number;

        constructor($parse: ng.IParseService) {
            Counter.prototype.link = (
                scope: ICounterScope,
                element: ng.IAugmentedJQuery,
                attrs: ICounterAttributes) => {

                this.minValue = attrs.minValue || -Infinity;

            }

            Counter.prototype.controller = (
                $scope: ICounterScope,
                $element: ng.IAugmentedJQuery,
                $attrs: ICounterAttributes) => {

                $scope.increment = () => {
                    $scope.counterValue++;
                    if (!$scope.counterValue && $scope.counterValue !== 0) $scope.counterValue = 1;
                }

                $scope.decrement = () => {
                    $scope.counterValue--;
                    if (!$scope.counterValue && $scope.counterValue !== 0) $scope.counterValue = -1;
                    if ($scope.counterValue < this.minValue) $scope.counterValue = this.minValue;
                }

            }

        };

        //The factory function returns Directive object as per Angular requirements
        public static Factory() {
            var directive = ($parse: ng.IParseService) => {
                return new Counter($parse);
            };

            directive['$inject'] = ['$parse'];
            return directive;
        }
    }

    app.directive('counter', Counter.Factory());
}