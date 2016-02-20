/// <reference path="../../_all.ts" />

module dockyard.directives {
    'use strict';

    export interface IDatePickerScope extends ng.IScope {
        field: any;
        fieldValue: string;
        formatError: boolean;
    }

    export function DatePicker(): ng.IDirective {
        return {
            restrict: 'E',
            template:
                '<label class="control-label" ng-if="field.label">{{::field.label}}</label>'
                + '<input type="text" ng-model="fieldValue" ng-model-options="{ updateOn: \'default blur\', debounce: { \'default\': 500, \'blur\': 0 } }" '
                    + 'ng-blur="onChange($event)" ng-required="field.required" data-field-name="{{::field.name}}" id="pca__txt__{{::field.name | validId}}" '
                    + 'class="form-control form-control-focus" name="pca__txt__{{::field.name | validId}}" stop-click-propagation />'
                + '<div style="color: red" ng-show="formatError">Date format: dd-mm-yyyy</div>',
            scope: {
                field: '='
            },
            controller: [
                '$scope',
                ($scope: IDatePickerScope) => {
                    $scope.formatError = false;
                    $scope.fieldValue = '';

                    var isValidDate = (value) => {
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

                    var validate = function () {
                        $scope.formatError = !isValidDate($scope.fieldValue);
                    };

                    $scope.$watch('fieldValue', function (value) {
                        validate();
                        $scope.field.value = (!$scope.formatError && value) ? value : null;
                    });

                    var updateFieldValue = function (field: any) {
                        if (field) {
                            $scope.fieldValue = field.value;
                            validate();
                        }
                        else {
                            $scope.fieldValue = '';
                        }
                    };

                    $scope.$watch('field', function (field: any) {
                        updateFieldValue(field);
                    });

                    updateFieldValue($scope.field);
                }
            ]
        };
    }
}

app.directive('datePicker', dockyard.directives.DatePicker);