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
                rows: '=',
                currentAction: '=',
                isDisabled: '='
            },

            controller: ['$scope', ($scope: interfaces.IQueryBuilderWidgetScope): void => {
                $scope.addRow = function () {
                    if ($scope.isDisabled)
                        return;
                    var condition = new model.Condition(
                        null,
                        $scope.defaultOperator,
                        null
                        );

                    $scope.rows.push(condition);
                };

                $scope.removeRow = function (index) {
                    if ($scope.isDisabled)
                        return;
                    $scope.rows.splice(index, 1);
                };

                $scope.valueChanged = function (row) {
                };

                $scope.isActionValid = function (action: interfaces.IActionVM) {
                    return model.ActivityDTO.isActionValid(action);
                }
            }]
        };
    }
}

app.directive('queryBuilderWidget', dockyard.directives.QueryBuilderWidget);
