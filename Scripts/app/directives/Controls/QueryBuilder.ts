/// <reference path="../../_all.ts" />

module dockyard.directives {
    'use strict';

    export interface IQueryField {
        name: string;
        label: string;
        fieldType: string;
        control: model.ControlDefinitionDTO;
    }

    export interface IQueryOperator {
        text: string;
        value: string;
    }

    export interface IQueryCondition {
        field: IQueryField;
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
        fields: Array<IQueryField>;
        operators: Array<IQueryOperator>;
        defaultOperator: string;
        conditions: Array<IQueryCondition>;

        addCondition: () => void;
        removeCondition: (index: number) => void;
    }

    export interface IQueryBuilderConditionScope extends ng.IScope {
        currentAction: model.ActivityDTO;
        fields: Array<IQueryField>;
        operators: Array<IQueryOperator>;
        condition: IQueryCondition;
        isSingle: boolean;
        hasConfigurationControl: boolean;

        fieldSelected: () => void;
        onRemoveCondition: () => void;

        rootElem: any;
    }

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

                    // TODO: remove.
                    // var extractType = function (field) {
                    //     if (!field.tags) {
                    //         return 'string';
                    //     }
                    // 
                    //     var predefinedTypes = ['string', 'date'];
                    //     if (!predefinedTypes.indexOf(field.tags)) {
                    //         return 'string';
                    //     }
                    // 
                    //     return field.tags;
                    // };

                    $scope.$watch('currentAction', (newValue: model.ActivityDTO) => {
                        if (newValue && newValue.crateStorage) {
                            var crate = crateHelper.findByManifestTypeAndLabel(
                                newValue.crateStorage,
                                'Standard Query Fields',
                                'Queryable Criteria'
                            );
                    
                            $scope.fields = [];
                            if (crate != null) {
                                var crateJson = <any>(crate.contents);
                                angular.forEach(crateJson.Fields, function (it) {
                                    $scope.fields.push({
                                        name: it.Name,
                                        label: it.Label,
                                        fieldType: it.FieldType,
                                        control: it.Control
                                    });
                                });
                            }
                        }
                    });

                    $scope.$watch('field', (newValue: any) => {
                        if (newValue && newValue.value) {
                            var jsonValue = angular.fromJson(newValue.value);
                            var serializedConditions = <Array<ISerializedCondition>>jsonValue;

                            var conditions: Array<IQueryCondition> = [];

                            angular.forEach(serializedConditions, (cond) => {
                                conditions.push({
                                    field: findField(cond.field),
                                    operator: cond.operator,
                                    value: null
                                });
                            });

                            $scope.conditions = conditions;
                        }
                        else {
                            addEmptyCondition();
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

                    var findField = (name): IQueryField => {
                        var i;
                        for (i = 0; i < $scope.fields.length; ++i) {
                            if ($scope.fields[i].name === name) {
                                return $scope.fields[i];
                            }
                        }

                        return null;
                    };

                    // var isValidDate = (value) => {
                    //     if (!value) { return true; }
                    // 
                    //     var tokens = value.split('-');
                    //     if (tokens.length !== 3) { return false; }
                    // 
                    //     if (tokens[0].length !== 2
                    //         || tokens[1].length !== 2
                    //         || tokens[2].length !== 4) {
                    //         return false;
                    //     }
                    // 
                    //     var days = parseInt(tokens[0]);
                    //     if (days < 1 || days > 31) { return false; }
                    // 
                    //     var months = parseInt(tokens[1]);
                    //     if (months < 1 || months > 12) { return false; }
                    // 
                    //     return true;
                    // };

                    // var extractConditionValue = (cond: interfaces.ICondition) => {
                    //     var field = findField(cond.field);
                    //     if (field === null) {
                    //         return null;
                    //     }
                    // 
                    //     var result = null;
                    // 
                    //     if (field.type === 'date') {
                    //         if (isValidDate(cond.value)) {
                    //             result = cond.value;
                    //             cond.valueError = null;
                    //         }
                    //         else {
                    //             cond.valueError = 'Date format: "dd-mm-yyyy"';
                    //         }
                    //     }
                    //     else {
                    //         result = cond.value;
                    //     }
                    // 
                    //     return result;
                    // };

                    var updateFieldValue = () => {
                        var toBeSerialized = [];
                        // angular.forEach($scope.conditions, function (cond) {
                        //     toBeSerialized.push({
                        //         field: cond.field,
                        //         operator: cond.operator,
                        //         value: extractConditionValue(cond)
                        //     });
                        // });

                        $scope.field.value = angular.toJson(toBeSerialized);
                    };

                    $scope.$watch('conditions', () => {
                        updateFieldValue();
                        console.log($scope.conditions);
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

    export function QueryBuilderCondition(): ng.IDirective {
        return {
            restrict: 'A',
            replace: true,
            templateUrl: '/AngularTemplate/QueryBuilderCondition',
            scope: {
                currentAction: '=',
                condition: '=',
                fields: '=',
                operators: '=',
                isSingle: '=',
                onRemoveCondition: '&'
            },
            link: (scope: IQueryBuilderConditionScope,
                elem: ng.IAugmentedJQuery,
                attr: ng.IAttributes) => {

                scope.rootElem = elem;
            },
            controller: ['$rootScope', '$scope', '$compile',
                ($rootScope: ng.IRootScopeService,
                    $scope: IQueryBuilderConditionScope,
                    $compile: ng.ICompileService) => {

                    var configurationControl = null;

                    var attachControl = () => {
                        if (configurationControl) {
                            configurationControl.scope.$destroy();
                            configurationControl.markup.remove();

                            configurationControl = null;
                            $scope.hasConfigurationControl = false;
                        }

                        if (!$scope.condition.field) {
                            return;
                        }

                        var configurationControlScope = $scope.$new();
                        (<any>configurationControlScope).control =
                            angular.copy($scope.condition.field.control);

                        (<any>configurationControlScope).control.value = $scope.condition.value;
                        (<any>configurationControlScope).currentAction = $scope.currentAction;

                        $scope.hasConfigurationControl = true;

                        $compile('<configuration-control current-action="currentAction" field="control" />')
                            (configurationControlScope, (markup, scope) => {
                                $('.condition-control', $scope.rootElem).append(markup);

                                scope.$watch('control', function (newValue: any) {
                                    $scope.condition.value = newValue.value;
                                }, true);

                                configurationControl = {
                                    scope: scope,
                                    markup: markup
                                };
                            });
                    };

                    $scope.fieldSelected = () => {
                        attachControl();
                    };

                    $scope.$watch('condition.value', function (value) {
                        if (configurationControl) {
                            configurationControl.scope.control.value = value;
                        }
                    });

                    attachControl();
                }
            ]
        };
    }
}

app.directive('queryBuilder', dockyard.directives.QueryBuilder);
app.directive('queryBuilderCondition', dockyard.directives.QueryBuilderCondition);
