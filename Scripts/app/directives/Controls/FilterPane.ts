/// <reference path="../../_all.ts" />

module dockyard.directives {
    'use strict';

    export function FilterPane(): ng.IDirective {
        var uniqueDirectiveId = 1;
        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/FilterPane',
            scope: {
                currentAction: '=',
                field: '=',
                change: '&'
            },
            controller: ['$scope', '$timeout', 'CrateHelper',
                function (
                    $scope: IPaneDefineCriteriaScope,
                    $timeout: ng.ITimeoutService,
                    crateHelper: services.CrateHelper
                ) {
                    $scope.uniqueDirectiveId = ++uniqueDirectiveId;

                    // TODO: FR-2393, remove this.
                    // $scope.operators = [
                    //     { text: '>', value: 'gt' },
                    //     { text: '>=', value: 'gte' },
                    //     { text: '<', value: 'lt' },
                    //     { text: '<=', value: 'lte' },
                    //     { text: '==', value: 'eq' },
                    //     { text: '<>', value: 'neq' }
                    // ];

                    // TODO: FR-2393, remove this.
                    // $scope.defaultOperator = '';

                    // $scope.$watch('currentAction', function (newValue: model.ActivityDTO) {
                    //     if (newValue && newValue.crateStorage) {
                    //         var crate = crateHelper.findByManifestTypeAndLabel(
                    //             newValue.crateStorage, 'Standard Design-Time Fields', 'Queryable Criteria');
                    // 
                    //         $scope.fields = [];
                    //         if (crate != null) {
                    //             var crateJson = <any>(crate.contents);
                    //             angular.forEach(crateJson.Fields, function (it) {
                    //                 $scope.fields.push({
                    //                     name: it.Name,
                    //                     label: it.Label,
                    //                     fieldType: it.FieldType,
                    //                     control: it.Control
                    //                 });
                    //             });
                    //         }
                    //     }
                    // });

                    $scope.conditions = [];

                    // var findField = (name): IQueryField => {
                    //     if (!$scope.fields) {
                    //         return null;
                    //     }
                    // 
                    //     var i;
                    //     for (i = 0; i < $scope.fields.length; ++i) {
                    //         if ($scope.fields[i].name === name) {
                    //             return $scope.fields[i];
                    //         }
                    //     }
                    // 
                    //     return null;
                    // };

                    $scope.$watch('field', function (newValue: any) {
                        if (newValue && newValue.value) {
                            debugger;
                            var jsonValue = angular.fromJson(newValue.value);

                            $scope.conditions = jsonValue.conditions;
                            if (!$scope.conditions) {
                                $scope.conditions = [];
                            }

                            $scope.executionType = jsonValue.executionType;
                        }
                        else {
                            $scope.conditions = [
                                {
                                    field: null,
                                    operator: '',
                                    value: null
                                }
                            ];
                            $scope.executionType = 2;
                        }
                    });

                    var updateFieldValue = function () {
                        $scope.field.value = angular.toJson({
                            executionType: $scope.executionType,
                            conditions: $scope.conditions
                        });

                        debugger;
                    };

                    $scope.$watch('conditions', () => { 
                        updateFieldValue();
                    }, true);

                    $scope.$watch('executionType', (oldValue, newValue) => {
                        updateFieldValue();

                        if (oldValue === newValue) {
                            return;
                        }

                        // Invoke onChange event handler
                        if ($scope.change != null && angular.isFunction($scope.change)) {
                            $scope.change()($scope.field);
                        }
                    });
                }
            ]
        }
    }

    export interface IPaneDefineCriteriaScope extends ng.IScope {
        field: any;
        change: () => (field: model.ControlDefinitionDTO) => void;
        fields: Array<IQueryField>;
        // TODO: FR-2393, remove this.
        // operators: Array<interfaces.IOperator>;
        // defaultOperator: string;
        conditions: Array<IQueryCondition>;
        executionType: number;
        uniqueDirectiveId: number;
    }
}

app.directive('filterPane', dockyard.directives.FilterPane);