/// <reference path="../../_all.ts" />
module dockyard.directives.radioButtonGroup {
    'use strict';

    export interface IRadioButtonGroupScope extends ng.IScope {
        field: model.RadioButtonGroupField;
        changeSelection: (radio: model.RadioButtonOptionField) => void;
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    class RadioButtonGroup implements ng.IDirective {
        public link: (scope: IRadioButtonGroupScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public template = '<div ng-repeat="radio in field.radios"><radio-button-option-field group-name="{{field.groupName}}" change-selection="changeSelection(radio)" currentAction="currentAction" field="radio"></radio-button-option-field></div>';
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

                $scope.changeSelection = <(radio: model.RadioButtonOptionField) => void>angular.bind(this, this.changeSelection);
            };
        }

        private changeSelection(radio: model.RadioButtonOptionField) {
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


    export interface IRadioButtonOptionFieldScope extends ng.IScope {
        field: model.RadioButtonOptionField;
        groupName: string,
        changeSelection: (radio: model.RadioButtonOptionField) => void;
    }

    class RadioButtonOptionField implements ng.IDirective {
        public link: (scope: IRadioButtonOptionFieldScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public templateUrl = '/AngularTemplate/RadioButtonOptionField';
        public controller: ($scope: IRadioButtonOptionFieldScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public scope = {
            currentAction: '=',
            field: '=',
            changeSelection: '&',
            groupName: '@'
        };
        public restrict = 'E';
        private _$element: ng.IAugmentedJQuery;
        private _$scope: IRadioButtonOptionFieldScope;

        constructor(
            private $compile: ng.ICompileService) {

            RadioButtonOptionField.prototype.link = (
                scope: IRadioButtonOptionFieldScope,
                element: ng.IAugmentedJQuery,
                attrs: ng.IAttributes) => {

                if (angular.isArray(scope.field.fields) || scope.field.fields.length > 0) {
                    element.find('.nested-fields').append('<div ng-repeat="field in field.fields"><configuration-field current-action="currentAction" field="field" /></div>');
                }

                $compile('<div ng-repeat="field in field.fields"><configuration-field current-action="currentAction" field="field" /></div>')(scope, function (cloned, scope) {
                    element.find('.nested-fields').append(cloned);
                });
            };

            RadioButtonOptionField.prototype.controller = (
                $scope: IRadioButtonOptionFieldScope,
                $element: ng.IAugmentedJQuery,
                $attrs: ng.IAttributes) => {
                this._$element = $element;
                this._$scope = $scope;

            };
        }

        //The factory function returns Directive object as per Angular requirements
        public static Factory() {
            var directive = ($compile: ng.ICompileService) => {
                return new RadioButtonOptionField($compile);
            };

            directive['$inject'] = ['$compile'];
            return directive;
        }
    }
    app.directive('radioButtonOptionField', RadioButtonOptionField.Factory());
}

