/// <reference path="../_all.ts" />
var dockyard;
(function (dockyard) {
    var directives;
    (function (directives) {
        'use strict';
        function PaneDefineCriteria() {
            return {
                restrict: 'E',
                templateUrl: '/AngularTemplate/PaneDefineCriteria',
                scope: {
                    criteria: '=',
                    fields: '=',
                    removeCriteria: '&onRemoveCriteria'
                }
            };
        }
        directives.PaneDefineCriteria = PaneDefineCriteria;
    })(directives = dockyard.directives || (dockyard.directives = {}));
})(dockyard || (dockyard = {}));
app.directive('paneDefineCriteria', dockyard.directives.PaneDefineCriteria);
//# sourceMappingURL=PaneDefineCriteria.js.map