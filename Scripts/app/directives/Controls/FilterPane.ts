/// <reference path="../../_all.ts" />

module dockyard.directives {
    'use strict';

    export function FilterPane(): ng.IDirective {
        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/FilterPane',
            scope: {
                field: '='
            },
            controller: (
                $scope: IPaneDefineCriteriaScope,
                $timeout: ng.ITimeoutService
                ): void => {

                $scope.operators = [
                    { text: 'Greater than', value: 'gt' },
                    { text: 'Greater than or equal', value: 'gte' },
                    { text: 'Less than', value: 'lt' },
                    { text: 'Less than or equal', value: 'lte' },
                    { text: 'Equal', value: 'eq' },
                    { text: 'Not equal', value: 'neq' }
                ];

                $scope.defaultOperator = 'gt';

                $scope.$watch('field', function (newValue: any) {
                    if (newValue && newValue.value) {
                        var jsonValue = angular.fromJson(newValue.value);
                        $scope.conditions = <Array<interfaces.ICondition>>jsonValue.conditions;
                        $scope.executionType = jsonValue.executionType;
                    }
                    else {
                        $scope.conditions = [];
                        $scope.executionType = 1;
                    }
                });

                var updateFieldValue = function () {
                    $scope.field.value = angular.toJson({
                        executionType: $scope.executionType,
                        conditions: $scope.conditions
                    });
                };

                $scope.$watch('conditions', function () {
                    updateFieldValue();
                }, true);

                $scope.$watch('executionType', function () {
                    updateFieldValue();
                });
            }
        };
    }

    export interface IPaneDefineCriteriaScope extends ng.IScope {
        field: any;
        operators: Array<interfaces.IOperator>;
        defaultOperator: string;
        conditions: Array<interfaces.ICondition>;
        executionType: number;
    }
}

app.directive('filterPane', dockyard.directives.FilterPane);