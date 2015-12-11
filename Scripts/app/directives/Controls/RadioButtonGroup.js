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
            function RadioButtonGroup() {
                var uniqueDirectiveId = 1;
                var template = '<div ng-repeat="radio in field.radios"><div class="radio-button-group-content"> <radio-button-option group-name="{{field.groupName+\'_rgb_\'+uniqueDirectiveId}}" change-selection="changeSelection(radio)" currentAction="currentAction" field="radio"></radio-button-option></div></div>';
                var controller = function ($scope, $element, $attrs) {
                    $scope.uniqueDirectiveId = ++uniqueDirectiveId;
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
                        // Invoke onChange event handler
                        if ($scope.change != null && angular.isFunction($scope.change())) {
                            $scope.change()($scope.field);
                        }
                    };
                };
                return {
                    restrict: 'E',
                    template: template,
                    controller: controller,
                    scope: {
                        currentAction: '=',
                        field: '=',
                        changeSelection: '&',
                        change: '&'
                    }
                };
            }
            radioButtonGroup.RadioButtonGroup = RadioButtonGroup;
            app.directive('radioButtonGroup', RadioButtonGroup);
            function RadioButtonOption($compile) {
                var link = function (scope, element, attrs) {
                    if (angular.isArray(scope.field.controls) || scope.field.controls.length > 0) {
                        $compile('<div ng-repeat="control in field.controls"><configuration-control current-action="currentAction" field="control" /></div>')(scope, function (cloned, scope) {
                            element.find('.nested-controls').append(cloned);
                        });
                    }
                };
                return {
                    restrict: 'E',
                    templateUrl: '/AngularTemplate/RadioButtonOption',
                    link: link,
                    scope: {
                        currentAction: '=',
                        field: '=',
                        changeSelection: '&',
                        groupName: '@'
                    }
                };
            }
            radioButtonGroup.RadioButtonOption = RadioButtonOption;
            app.directive('radioButtonOption', ['$compile', RadioButtonOption]);
        })(radioButtonGroup = directives.radioButtonGroup || (directives.radioButtonGroup = {}));
    })(directives = dockyard.directives || (dockyard.directives = {}));
})(dockyard || (dockyard = {}));
