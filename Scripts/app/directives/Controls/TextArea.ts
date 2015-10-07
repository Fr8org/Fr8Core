/// <reference path="../../_all.ts" />
module dockyard.directives.textBlock {
    'use strict';

    export interface ITextAreaScope extends ng.IScope {
        field: model.TextAreaField;
        buttonSet: Array<Array<String>>;
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    class TextArea implements ng.IDirective {
        public link: (scope: ITextAreaScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public templateUrl = '/AngularTemplate/TextArea';
        public controller: ($scope: ITextAreaScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public scope = {
            field: '='
        };

        public restrict = 'E';
        public replace = true;
        private _$element: ng.IAugmentedJQuery;
        private _$scope: ITextAreaScope;
        private _availableButtons = ['h1', 'h2', 'h3', 'h4', 'h5', 'h6', 'p', 'bold', 'italics', 'underline', 'ul', 'undo', 'redo', 'html', 'insertImage', 'insertLink'];
        private _disabledButtons = [];
        private _buttonSet = [this._availableButtons, this._disabledButtons];

        constructor() {
            TextArea.prototype.link = (
                scope: ITextAreaScope,
                element: ng.IAugmentedJQuery,
                attrs: ng.IAttributes) => {

                //Link function goes here
            };

            TextArea.prototype.controller = (
                $scope: ITextAreaScope,
                $element: ng.IAugmentedJQuery,
                $attrs: ng.IAttributes) => {
                this._$element = $element;
                this._$scope = $scope;

                this._$scope.buttonSet = this._buttonSet;

            };
        }

        //The factory function returns Directive object as per Angular requirements
        public static Factory() {
            var directive = () => {
                return new TextArea();
            };

            directive['$inject'] = [];
            return directive;
        }
    }

    app.directive('textArea', TextArea.Factory());
}