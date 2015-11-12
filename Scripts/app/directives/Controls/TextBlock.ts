/// <reference path="../../_all.ts" />
module dockyard.directives.button {
    'use strict';

    export interface ITextBlockScope extends ng.IScope {
        field: model.TextBlock;
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    class TextBlock implements ng.IDirective {
        public link: (scope: ITextBlockScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public templateUrl = '/AngularTemplate/TextBlock';
        public controller: ($scope: ITextBlockScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public scope = {
            field: '='
        };

        public restrict = 'E';
        public replace = true;
        private _$element: ng.IAugmentedJQuery;
        private _$scope: ITextBlockScope;

        constructor() {
            TextBlock.prototype.link = (
                scope: ITextBlockScope,
                element: ng.IAugmentedJQuery,
                attrs: ng.IAttributes) => {

                //Link function goes here
            };

            TextBlock.prototype.controller = (
                $scope: ITextBlockScope,
                $element: ng.IAugmentedJQuery,
                $attrs: ng.IAttributes) => {
                this._$element = $element;
                this._$scope = $scope;
            };
        }

        //The factory function returns Directive object as per Angular requirements
        public static Factory() {
            var directive = () => {
                return new TextBlock();
            };

            directive['$inject'] = [];
            return directive;
        }
    }

    app.directive('textBlock', TextBlock.Factory());
}