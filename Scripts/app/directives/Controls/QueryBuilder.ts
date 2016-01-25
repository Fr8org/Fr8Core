/// <reference path="../../_all.ts" />

module dockyard.directives {
    'use strict';

    export function QueryBuilder(): ng.IDirective {
        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/QueryBuilder',
            scope: {
                currentAction: '=',
                field: '='
            },
            controller: ['$scope', '$timeout', 'CrateHelper',
                function (
                    $scope: IQueryBuilderScope,
                    $timeout: ng.ITimeoutService,
                    crateHelper: services.CrateHelper
                    ) {

                    $scope.operators = [
                        { text: '>', value: 'gt' },
                        { text: '>=', value: 'gte' },
                        { text: '<', value: 'lt' },
                        { text: '<=', value: 'lte' },
                        { text: '==', value: 'eq' },
                        { text: '<>', value: 'neq' }
                    ];

                    $scope.defaultOperator = '';

                    $scope.$watch('currentAction', function (newValue: model.ActivityDTO) {
                        if (newValue && newValue.crateStorage) {
                            var crate = crateHelper.findByManifestTypeAndLabel(
                                newValue.crateStorage, 'Standard Design-Time Fields', 'Queryable Criteria');

                            $scope.fields = [];
                            if (crate != null) {
                                var crateJson = <any> (crate.contents);
                                angular.forEach(crateJson.Fields, function (it) {
                                    $scope.fields.push({ name: it.key, key: it.key });
                                });
                            }
                        }
                    });

                    $scope.$watch('field', function (newValue: any) {

                        if (newValue && newValue.value) {
                            var jsonValue = angular.fromJson(newValue.value);
                            $scope.conditions = <Array<interfaces.ICondition>>jsonValue;
                        }
                        else {
                            $scope.conditions = [
                                new model.Condition(
                                    null,
                                    $scope.defaultOperator,
                                    null)
                            ];
                        }
                    });

                    var updateFieldValue = function () {
                        $scope.field.value = angular.toJson($scope.conditions);
                    };

                    $scope.$watch('conditions', function () {
                        updateFieldValue();
                    }, true);
                }
            ]
        }
    }

    export interface IQueryBuilderScope extends ng.IScope {
        field: any;
        fields: Array<interfaces.IField>;
        operators: Array<interfaces.IOperator>;
        defaultOperator: string;
        conditions: Array<interfaces.ICondition>;
    }
}

app.directive('queryBuilder', dockyard.directives.QueryBuilder);