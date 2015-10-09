/// <reference path="../../_all.ts" />
var dockyard;
(function (dockyard) {
    var directives;
    (function (directives) {
        var textBlock;
        (function (textBlock) {
            'use strict';
            var TextBlock = (function () {
                function TextBlock() {
                    var _this = this;
                    this.templateUrl = '/AngularTemplate/TextBlock';
                    this.scope = {
                        field: '='
                    };
                    this.restrict = 'E';
                    this.replace = true;
                    TextBlock.prototype.link = function (scope, element, attrs) {
                    };
                    TextBlock.prototype.controller = function ($scope, $element, $attrs) {
                        _this._$element = $element;
                        _this._$scope = $scope;
                    };
                }
                TextBlock.Factory = function () {
                    var directive = function () {
                        return new TextBlock();
                    };
                    directive['$inject'] = [];
                    return directive;
                };
                return TextBlock;
            })();
            app.directive('textBlock', TextBlock.Factory());
        })(textBlock = directives.textBlock || (directives.textBlock = {}));
    })(directives = dockyard.directives || (dockyard.directives = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=TextBlock.js.map