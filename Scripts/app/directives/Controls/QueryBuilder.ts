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

                    var extractType = function (field) {
                        if (!field.tags) {
                            return 'string';
                        }

                        var predefinedTypes = ['string', 'date'];
                        if (!predefinedTypes.indexOf(field.tags)) {
                            return 'string';
                        }

                        return field.tags;
                    };

                    $scope.$watch('currentAction', function (newValue: model.ActivityDTO) {
                        if (newValue && newValue.crateStorage) {
                            var crate = crateHelper.findByManifestTypeAndLabel(
                                newValue.crateStorage, 'Standard Design-Time Fields', 'Queryable Criteria');

                            $scope.fields = [];
                            if (crate != null) {
                                var crateJson = <any> (crate.contents);
                                angular.forEach(crateJson.Fields, function (it) {
                                    $scope.fields.push({ name: it.key, key: it.key, type: extractType(it) });
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

                    var findField = function (name) {
                        var i;
                        for (i = 0; i < $scope.fields.length; ++i) {
                            if ($scope.fields[i].key === name) {
                                return $scope.fields[i];
                            }
                        }

                        return null;
                    };

                    var isValidDate = function (value) {
                        if (!value) { return true; }

                        var tokens = value.split('-');
                        if (tokens.length !== 3) { return false; }

                        if (tokens[0].length !== 2
                            || tokens[1].length !== 2
                            || tokens[2].length !== 4) {
                            return false;
                        }

                        var days = parseInt(tokens[0]);
                        if (days < 1 || days > 31) { return false; }

                        var months = parseInt(tokens[1]);
                        if (months < 1 || months > 12) { return false; }

                        return true;
                    };

                    var extractConditionValue = function (cond: interfaces.ICondition) {
                        var field = findField(cond.field);
                        if (field === null) {
                            return null;
                        }

                        var result = null;

                        if (field.type === 'date') {
                            if (isValidDate(cond.value)) {
                                result = cond.value;
                                cond.valueError = null;
                            }
                            else {
                                cond.valueError = 'Date format: "dd-mm-yyyy"';
                            }
                        }
                        else {
                            result = cond.value;
                        }

                        return result;
                    };

                    var updateFieldValue = function () {
                        var toBeSerialized = [];
                        angular.forEach($scope.conditions, function (cond) {
                            toBeSerialized.push({
                                field: cond.field,
                                operator: cond.operator,
                                value: extractConditionValue(cond)
                            });
                        });

                        $scope.field.value = angular.toJson(toBeSerialized);
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
        fields: Array<any>;
        operators: Array<interfaces.IOperator>;
        defaultOperator: string;
        conditions: Array<interfaces.ICondition>;
    }
}

app.directive('queryBuilder', dockyard.directives.QueryBuilder);