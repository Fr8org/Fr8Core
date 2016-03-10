/// <reference path="../../_all.ts" />

module dockyard.directives.dropDownListBox {
    'use strict';

    export interface IDropDownListBoxScope extends ng.IScope {
        field: model.DropDownList;
        currentAction: model.ActivityDTO;
        change: () => (field: model.ControlDefinitionDTO) => void;
        click: () => (field: model.ControlDefinitionDTO) => void;
        selectedItem: model.FieldDTO;
        setSelectedItem: (item: model.FieldDTO) => void;
        toggle: boolean;
        toggleDropDown: (select) => void;
        focusOutSet: (focusElem: any) => void;
    }

    export function DropDownListBox(): ng.IDirective {
        var controller = ['$scope', 'UpstreamExtractor',
            function (
                $scope: IDropDownListBoxScope,
                UpstreamExtractor: services.UpstreamExtractor
            ) {

                $scope.setSelectedItem = (item: model.FieldDTO) => {
                    $scope.field.value = item.value || (<any>item).Value;
                    $scope.field.selectedKey = item.key;
                    $scope.selectedItem = item;

                    // Invoke onChange event handler
                    if ($scope.change != null && angular.isFunction($scope.change)) {
                        $scope.change()($scope.field);
                    }
                };

                $scope.toggle = false;

                $scope.toggleDropDown = $select => {
                    if (!$scope.focusOutSet) {
                        var focusElem = angular.element($select.focusInput);
                        $scope.focusOutSet = isFocusOutFunc;
                        $scope.focusOutSet(focusElem);
                    }

                    if (!$scope.toggle
                        && $scope.field.source
                        && $scope.field.source.requestUpstream
                        // Only "Field Description" manifestType currently supported for DDLs.
                        && $scope.field.source.manifestType === 'Field Description') {

                        UpstreamExtractor
                            .extractUpstreamData($scope.currentAction.id)
                            .then((cm: any) => {
                                var fields = <Array<model.FieldDTO>>cm.fields;

                                $scope.field.listItems = [];
                                angular.forEach(fields, (it) => {
                                    $scope.field.listItems.push(<model.DropDownListItem>it);
                                });
                            });
                    }
                    else {
                        $select.open = !$scope.toggle;
                        $scope.toggle = !$scope.toggle;
                    }
                }

                var isFocusOutFunc = focusElem => {
                    focusElem.focusout(() => {
                        $scope.toggle = false;
                    });
                }

                var findAndSetSelectedItem = () => {
                    if (!$scope.field.listItems) {
                        return;
                    }

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
            }
        ];

        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/DropDownListBox',
            controller: controller,
            scope: {
                currentAction: '=',
                field: '=',
                change: '&',
                click: '&'
            }
        };
    }

    app.directive('dropDownListBox', DropDownListBox);
}