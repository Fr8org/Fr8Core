/// <reference path="../../_all.ts" />

module dockyard.directives.dropDownListBox {
    'use strict';

    import pca = dockyard.directives.paneConfigureAction;

    export enum MessageType {
        DropDownListBox_NoRecords
    }

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
        reconfigure: () => void;
        isDisabled: string;
    }

    export function DropDownListBox(): ng.IDirective {
        var controller = ['$scope', 'UpstreamExtractor',
            function (
                $scope: IDropDownListBoxScope,
                UpstreamExtractor: services.UpstreamExtractor
            ) {

                $scope.reconfigure = () => {
                    $scope.$emit(pca.MessageType[pca.MessageType.PaneConfigureAction_Reconfigure], new pca.ActionReconfigureEventArgs($scope.currentAction));
                };

                $scope.setSelectedItem = (item: model.DropDownListItem) => {
                  
                    let field = {
                        value: null,
                        key: null
                    };
                    if (item) {
                        field = {
                            value: item.value,
                            key: item.key
                        };
                    }
                    $scope.field.value = field.value;
                    $scope.field.selectedKey = field.key;
                    $scope.selectedItem = item;

                    // Invoke onChange event handler
                    if ($scope.change != null && angular.isFunction($scope.change)) {
                        $scope.change()($scope.field);
                    }
                    
                };

                // parameter isSilent is for to show the error messages or not
                var loadUpstreamFields = (isSilent: boolean) => {

                    var availabilityType = 'NotSet';
                    if ($scope.field.source) {
                        switch ($scope.field.source.availabilityType)
                        {
                            case model.AvailabilityType.Configuration:
                                availabilityType = 'Configuration';
                                break;
                            case model.AvailabilityType.RunTime:
                                availabilityType = 'RunTime';
                                break;
                            case model.AvailabilityType.Always:
                                availabilityType = 'Always';
                                break;
                            default:
                                break;
                        }
                    }

                    return UpstreamExtractor
                        .getAvailableData($scope.currentAction.id, availabilityType)
                        .then((data: model.IncomingCratesDTO) => {
                            var listItems: Array<model.DropDownListItem> = [];

                            angular.forEach(data.availableCrates, (ct) => {
                                angular.forEach(ct.fields, (f) => {
                                    var i, j;
                                    var found = false;
                                    for (i = 0; i < listItems.length; ++i) {
                                        if (listItems[i].key === f.key) {
                                            found = true;
                                            break;
                                        }
                                    }
                                    if (!found) {
                                        listItems.push(<model.DropDownListItem>f);
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

                            triggerNoRecords(isSilent);

                        });
                };

                $scope.toggle = false;

                $scope.toggleDropDown = $select => {

                        // added by Tony
                    if ($scope.isDisabled) {
                        return false;
                    }

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

                        loadUpstreamFields(false).then(() => { //parameter isSilent false, since we want to see error messages
                            $select.open = !$scope.toggle;   
                            $scope.toggle = !$scope.toggle;
                        });
                    }
                    else {
                        $select.open = !$scope.toggle;
                        $scope.toggle = !$scope.toggle;
                        triggerNoRecords(false);
                    }
                }

                var isFocusOutFunc = focusElem => {
                    focusElem.focusout(() => {
                        $scope.toggle = false;
                    });
                }

                var triggerNoRecords = (isSilent: boolean) => {
                    if ($scope.field.listItems.length === 0 && !isSilent) {
                        $scope.$emit(MessageType[MessageType.DropDownListBox_NoRecords], new AlertEventArgs());
                    } else {
                        findAndSetSelectedItem();
                    }
                }

                var findAndSetSelectedItem = () => {
                    $scope.selectedItem = null;

                    if (!$scope.field.listItems) {
                        return;
                    }

                    for (var i = 0; i < $scope.field.listItems.length; i++) {
                        if ($scope.field.listItems[i].selected ||
                            ($scope.field.value === $scope.field.listItems[i].value
                                && (!$scope.field.hasOwnProperty('selectedKey')
                                    || $scope.field.hasOwnProperty('selectedKey')
                                    && $scope.field.selectedKey === $scope.field.listItems[i].key
                                ))) {
                            $scope.selectedItem = $scope.field.listItems[i];
                            break;
                        }
                    }
                };

                $scope.$watch('field', () => {
                    findAndSetSelectedItem();
                });

                if (!$scope.toggle
                    && $scope.field.source
                    && $scope.field.source.requestUpstream
                    // Only "Field Description" manifestType currently supported for DDLs.
                    && $scope.field.source.manifestType === 'Field Description') {
                    loadUpstreamFields(true);  //parameter isSilent true, to prevent showing error messages everytime ddlb is initialized. 
                }

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
                click: '&',
                isDisabled: '='
            }
        };
    }

    app.directive('dropDownListBox', DropDownListBox);
}