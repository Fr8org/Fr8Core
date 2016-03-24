/// <reference path="../../_all.ts" />
module dockyard.directives.button {
    'use strict';

    export interface ITextBlockScope extends ng.IScope {
        field: model.TextBlock;
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    export function TextBlock(): ng.IDirective {
        return {
            restrict: 'E',
            replace: true,
            templateUrl: '/AngularTemplate/TextBlock',
            scope: {
                field: '='
            }
        };
    }

    app.directive('textBlock', TextBlock);
}