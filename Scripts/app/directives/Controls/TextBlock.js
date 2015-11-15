/// <reference path="../../_all.ts" />
var dockyard;
(function (dockyard) {
    var directives;
    (function (directives) {
        var button;
        (function (button) {
            'use strict';
            //More detail on creating directives in TypeScript: 
            //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
            //class TextBlock implements ng.IDirective {
            function TextArea() {
                return {
                    restrict: 'E',
                    replace: true,
                    templateUrl: '/AngularTemplate/TextBlock',
                    scope: {
                        field: '='
                    }
                };
            }
            button.TextArea = TextArea;
            app.directive('textBlock', TextArea);
        })(button = directives.button || (directives.button = {}));
    })(directives = dockyard.directives || (dockyard.directives = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=TextBlock.js.map