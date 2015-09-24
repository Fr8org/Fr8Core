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
                currentAction: '='
            },

            controller: ($scope: interfaces.IQueryBuilderWidgetScope): void => {
                $scope.addRow = function () {
                    var condition = new model.Condition(
                        tryFirstFieldKey($scope.fields),
                        $scope.defaultOperator,
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

                $scope.isActionValid = function (action: interfaces.IActionVM) {
                    return model.ActionDTO.isActionValid(action);
                }
            }
        };
    }
}

app.directive('queryBuilderWidget', dockyard.directives.QueryBuilderWidget);
