/// <reference path="../_all.ts" />

module dockyard.directives {
    'use strict';

    export function QueryBuilderWidget(): ng.IDirective {

        var tryFirstFieldKey = function (array: Array<model.Field>): string {
            if(array) {
                return array[0].key;
            }

            return null;
        };


        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/QueryBuilderWidget',
            scope: {
                fields: '=',
                operators: '=',
                defaultOperator: '=',
                rows: '='
            },

            controller: ($scope: interfaces.IQueryBuilderWidgetScope): void => {
                if(!$scope.operators) {
                    $scope.operators = [
                        { text: 'Greater than', value: 'gt' },
                        { text: 'Greater than or equal', value: 'gte' },
                        { text: 'Less than', value: 'lt' },
                        { text: 'Less than or equal', value: 'lte' },
                        { text: 'Equal', value: 'eq' },
                        { text: 'Not equal', value: 'neq' }
                    ];
                }

                if ($scope.rows) {
                    var condition = new model.Condition(
                        tryFirstFieldKey($scope.fields),
                        $scope.defaultOperator || 'gt',
                        null
                    );
                    condition.validate();

                    $scope.rows.push(condition);
                }

                $scope.addRow = function () {
                    var condition = new model.Condition(
                        tryFirstFieldKey($scope.fields),
                        $scope.defaultOperator || 'gt',
                        null
                        );
                    condition.validate();

                    $scope.rows.push(condition);
                };

                $scope.removeRow = function (index) {
                    $scope.rows.splice(index, 1);
                };

                $scope.valueChanged = function (row) {
                    row.valueError = !row.value;
                };
            }
        };
    }
}

app.directive('queryBuilderWidget', dockyard.directives.QueryBuilderWidget);
