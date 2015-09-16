/// <reference path="../../_all.ts" />
module dockyard.directives.dropDownListBox {
    'use strict';

    export interface IDropDownListBoxScope extends ng.IScope {
        field: model.DropDownListBoxField;
        selectedItem: model.DropDownListItem;
        SetSelectedItem: (item: model.DropDownListItem) => void;
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
                this._$scope.selectedItem = null;

                $scope.SetSelectedItem = <(radio: model.DropDownListItem) => void> angular.bind(this, this.SetSelectedItem);
            };
        }

        private SetSelectedItem(item: model.DropDownListItem) {
            this._$scope.field.value = item.value;
            this._$scope.selectedItem = item;
        }

        private FindAndSetSelectedItem() {
            for (var i = 0; i < this._$scope.field.listItems.length; i++) {
                if (this._$scope.field.value == this._$scope.field.listItems[i].value) {
                    this._$scope.selectedItem = this._$scope.field.listItems[i];
                }
            }
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