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
                    this.template = '<div ng-repeat="radio in field.radios"><radio-button-option-field group-name="{{field.groupName}}" change-selection="changeSelection(radio)" currentAction="currentAction" field="radio"></radio-button-option-field></div>';
                    this.scope = {
                        currentAction: '=',
                        field: '=',
                        changeSelection: '&'
                    };
                    this.restrict = 'E';
                    RadioButtonGroup.prototype.link = function (scope, element, attrs) {
                        //Link function goes here
                    };
                    RadioButtonGroup.prototype.controller = function ($scope, $element, $attrs) {
                        _this._$element = $element;
                        _this._$scope = $scope;
                        $scope.changeSelection = angular.bind(_this, _this.changeSelection);
                    };
                }
                RadioButtonGroup.prototype.changeSelection = function (radio) {
                    var radios = this._$scope.field.radios;
                    for (var i = 0; i < radios.length; i++) {
                        if (radios[i] === radio) {
                            radios[i].selected = true;
                        }
                        else {
                            radios[i].selected = false;
                        }
                    }
                    this._$scope.field.value = radio.value;
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
            var RadioButtonOptionField = (function () {
                function RadioButtonOptionField($compile) {
                    var _this = this;
                    this.$compile = $compile;
                    this.templateUrl = '/AngularTemplate/RadioButtonOptionField';
                    this.scope = {
                        currentAction: '=',
                        field: '=',
                        changeSelection: '&',
                        groupName: '@'
                    };
                    this.restrict = 'E';
                    RadioButtonOptionField.prototype.link = function (scope, element, attrs) {
                        if (angular.isArray(scope.field.fields) || scope.field.fields.length > 0) {
                            element.find('.nested-fields').append('<div ng-repeat="field in field.fields"><configuration-field current-action="currentAction" field="field" /></div>');
                        }
                        $compile('<div ng-repeat="field in field.fields"><configuration-field current-action="currentAction" field="field" /></div>')(scope, function (cloned, scope) {
                            element.find('.nested-fields').append(cloned);
                        });
                    };
                    RadioButtonOptionField.prototype.controller = function ($scope, $element, $attrs) {
                        _this._$element = $element;
                        _this._$scope = $scope;
                    };
                }
                //The factory function returns Directive object as per Angular requirements
                RadioButtonOptionField.Factory = function () {
                    var directive = function ($compile) {
                        return new RadioButtonOptionField($compile);
                    };
                    directive['$inject'] = ['$compile'];
                    return directive;
                };
                return RadioButtonOptionField;
            })();
            app.directive('radioButtonOptionField', RadioButtonOptionField.Factory());
        })(radioButtonGroup = directives.radioButtonGroup || (directives.radioButtonGroup = {}));
    })(directives = dockyard.directives || (dockyard.directives = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=RadioButtonGroup.js.map