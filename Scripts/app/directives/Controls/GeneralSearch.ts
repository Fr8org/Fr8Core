/// <reference path="../../_all.ts" />

module dockyard.directives {
    'use strict';

    export function GeneralSearch(): ng.IDirective {
        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/GeneralSearch',
            scope: {
                currentAction: '=',           
                field: '=',
                onsort: '='
            },
            controller: ['$scope', '$timeout', 'CrateHelper',
                function (
                    $scope: IGeneralSearchScope,
                    $timeout: ng.ITimeoutService,
                    crateHelper: services.CrateHelper
                    ) {
                   
                    $scope.operators = [
                        { text: 'Greater than', value: 'gt' },
                        { text: 'Greater than or equal', value: 'gte' },
                        { text: 'Less than', value: 'lt' },
                        { text: 'Less than or equal', value: 'lte' },
                        { text: 'Equal', value: 'eq' },
                        { text: 'Not equal', value: 'neq' }
                    ];

                    $scope.defaultOperator = '';     
      

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
                        }
                    });                
                }
            ]
        }
    }

    export interface IGeneralSearchScope extends ng.IScope {
        field: any;
        fields: Array<interfaces.IField>;
        operators: Array<interfaces.IOperator>;
        defaultOperator: string;
        conditions: Array<interfaces.ICondition>;
        executionType: number;
   

    }
}

app.directive('generalSearch', dockyard.directives.GeneralSearch); 