/// <reference path="../../_all.ts" />
module dockyard.directives.radioButtonGroup {
    'use strict';

    export interface IRadioButtonGroupScope extends ng.IScope {
        field: model.RadioButtonGroup;
        changeSelection: (radio: model.RadioButtonOption) => void;
        uniqueDirectiveId: number;
        change: () => (field: model.ControlDefinitionDTO) => void;

        //for DropdownListBox issue FR-4160
        isDisabled: boolean;
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    export function RadioButtonGroup(): ng.IDirective {
        var uniqueDirectiveId = 1;
        var template = '<div ng-repeat="radio in field.radios" ng-id="!radio.isHidden"><div class="radio-button-group-content input-group"> <radio-button-option group-name="{{field.groupName+\'_rgb_\'+uniqueDirectiveId}}" change-selection="changeSelection(radio)" current-action="currentAction" field="radio" is-disabled="isDisabled"></radio-button-option></div></div>';
        var controller = ['$scope','$element','$attrs', ($scope: IRadioButtonGroupScope, $element: ng.IAugmentedJQuery, $attrs: ng.IAttributes) => {
            $scope.uniqueDirectiveId = ++uniqueDirectiveId;
            $scope.changeSelection = (radio: model.RadioButtonOption) => {
                var radios = $scope.field.radios
                for (var i = 0; i < radios.length; i++) {
                    if (radios[i] === radio) {
                        radios[i].selected = true;
                    } else {
                        radios[i].selected = false;
                    }
                }
                $scope.field.value = radio.value;
                // Invoke onChange event handler
                if ($scope.change != null && angular.isFunction($scope.change())) {
                    $scope.change()($scope.field);
                }
            }
        }];

        return {
            restrict: 'E',
            template: template,
            controller: controller,
            scope: {
                currentAction: '=',
                field: '=',
                changeSelection: '&',
                change: '&',
                isDisabled: '='
            }
        };
        
    }
    app.directive('radioButtonGroup', RadioButtonGroup);


    export interface IRadioButtonOptionScope extends ng.IScope {
        field: model.RadioButtonOption;
        groupName: string;
        changeSelection: (radio: model.RadioButtonOption) => void;

        // for FR-4160
        isDisabled:boolean;
    }

    export function RadioButtonOption($compile: ng.ICompileService): ng.IDirective {
        var link = (scope: IRadioButtonOptionScope, element: ng.IAugmentedJQuery,attrs: ng.IAttributes) => {
            if (angular.isArray(scope.field.controls) || scope.field.controls.length > 0) {
                $compile('<div ng-repeat="control in field.controls"><configuration-control current-action="currentAction" field="control" is-disabled="isDisabled" /></div>')(scope, function (cloned, scope) {
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
                groupName: '@',
                isDisabled: '='
            }
        };
    }
    app.directive('radioButtonOption', ['$compile', RadioButtonOption]);
}

