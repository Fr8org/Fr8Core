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
        var controller = ['$scope', 'UpstreamExtractor','UIHelperService',
            function (
                $scope: IDropDownListBoxScope,
                UpstreamExtractor: services.UpstreamExtractor,
                uiHelperService: services.IUIHelperService
            ) {

                var alertMessage = new model.AlertDTO();
                alertMessage.title = "Notification";
                alertMessage.body = 'There are no upstream fields available right now. To learn more,<a href= "/documentation/UpstreamCrates.html" target= "_blank" > click here </a><i class="fa fa-question-circle" > </i>';

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
                            .extractUpstreamData($scope.currentAction.id, 'Field Description', 'NotSet')
                            .then((data: any) => {
                                var listItems: Array<model.DropDownListItem> = []; 

                                angular.forEach(<Array<any>>data, cm => {
                                    var fields = <Array<model.FieldDTO>>cm.fields;

                                    angular.forEach(fields, (it) => {
                                        var i, j;
                                        var found = false;
                                        for (i = 0; i < listItems.length; ++i) {
                                            if (listItems[i].key === it.key) {
                                                found = true;
                                                break;
                                            }
                                        }

                                        if (!found) {
                                            listItems.push(<model.DropDownListItem>it);
                                        }
                                    });
                                });

                                listItems.sort((x, y) => {
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
                                
                                $scope.field.listItems = listItems;

                                $select.open = !$scope.toggle;
                                $scope.toggle = !$scope.toggle;

                                if ($scope.field.type == "TextSource" && $scope.field.listItems.length === 0) {
                                    uiHelperService.openConfirmationModal(alertMessage);
                                }

                            });
                    }
                    else {
                        $select.open = !$scope.toggle;
                        $scope.toggle = !$scope.toggle;

                        if ($scope.field.type == "TextSource" && $scope.field.listItems.length === 0) {
                            uiHelperService.openConfirmationModal(alertMessage);
                        }
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