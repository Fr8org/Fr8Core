/// <reference path="../../_all.ts" />
module dockyard.directives.dropDownListBox {
    'use strict';

    export interface IDropDownListBoxScope extends ng.IScope {
        field: model.DropDownListControlDefinitionDTO;
        change: () => (fieldName: string) => void;
        selectedItem: model.FieldDTO;
        setSelectedItem: (item: model.FieldDTO) => void;
    }

    export function DropDownListBox(): ng.IDirective {
        var controller = ['$scope', function ($scope: IDropDownListBoxScope) {
            $scope.setSelectedItem = function (item: model.FieldDTO) {
                $scope.field.value = item.value || (<any>item).Value;
                $scope.selectedItem = item;

                // Invoke onChange event handler
                if ($scope.change != null && angular.isFunction($scope.change)) {
                    $scope.change()($scope.field.name);
                }
            };

            var findAndSetSelectedItem = function () {
                for (var i = 0; i < $scope.field.listItems.length; i++) {
                    if ($scope.field.value == $scope.field.listItems[i].value || (<any>$scope.field.listItems[i]).Value) {
                        $scope.selectedItem = $scope.field.listItems[i];
                        break;
                    }
                }
            };

            findAndSetSelectedItem();
        }];

        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/DropDownListBox',
            controller: controller,
            scope: {
                field: '=',
                change: '&'
            }
        };
    }

    app.directive('dropDownListBox', DropDownListBox);
}