/// <reference path="../../_all.ts" />
var dockyard;
(function (dockyard) {
    var directives;
    (function (directives) {
        var dropDownListBox;
        (function (dropDownListBox) {
            'use strict';
            //More detail on creating directives in TypeScript: 
            //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
            var DropDownListBox = (function () {
                function DropDownListBox() {
                    this.templateUrl = '/AngularTemplate/DropDownListBox';
                    this.scope = {
                        field: '=',
                        change: '&'
                    };
                    this.restrict = 'E';
                    DropDownListBox.prototype.link = function (scope, element, attrs) {
                        //Link function goes here
                    };
                    DropDownListBox.prototype.controller = function ($scope, $element, $attrs) {
                        $scope.selectedItem = null;
                        $scope.SetSelectedItem = function (item) {
                            $scope.field.value = item.Value;
                            $scope.selectedItem = item;
                            // Invoike onChange event handler
                            if ($scope.change != null && angular.isFunction($scope.change)) {
                                $scope.change()($scope.field.name);
                            }
                        };
                        var FindAndSetSelectedItem = function () {
                            for (var i = 0; i < $scope.field.listItems.length; i++) {
                                if ($scope.field.value == $scope.field.listItems[i].Value) {
                                    $scope.selectedItem = $scope.field.listItems[i];
                                    break;
                                }
                            }
                        };
                        FindAndSetSelectedItem();
                        $scope.defaultitem = null;
                    };
                }
                //The factory function returns Directive object as per Angular requirements
                DropDownListBox.Factory = function () {
                    var directive = function () {
                        return new DropDownListBox();
                    };
                    directive['$inject'] = [];
                    return directive;
                };
                return DropDownListBox;
            })();
            app.directive('dropDownListBox', DropDownListBox.Factory());
        })(dropDownListBox = directives.dropDownListBox || (directives.dropDownListBox = {}));
    })(directives = dockyard.directives || (dockyard.directives = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=DropDownListBox.js.map