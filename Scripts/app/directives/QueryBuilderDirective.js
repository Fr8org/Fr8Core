'use strict';

app.directive('queryBuilder', function () {
    var template =
        '<div>\
            <table class="table">\
                <thead>\
                    <tr>\
                        <th class="col-md-1"></th>\
                        <th class="col-md-3">Field</th>\
                        <th class="col-md-3">Operator</th>\
                        <th class="col-md-4">Value</th>\
                        <th class="col-md-1"></th>\
                    </tr>\
                </thead>\
                <tbody>\
                    <tr ng-repeat="row in rows track by $index">\
                        <td>\
                            <div class="text-right">\
                                <div class="form-control-static">{{$index + 1}}</div>\
                            </div>\
                        </td>\
                        <td>\
                            <select class="form-control" ng-options="f.key as f.name for f in fields" ng-model="row.field"></select>\
                        </td>\
                        <td>\
                            <select class="form-control" ng-options="op.value as op.text for op in operators" ng-model="row.operator"></select>\
                        </td>\
                        <td>\
                            <div ng-class="{ \'has-error\': row.valueError }">\
                                <input type="text" \
                                    class="form-control" \
                                    ng-required="true" \
                                    ng-model="row.value" \
                                    ng-change="valueChanged(row)" />\
                            </div>\
                        </td>\
                        <td>\
                            <a href title="Delete filter rule" \
                                class="btn btn-danger" \
                                ng-show="rows.length > 1" \
                                ng-click="removeRow($index)">\
                                Delete\
                            </a>\
                        </td>\
                    </tr>\
                </tbody>\
            </table>\
            <div>\
                <a href class="btn btn-primary" ng-click="addRow()">Add Row</a>\
            </div>\
        </div>';

    var tryFirstFieldKey = function (array) {
        if (array) {
            return array[0].key;
        }

        return null;
    };

    var controller = function ($scope) {
        if (!$scope.fields) {
            $scope.fields = [];
        }

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

        $scope.rows = [
            {
                field: tryFirstFieldKey($scope.fields),
                operator: $scope.defaultOperator || 'gt',
                value: null,
                valueError: true
            }
        ];

        $scope.addRow = function () {
            $scope.rows.push({
                field: tryFirstFieldKey($scope.fields),
                operator: $scope.defaultOperator || 'gt',
                value: null,
                valueError: true
            });
        };

        $scope.removeRow = function (index) {
            $scope.rows.splice(index, 1);
        };

        $scope.valueChanged = function (row) {
            row.valueError = !row.value;
        };
    };

    return {
        restrict: 'E',
        template: template,
        controller: controller,
        scope: {
            fields: '=',
            operators: '=',
            defaultOperator: '='
        }
    };
});
