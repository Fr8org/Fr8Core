/// <reference path="../../_all.ts" />
module dockyard.directives.inputFocus {
    'use strict';

    export interface IInputFocusAttributes extends ng.IAttributes {
        inputFocus: string;
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    class InputFocus implements ng.IDirective {
        public link: (scope: ng.IScope, element: ng.IAugmentedJQuery, attrs: IInputFocusAttributes) => void;
        public restrict = 'A';

        private prevState: boolean = false;

        constructor($parse: ng.IParseService) {
            InputFocus.prototype.link = (
                scope: ng.IScope,
                element: ng.IAugmentedJQuery,
                attrs: IInputFocusAttributes) => {
                var model = $parse(attrs.inputFocus);
                scope.$watch(model, (value) => {
                    if (value && !this.prevState) {
                        setTimeout(() => { element.focus(); }, 0);
                    }

                    this.prevState = !!value;
                });
            };
        }

        //The factory function returns Directive object as per Angular requirements
        public static Factory() {
            var directive = ($parse: ng.IParseService) => {
                return new InputFocus($parse);
            };

            directive['$inject'] = ['$parse'];
            return directive;
        }
    }

    app.directive('inputFocus', InputFocus.Factory());
}