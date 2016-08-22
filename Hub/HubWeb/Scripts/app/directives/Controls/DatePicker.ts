/// <reference path="../../_all.ts" />

module dockyard.directives {
    'use strict';

    export interface IDatePickerScope extends ng.IScope {
        field: any;
        fieldValue: string;
        formatError: boolean;
        opened: boolean;
        dateOptions: any;
        dateFormat: string;

        open: () => void;
    }

    export function DatePicker(): ng.IDirective {
        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/DatePicker',
            scope: {
                field: '='
            },
            controller: [
                '$scope',
                ($scope: IDatePickerScope) => {
                    $scope.formatError = false;
                    $scope.fieldValue = '';
                    $scope.opened = false;
                    $scope.dateFormat = 'dd-MM-yyyy';

                    var extractDateString = function (date) {
                        if (!angular.isDate(date)) {
                            return date;
                        }

                        var padLeft = (str, char, n) => {
                            if (!str || str.length >= n) {
                                return str;
                            }

                            var result = str;
                            for (var i = str.length; i < n; ++i) {
                                result = char + str;
                            }

                            return result;
                        };

                        var days = padLeft(date.getDate().toString(), '0', 2);
                        var months = padLeft((date.getMonth() + 1).toString(), '0', 2);
                        var years = padLeft(date.getFullYear().toString(), '0', 4);

                        return days + '-' + months + '-' + years;
                    };

                    $scope.$watch('fieldValue', (value) => {
                        $scope.field.value = value ? extractDateString(value) : null;
                    });

                    $scope.open = () => {
                        $scope.opened = true;
                    };

                    $scope.dateOptions = {
                        formatYear: 'yy',
                        startingDay: 1
                    };

                    var updateFieldValue = function (field: any) {
                        if (field) {
                            $scope.fieldValue = field.value;
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