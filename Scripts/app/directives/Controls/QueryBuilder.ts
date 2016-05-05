/// <reference path="../../_all.ts" />

module dockyard.directives {
    'use strict';

    export interface IQueryField {
        name: string;
        label: string;
        fieldType: string;
    }

    export interface IQueryOperator {
        text: string;
        value: string;
    }

    export interface IQueryCondition {
        field: model.FieldDTO;
        operator: string;
        value: string;
    }

    export interface ISerializedCondition {
        field: string;
        operator: string;
        value: string;
    }

    export interface IQueryBuilderScope extends ng.IScope {
        currentAction: model.ActivityDTO;
        field: any;
        fields: Array<model.FieldDTO>;
        operators: Array<IQueryOperator>;
        defaultOperator: string;
        conditions: Array<IQueryCondition>;
        rows: Array<interfaces.ICondition>;

        addCondition: () => void;
        removeCondition: (index: number) => void;
        addRowText: string;
    }

    export function QueryBuilder(): ng.IDirective {
        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/QueryBuilder',
            scope: {
                currentAction: '=',
                field: '=',
                rows: '=?',
                isDisabled: '=',
                addRowText: '@'
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

                    $scope.addRowText = $scope.addRowText || 'Add Row';
                    $scope.defaultOperator = '';

                    $scope.$watch('currentAction', (newValue: model.ActivityDTO) => {
                        if (newValue && newValue.crateStorage) {
                            var crate = crateHelper.findByManifestTypeAndLabel(
                                newValue.crateStorage,
                                'Field Description',
                                'Queryable Criteria'
                            );
                    
                            $scope.fields = [];
                            if (crate != null) {
                                var crateJson = <any>(crate.contents);
                                angular.forEach(crateJson.Fields, function (it) {
                                    $scope.fields.push(it);
                                });

                                if ($scope.rows) {
                                    $scope.conditions = [];
                                    angular.forEach($scope.rows, function (it) {
                                        $scope.conditions.push({
                                            field: findField(it.field),
                                            operator: it.operator,
                                            value: it.value
                                        });
                                    });
                                }
                            }
                        }
                    });

                    $scope.$watch('field', (newValue: any) => {
                        if (newValue && newValue.value) {
                            var jsonValue = angular.fromJson(newValue.value);
                            var serializedConditions = <Array<ISerializedCondition>>jsonValue;

                            var conditions: Array<IQueryCondition> = [];

                            if (serializedConditions.length) {
                                angular.forEach(serializedConditions, (cond) => {
                                    conditions.push({
                                        field: findField(cond.field),
                                        operator: cond.operator,
                                        value: cond.value
                                    });
                                });

                                $scope.conditions = conditions;
                            } else {
                                addEmptyCondition();
                            }
                        }
                        else {
                            if (!$scope.conditions || !$scope.conditions.length) {
                                addEmptyCondition();
                            }
                        }
                    });

                    var addEmptyCondition = () => {
                        if (!$scope.conditions) {
                            $scope.conditions = [];
                        }

                        var condition = {
                            field: null,
                            operator: $scope.defaultOperator,
                            value: null
                        };

                        $scope.conditions.push(condition);
                    };

                    var findField = (name): model.FieldDTO => {
                        var i;
                        for (i = 0; i < $scope.fields.length; ++i) {
                            if ($scope.fields[i].key === name) {
                                return $scope.fields[i];
                            }
                        }

                        return null;
                    };

                    var updateFieldValue = () => {
                        var toBeSerialized = [];
                        angular.forEach($scope.conditions, function (cond) {
                            if (!cond.field) { return; }

                            toBeSerialized.push({
                                field: cond.field.key,
                                operator: cond.operator,
                                value: cond.value
                            });
                        });

                        if ($scope.field) {
                            $scope.field.value = angular.toJson(toBeSerialized);
                        }

                        if ($scope.rows) {
                            $scope.rows.splice(0, $scope.rows.length);
                            angular.forEach(toBeSerialized, function (cond) {
                                $scope.rows.push(cond);
                            });
                        }
                    };

                    $scope.$watch('conditions', () => {
                        updateFieldValue();
                    }, true);

                    $scope.addCondition = () => {
                        addEmptyCondition();
                    };

                    $scope.removeCondition = (index: number) => {
                        $scope.conditions.splice(index, 1);
                    };
                }
            ]
        };
    }
}

app.directive('queryBuilder', dockyard.directives.QueryBuilder);
