/// <reference path="../../_all.ts" />
module dockyard.directives.counter {
    'use strict';

    export interface ICounterAttributes extends ng.IAttributes {
        minValue: string;
    }

    export interface ICounterScope extends ng.IScope {
        counterValue: number;
        counterTooltip: string;
        increment(): void;
        decrement(): void;
        adjustValue(): void;
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

        constructor() {
            Counter.prototype.link = (
                scope: ICounterScope,
                element: ng.IAugmentedJQuery,
                attrs: ICounterAttributes) => {

            }

            Counter.prototype.controller = (
                $scope: ICounterScope,
                $element: ng.IAugmentedJQuery,
                $attrs: ICounterAttributes) => {

                var minValue = parseInt($attrs.minValue);
                if (!isFinite(minValue)) minValue = -Infinity;

                var prevValue = $scope.counterValue;

                $scope.increment = () => {
                    $scope.counterValue++;
                    $scope.adjustValue();
                }

                $scope.decrement = () => {
                    $scope.counterValue--;
                    $scope.adjustValue();
                }

                $scope.adjustValue = () => {
                    if (!isFinite($scope.counterValue)) $scope.counterValue = prevValue;
                    if ($scope.counterValue < minValue) $scope.counterValue = minValue;
                }
            };

            Counter.prototype.controller['$inject'] = ['$scope', '$element', '$attrs'];

        };

        //The factory function returns Directive object as per Angular requirements
        public static Factory() {
            var directive = () => {
                return new Counter();
            };

            directive['$inject'] = [];
            return directive;
        }
    }

    app.directive('counter', Counter.Factory());
}