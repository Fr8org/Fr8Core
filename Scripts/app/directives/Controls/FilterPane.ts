/// <reference path="../../_all.ts" />

module dockyard.directives {
    'use strict';

    export function FilterPane(): ng.IDirective {
        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/FilterPane',
            scope: {
                currentAction: '=',
                field: '='
            },
            controller: ['$scope', '$timeout', 'CrateHelper',
                function (
                    $scope: IPaneDefineCriteriaScope,
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

                    $scope.$watch('currentAction', function (newValue: model.ActionDTO) {
                        if (newValue && newValue.crateStorage) {
                            var crate = crateHelper.findByManifestTypeAndLabel(
                                newValue.crateStorage, 'Standard Design-Time Fields', 'Queryable Criteria');

                            $scope.fields = [];
                            if (crate != null) {
                                var crateJson = angular.fromJson(crate.contents);
                                angular.forEach(crateJson.Fields, function (it) {
                                    $scope.fields.push({ name: it.Key, key: it.Key });
                                });
                            }
                        }
                    });

                    $scope.$watch('field', function (newValue: any) {

                        if (newValue && newValue.value) {
                            var jsonValue = angular.fromJson(newValue.value);
                            $scope.conditions = <Array<interfaces.ICondition>>jsonValue.conditions;
                            $scope.executionType = jsonValue.executionType;
                        }
                        else {
                            $scope.conditions = [
                                new model.Condition(
                                    null,
                                    $scope.defaultOperator,
                                    null)
                            ];
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
            ]
        }
    }

    export interface IPaneDefineCriteriaScope extends ng.IScope {
        field: any;
        fields: Array<interfaces.IField>;
        operators: Array<interfaces.IOperator>;
        defaultOperator: string;
        conditions: Array<interfaces.ICondition>;
        executionType: number;
    }
}

app.directive('filterPane', dockyard.directives.FilterPane);