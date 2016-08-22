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
        requestUpstream: boolean;

        addCondition: () => void;
        removeCondition: (index: number) => void;
        addRowText: string;
        change: () => (field: model.ControlDefinitionDTO) => void;
    }

    export function QueryBuilder(): ng.IDirective {
        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/QueryBuilder',
            scope: {
                currentAction: '=',
                field: '=',
                rows: '=?',
                requestUpstream: '=?',
                isDisabled: '=',
                addRowText: '@',
                change: '='
            },
            controller: ['$scope', '$timeout', 'CrateHelper','UpstreamExtractor',
                function (
                    $scope: IQueryBuilderScope,
                    $timeout: ng.ITimeoutService,
                    crateHelper: services.CrateHelper,
                    UpstreamExtractor: services.UpstreamExtractor
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
                            if ($scope.fields.length === 0 && $scope.requestUpstream) {
                                loadUpstreamFields();
                            }
                        }
                    });

                    var loadUpstreamFields = () => {
                        var availabilityType = 'NotSet';

                        return UpstreamExtractor
                            .getAvailableData($scope.currentAction.id, availabilityType)
                            .then((data: model.IncomingCratesDTO) => {
                                var fields: Array<model.FieldDTO> = [];

                                angular.forEach(data.availableCrates, (ct) => {
                                    angular.forEach(ct.fields, (f) => {
                                        var i, j;
                                        var found = false;
                                        for (i = 0; i < fields.length; ++i) {
                                            if (fields[i].key === f.key) {
                                                found = true;
                                                break;
                                            }
                                        }
                                        if (!found) {
                                            fields.push(f);
                                        }
                                    });
                                });

                                fields.sort((x, y) => {
                                    if (x.key < y.key) {
                                        return -1;
                                    }
                                    else if (x.key > y.key) {
                                        return 1;
                                    }
                                    else {
                                        return 0;
                                    }
                                });

                                $scope.fields = fields;
                            });
                    };

                    $scope.$watch('field', (newValue: any) => {
                        if (newValue && newValue.value) {
                            var jsonValue = angular.fromJson(newValue.value);
                            var serializedConditions = <Array<ISerializedCondition>>jsonValue;

                            if (serializedConditions.length) {
                                innitializeConditions(serializedConditions);
                            }
                            else if ((<any>serializedConditions).conditions) {
                                if ((<any>serializedConditions).conditions.length) {
                                    innitializeConditions((<any>serializedConditions).conditions);        
                                }
                            }
                            else {
                                addEmptyCondition();
                            }
                        }
                        else {
                            if (!$scope.conditions || !$scope.conditions.length) {
                                addEmptyCondition();
                            }
                        }
                    });

                    var innitializeConditions = (serializedConditions: Array<ISerializedCondition>) => {
                        var conditions: Array<IQueryCondition> = [];
                        if ($scope.requestUpstream) {
                            loadUpstreamFields().then(() => { //parameter isSilent false, since we want to see error messages
                                angular.forEach(serializedConditions, (cond) => {
                                    conditions.push({
                                        field: findField(cond.field),
                                        operator: cond.operator,
                                        value: cond.value
                                    });
                                });
                            });

                        }
                        else {
                            angular.forEach(serializedConditions, (cond) => {
                                conditions.push({
                                    field: findField(cond.field),
                                    operator: cond.operator,
                                    value: cond.value
                                });
                            });
                        }

                        $scope.conditions = conditions;
                    }

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

                    if (angular.isUndefined($scope.requestUpstream)
                        && !angular.isUndefined($scope.field)
                        && $scope.field.source) {
                        $scope.requestUpstream = $scope.field.source.requestUpstream;
                    }
                }
            ]
        };
    }
}

app.directive('queryBuilder', dockyard.directives.QueryBuilder);
