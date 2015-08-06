/// <reference path="../../_all.ts" />
var dockyard;
(function (dockyard) {
    var directives;
    (function (directives) {
        var paneDefineCriteria;
        (function (paneDefineCriteria) {
            'use strict';
            function PaneDefineCriteria() {
                var onRender = function (eventArgs, scope) {
                    console.log('PaneDefineCriteria::onRender', eventArgs);
                    scope.fields = eventArgs.fields;
                    scope.criteria = eventArgs.criteria.clone();
                    scope.isVisible = true;
                };
                var onHide = function (scope) {
                    scope.isVisible = false;
                    scope.criteria = null;
                    scope.fields = [];
                };
                var removeCriteria = function (scope) {
                    var eventArgs = new paneDefineCriteria.CriteriaRemovingEventArgs(scope.criteria.id);
                    scope.$emit(paneDefineCriteria.MessageType[paneDefineCriteria.MessageType.PaneDefineCriteria_CriteriaRemoved], eventArgs);
                };
                return {
                    restrict: 'E',
                    templateUrl: '/AngularTemplate/PaneDefineCriteria',
                    scope: {},
                    controller: function ($scope) {
                        $scope.removeCriteria = function () { removeCriteria($scope); };
                        $scope.$on(paneDefineCriteria.MessageType[paneDefineCriteria.MessageType.PaneDefineCriteria_Render], function (event, eventArgs) { return onRender(eventArgs, $scope); });
                        $scope.$on(paneDefineCriteria.MessageType[paneDefineCriteria.MessageType.PaneDefineCriteria_Hide], function (event) { return onHide($scope); });
                    }
                };
            }
            paneDefineCriteria.PaneDefineCriteria = PaneDefineCriteria;
        })(paneDefineCriteria = directives.paneDefineCriteria || (directives.paneDefineCriteria = {}));
    })(directives = dockyard.directives || (dockyard.directives = {}));
})(dockyard || (dockyard = {}));
app.directive('paneDefineCriteria', dockyard.directives.paneDefineCriteria.PaneDefineCriteria);
//# sourceMappingURL=PaneDefineCriteria.js.map