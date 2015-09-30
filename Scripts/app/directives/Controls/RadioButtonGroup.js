/// <reference path="../../_all.ts" />
var dockyard;
(function (dockyard) {
    var directives;
    (function (directives) {
        var radioButtonGroup;
        (function (radioButtonGroup) {
            'use strict';
            //More detail on creating directives in TypeScript: 
            //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
            var RadioButtonGroup = (function () {
                function RadioButtonGroup() {
                    var _this = this;
                    this.templateUrl = '/AngularTemplate/RadioButtonGroup';
                    this.scope = {
                        field: '='
                    };
                    this.restrict = 'E';
                    RadioButtonGroup.prototype.link = function (scope, element, attrs) {
                        //Link function goes here
                    };
                    RadioButtonGroup.prototype.controller = function ($scope, $element, $attrs) {
                        _this._$element = $element;
                        _this._$scope = $scope;
                        $scope.ChangeSelection = angular.bind(_this, _this.ChangeSelection);
                    };
                }
                RadioButtonGroup.prototype.ChangeSelection = function (radio) {
                    for (var i = 0; i < this._$scope.field.radios.length; i++) {
                        if (this._$scope.field.radios[i] === radio) {
                            this._$scope.field.radios[i].selected = true;
                        }
                        else {
                            this._$scope.field.radios[i].selected = false;
                        }
                    }
                };
                //The factory function returns Directive object as per Angular requirements
                RadioButtonGroup.Factory = function () {
                    var directive = function () {
                        return new RadioButtonGroup();
                    };
                    directive['$inject'] = [];
                    return directive;
                };
                return RadioButtonGroup;
            })();
            app.directive('radioButtonGroup', RadioButtonGroup.Factory());
        })(radioButtonGroup = directives.radioButtonGroup || (directives.radioButtonGroup = {}));
    })(directives = dockyard.directives || (dockyard.directives = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=RadioButtonGroup.js.map