/// <reference path="../../_all.ts" />

module dockyard.directives.dropDownListBox {
    'use strict';

    export interface IDropDownListBoxScope extends ng.IScope {
        field: model.DropDownList;
        change: () => (field: model.ControlDefinitionDTO) => void;
        selectedItem: model.FieldDTO;
        setSelectedItem: (item: model.FieldDTO) => void;
    }

    export function DropDownListBox(): ng.IDirective {
        var controller = ['$scope', function ($scope: IDropDownListBoxScope) {
            $scope.setSelectedItem = (item: model.FieldDTO) => {
                $scope.field.value = item.value || (<any>item).Value;
                $scope.field.selectedKey = item.key;
                $scope.selectedItem = item;

                // Invoke onChange event handler
                if ($scope.change != null && angular.isFunction($scope.change)) {
                    $scope.change()($scope.field);
                }
            };

            var findAndSetSelectedItem = function () {
                debugger;
                for (var i = 0; i < $scope.field.listItems.length; i++) {
                    if ($scope.field.listItems[i].selected ||
                        ($scope.field.value == $scope.field.listItems[i].value 
                        && (!$scope.field.hasOwnProperty('selectedKey')
                            || $scope.field.hasOwnProperty('selectedKey')
                                && $scope.field.selectedKey == $scope.field.listItems[i].key
                            ))) {
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