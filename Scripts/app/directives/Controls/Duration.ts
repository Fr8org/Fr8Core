/// <reference path="../../_all.ts" />
module dockyard.directives.duration {
    'use strict';

    export interface IDurationScope extends ng.IScope {
        field: model.DurationControlDefinitionDTO;
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    class Duration implements ng.IDirective {
        public link: (scope: IDurationScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public controller: ($scope: IDurationScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;

        public templateUrl = '/AngularTemplate/Duration';
        public restrict = 'E';
        public scope = {

        }

        constructor() {
            Duration.prototype.link = (
                scope: IDurationScope,
                element: ng.IAugmentedJQuery,
                attrs: ng.IAttributes) => {

            }

            Duration.prototype.controller = (
                $scope: IDurationScope,
                $element: ng.IAugmentedJQuery,
                $attrs: ng.IAttributes) => {

            }

        };

        //The factory function returns Directive object as per Angular requirements
        public static Factory() {
            var directive = () => {
                return new Duration();
            };

            directive['$inject'] = [];
            return directive;
        }
    }

    app.directive('duration', Duration.Factory());
}