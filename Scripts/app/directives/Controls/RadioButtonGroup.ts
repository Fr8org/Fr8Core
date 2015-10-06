/// <reference path="../../_all.ts" />
module dockyard.directives.radioButtonGroup {
    'use strict';

    export interface IRadioButtonGroupScope extends ng.IScope {
        field: model.RadioButtonGroupField;
        ChangeSelection: (radio: model.RadioField) => void;
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    class RadioButtonGroup implements ng.IDirective {
        public link: (scope: IRadioButtonGroupScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public templateUrl = '/AngularTemplate/RadioButtonGroup';
        public controller: ($scope: IRadioButtonGroupScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public scope = {
            currentAction: '=',
            field: '='
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

                $scope.ChangeSelection = <(radio: model.RadioField) => void> angular.bind(this, this.ChangeSelection);
            };
        }

        private ChangeSelection(radio: model.RadioField) {
            var radios = this._$scope.field.radios
            for (var i = 0; i < radios.length; i++) {
                if (radios[i] === radio) {
                    radios[i].selected = true;
                } else {
                    radios[i].selected = false;
                }
            }
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
}

