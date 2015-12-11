/// <reference path="../../_all.ts" />
var dockyard;
(function (dockyard) {
    var directives;
    (function (directives) {
        var dropDownListBox;
        (function (dropDownListBox) {
            'use strict';
            function DropDownListBox() {
                var controller = ['$scope', function ($scope) {
                        $scope.setSelectedItem = function (item) {
                            $scope.field.value = item.value || item.Value;
                            $scope.field.selectedKey = item.key;
                            $scope.selectedItem = item;
                            // Invoke onChange event handler
                            if ($scope.change != null && angular.isFunction($scope.change)) {
                                $scope.change()($scope.field);
                            }
                        };
                        var findAndSetSelectedItem = function () {
                            for (var i = 0; i < $scope.field.listItems.length; i++) {
                                if ($scope.field.value == $scope.field.listItems[i].value
                                    && (!$scope.field.hasOwnProperty('selectedKey')
                                        || $scope.field.hasOwnProperty('selectedKey')
                                            && $scope.field.selectedKey == $scope.field.listItems[i].key)) {
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
            dropDownListBox.DropDownListBox = DropDownListBox;
            app.directive('dropDownListBox', DropDownListBox);
        })(dropDownListBox = directives.dropDownListBox || (directives.dropDownListBox = {}));
    })(directives = dockyard.directives || (dockyard.directives = {}));
})(dockyard || (dockyard = {}));
