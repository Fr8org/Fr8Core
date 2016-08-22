/// <reference path="../../_all.ts" />

module dockyard.directives {
    'use strict';

    export class FieldListRow {
        public field: string;
        public value: string;
    }

    export function FieldList(): ng.IDirective {
        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/FieldList',
            scope: {
                field: '=',
                change: '&'
            },
            controller: ['$scope', 
                function ($scope: IFieldListScope) {                   
                    $scope.rows = [];

                    // Update UI state, when field is changed.
                    $scope.$watch('field', function (field: any) {
                        if (!field || !field.value) {
                            $scope.rows = [];
                            return;
                        }

                        var jsonRows = angular.fromJson(field.value);
                        angular.forEach(jsonRows, function (row) {
                            var fieldListRow = new FieldListRow();
                            fieldListRow.field = row.Key;
                            fieldListRow.value = row.Value;

                            $scope.rows.push(fieldListRow);
                        });
                    });

                    // Create new row.
                    $scope.addRow = function () {

                        var fieldListRow = new FieldListRow();
                        fieldListRow.field = '';
                        fieldListRow.value = '';

                        $scope.rows.push(fieldListRow);
                        _updateFieldValue();
                    };

                    // Remove existing row.
                    $scope.removeRow = function (index) {
                        $scope.rows.splice(index, 1);
                        _updateFieldValue();
                    };

                    $scope.update = function (event) {
                        if (event && event.relatedTarget && event.relatedTarget) {
                            if ($(event.relatedTarget).hasClass('field-list-add-button')
                                || $(event.relatedTarget).hasClass('field-list-remove-button')) {
                                return;
                            }
                        }

                        _updateFieldValue();
                    };

                    var _updateFieldValue = function () {
                        var designTimeFields = [];

                        angular.forEach($scope.rows, function (row) {
                            designTimeFields.push({
                                Key: row.field,
                                Value: row.value
                            });
                        });

                        $scope.field.value = angular.toJson(designTimeFields);

                        if ($scope.change && angular.isFunction($scope.change)) {
                            $scope.change()($scope.field);
                        }
                    };
                }
            ]
        }
    }

    export interface IFieldListScope extends ng.IScope {
        field: any;
        rows: Array<FieldListRow>;
        update: (event: any) => void;
        addRow: () => void;
        removeRow: (index: number) => void;
        change: () => (field: model.ControlDefinitionDTO) => void;
    }
}

app.directive('fieldList', dockyard.directives.FieldList); 