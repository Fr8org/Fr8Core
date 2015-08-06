/// <reference path="../_all.ts" />
var dockyard;
(function (dockyard) {
    var directives;
    (function (directives) {
        'use strict';
        function QueryBuilderWidget() {
            var tryFirstFieldKey = function (array) {
                if (array) {
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
                controller: function ($scope) {
                    if (!$scope.operators) {
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
                        var condition = new dockyard.model.Condition(tryFirstFieldKey($scope.fields), $scope.defaultOperator || 'gt', null);
                        condition.validate();
                        $scope.rows.push(condition);
                    }
                    $scope.addRow = function () {
                        var condition = new dockyard.model.Condition(tryFirstFieldKey($scope.fields), $scope.defaultOperator || 'gt', null);
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
        directives.QueryBuilderWidget = QueryBuilderWidget;
    })(directives = dockyard.directives || (dockyard.directives = {}));
})(dockyard || (dockyard = {}));
app.directive('queryBuilderWidget', dockyard.directives.QueryBuilderWidget);
//# sourceMappingURL=querybuilderwidget.js.map