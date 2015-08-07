/// <reference path="../../_all.ts" />
/// <reference path="../../../typings/angularjs/angular.d.ts"/>
var dockyard;
(function (dockyard) {
    var directives;
    (function (directives) {
        var PaneFieldMapping;
        (function (PaneFieldMapping_1) {
            'use strict';
            var PaneFieldMapping = (function () {
                function PaneFieldMapping() {
                    this.templateUrl = '/AngularTemplate/PaneFieldMapping';
                    this.restrict = 'E';
                }
                PaneFieldMapping.factory = function () {
                    var directive = function () {
                        return new PaneFieldMapping();
                    };
                    directive['$inject'] = ['$rootScope'];
                    return directive;
                };
                return PaneFieldMapping;
            })();
            app.directive('paneFieldMapping', PaneFieldMapping.factory());
        })(PaneFieldMapping = directives.PaneFieldMapping || (directives.PaneFieldMapping = {}));
    })(directives = dockyard.directives || (dockyard.directives = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=PaneFieldMapping.js.map