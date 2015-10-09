/// <reference path="../../_all.ts" />
module dockyard.directives.radioButtonGroup {
    'use strict';

    export interface IRadioButtonGroupScope extends ng.IScope {
        field: model.RadioButtonGroupControlDefinitionDTO;
        changeSelection: (radio: model.RadioButtonOption) => void;
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    class RadioButtonGroup implements ng.IDirective {
        public link: (scope: IRadioButtonGroupScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public template = '<div ng-repeat="radio in field.radios"><radio-button-option group-name="{{field.groupName}}" change-selection="changeSelection(radio)" currentAction="currentAction" field="radio"></radio-button-option></div>';
        public controller: ($scope: IRadioButtonGroupScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public scope = {
            currentAction: '=',
            field: '=',
            changeSelection: '&'
        };
        public restrict = 'E';
        private _$element: ng.IAugmentedJQuery;
        private _$scope: IRadioButtonGroupScope;

        constructor() {
            RadioButtonGroup.prototype.link = (
                scope: IRadioButtonGroupScope,
                element: ng.IAugmentedJQuery,
                attrs: ng.IAttributes) => {

                //Link function goes here
            };

            RadioButtonGroup.prototype.controller = (
                $scope: IRadioButtonGroupScope,
                $element: ng.IAugmentedJQuery,
                $attrs: ng.IAttributes) => {
                this._$element = $element;
                this._$scope = $scope;

                $scope.changeSelection = <(radio: model.RadioButtonOption) => void>angular.bind(this, this.changeSelection);
            };
        }

        private changeSelection(radio: model.RadioButtonOption) {
            var radios = this._$scope.field.radios
            for (var i = 0; i < radios.length; i++) {
                if (radios[i] === radio) {
                    radios[i].selected = true;
                } else {
                    radios[i].selected = false;
                }
            }
            this._$scope.field.value = radio.value;
        }

        //The factory function returns Directive object as per Angular requirements
        public static Factory() {
            var directive = () => {
                return new RadioButtonGroup();
            };

            directive['$inject'] = [];
            return directive;
        }
    }
    app.directive('radioButtonGroup', RadioButtonGroup.Factory());


    export interface IRadioButtonOptionScope extends ng.IScope {
        field: model.RadioButtonOption;
        groupName: string,
        changeSelection: (radio: model.RadioButtonOption) => void;
    }

    class RadioButtonOption implements ng.IDirective {
        public link: (scope: IRadioButtonOptionScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public templateUrl = '/AngularTemplate/RadioButtonOption';
        public controller: ($scope: IRadioButtonOptionScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public scope = {
            currentAction: '=',
            field: '=',
            changeSelection: '&',
            groupName: '@'
        };
        public restrict = 'E';
        private _$element: ng.IAugmentedJQuery;
        private _$scope: IRadioButtonOptionScope;

        constructor(
            private $compile: ng.ICompileService) {

            RadioButtonOption.prototype.link = (
                scope: IRadioButtonOptionScope,
                element: ng.IAugmentedJQuery,
                attrs: ng.IAttributes) => {

                if (angular.isArray(scope.field.controls) || scope.field.controls.length > 0) {
                    $compile('<div ng-repeat="control in field.controls"><configuration-field current-action="currentAction" field="control" /></div>')(scope, function (cloned, scope) {
                        element.find('.nested-controls').append(cloned);
                    });
                }
            };

            RadioButtonOption.prototype.controller = (
                $scope: IRadioButtonOptionScope,
                $element: ng.IAugmentedJQuery,
                $attrs: ng.IAttributes) => {
                this._$element = $element;
                this._$scope = $scope;

            };
        }

        //The factory function returns Directive object as per Angular requirements
        public static Factory() {
            var directive = ($compile: ng.ICompileService) => {
                return new RadioButtonOption($compile);
            };

            directive['$inject'] = ['$compile'];
            return directive;
        }
    }
    app.directive('radioButtonOption', RadioButtonOption.Factory());
}

