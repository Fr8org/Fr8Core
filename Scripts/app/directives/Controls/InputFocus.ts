/// <reference path="../../_all.ts" />
module dockyard.directives.inputFocus {
    'use strict';

    export interface IInputFocusAttributes extends ng.IAttributes {
        inputFocus: string;
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    export function InputFocus($parse: ng.IParseService): ng.IDirective {

        var link = (scope: ng.IScope, element: ng.IAugmentedJQuery, attrs: IInputFocusAttributes) => {

            var prevState = false;
            var model = $parse(attrs.inputFocus);
            scope.$watch(model, (value) => {
                if (value && !prevState) {
                    setTimeout(() => { element.focus(); }, 0);
                }

                prevState = !!value;
            });
        };
        
        return {
            restrict: 'A',
            link: link
        };
    }

    app.directive('inputFocus', ['$parse', InputFocus]);
}