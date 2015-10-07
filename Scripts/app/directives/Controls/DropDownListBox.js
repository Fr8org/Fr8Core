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
                    var _this = this;
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
                        _this._$element = $element;
                        _this._$scope = $scope;
                        _this._$scope.selectedItem = null;
                        $scope.SetSelectedItem = angular.bind(_this, _this.SetSelectedItem);
                        _this.FindAndSetSelectedItem();
                        $scope.defaultitem = null;
                    };
                }
                DropDownListBox.prototype.SetSelectedItem = function (item) {
                    this._$scope.field.value = item.Value;
                    this._$scope.selectedItem = item;
                    // Invoike onChange event handler
                    if (this._$scope.change != null && angular.isFunction(this._$scope.change)) {
                        this._$scope.change()(this._$scope.field.name);
                    }
                };
                DropDownListBox.prototype.FindAndSetSelectedItem = function () {
                    for (var i = 0; i < this._$scope.field.listItems.length; i++) {
                        if (this._$scope.field.value == this._$scope.field.listItems[i].Value) {
                            this._$scope.selectedItem = this._$scope.field.listItems[i];
                            break;
                        }
                    }
                };
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