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
                    this.template = '<div ng-repeat="radio in field.radios"><radio-button-option group-name="{{field.groupName}}" change-selection="changeSelection(radio)" currentAction="currentAction" field="radio"></radio-button-option></div>';
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
                        //<(radio: model.RadioButtonOption) => void>
                        $scope.changeSelection = function (radio) {
                            var radios = $scope.field.radios;
                            for (var i = 0; i < radios.length; i++) {
                                if (radios[i] === radio) {
                                    radios[i].selected = true;
                                }
                                else {
                                    radios[i].selected = false;
                                }
                            }
                            $scope.field.value = radio.value;
                        };
                    };
                }
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
            var RadioButtonOption = (function () {
                function RadioButtonOption($compile) {
                    var _this = this;
                    this.$compile = $compile;
                    this.templateUrl = '/AngularTemplate/RadioButtonOption';
                    this.scope = {
                        currentAction: '=',
                        field: '=',
                        changeSelection: '&',
                        groupName: '@'
                    };
                    this.restrict = 'E';
                    RadioButtonOption.prototype.link = function (scope, element, attrs) {
                        if (angular.isArray(scope.field.controls) || scope.field.controls.length > 0) {
                            $compile('<div ng-repeat="control in field.controls"><configuration-control current-action="currentAction" field="control" /></div>')(scope, function (cloned, scope) {
                                element.find('.nested-controls').append(cloned);
                            });
                        }
                    };
                    RadioButtonOption.prototype.controller = function ($scope, $element, $attrs) {
                        _this._$element = $element;
                        _this._$scope = $scope;
                    };
                }
                //The factory function returns Directive object as per Angular requirements
                RadioButtonOption.Factory = function () {
                    var directive = function ($compile) {
                        return new RadioButtonOption($compile);
                    };
                    directive['$inject'] = ['$compile'];
                    return directive;
                };
                return RadioButtonOption;
            })();
            app.directive('radioButtonOption', RadioButtonOption.Factory());
        })(radioButtonGroup = directives.radioButtonGroup || (directives.radioButtonGroup = {}));
    })(directives = dockyard.directives || (dockyard.directives = {}));
})(dockyard || (dockyard = {}));
