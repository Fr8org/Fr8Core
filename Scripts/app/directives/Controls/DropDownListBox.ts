/// <reference path="../../_all.ts" />
module dockyard.directives.dropDownListBox {
    'use strict';

    export interface IDropDownListBoxScope extends ng.IScope {
        field: model.DropDownListBoxField;
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    class DropDownListBox implements ng.IDirective {
        public link: (scope: IDropDownListBoxScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public templateUrl = '/AngularTemplate/DropDownListBox';
        public controller: ($scope: IDropDownListBoxScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public scope = {
            field: '='
        };
        public restrict = 'E';
        private _$element: ng.IAugmentedJQuery;
        private _$scope: IDropDownListBoxScope;

        constructor() {
            DropDownListBox.prototype.link = (
                scope: IDropDownListBoxScope,
                element: ng.IAugmentedJQuery,
                attrs: ng.IAttributes) => {

                //Link function goes here
            };

            DropDownListBox.prototype.controller = (
                $scope: IDropDownListBoxScope,
                $element: ng.IAugmentedJQuery,
                $attrs: ng.IAttributes) => {
                this._$element = $element;
                this._$scope = $scope;
            };
        }

        //The factory function returns Directive object as per Angular requirements
        public static Factory() {
            var directive = () => {
                return new DropDownListBox();
            };

            directive['$inject'] = [];
            return directive;
        }
    }

    app.directive('dropDownListBox', DropDownListBox.Factory());
}