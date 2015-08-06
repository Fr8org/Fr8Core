/// <reference path="../../_all.ts" />
 
module dockyard.directives.paneDefineCriteria {
    'use strict';

    export function PaneDefineCriteria(): ng.IDirective {

        var onRender = function (eventArgs: RenderEventArgs, scope: IPaneDefineCriteriaScope) {
            console.log('PaneDefineCriteria::onRender', eventArgs);

            scope.fields = eventArgs.fields;
            scope.criteria = eventArgs.criteria.clone();
            scope.isVisible = true;
        };


        var onHide = function (scope: IPaneDefineCriteriaScope) {
            scope.isVisible = false;
            scope.criteria = null;
            scope.fields = [];
        };
        

        var removeCriteria = function (scope: IPaneDefineCriteriaScope) {
            var eventArgs = new CriteriaRemovingEventArgs(scope.criteria.id);
            scope.$emit(MessageType[MessageType.PaneDefineCriteria_CriteriaRemoved], eventArgs);
        };


        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/PaneDefineCriteria',
            scope: {},
            controller: ($scope: IPaneDefineCriteriaScope): void => {
                $scope.removeCriteria = function () { removeCriteria($scope); }

                $scope.$on(MessageType[MessageType.PaneDefineCriteria_Render],
                    (event: ng.IAngularEvent, eventArgs: RenderEventArgs) => onRender(eventArgs, $scope));

                $scope.$on(MessageType[MessageType.PaneDefineCriteria_Hide],
                    (event: ng.IAngularEvent) => onHide($scope));
            }
        };
    }
}

app.directive('paneDefineCriteria', dockyard.directives.paneDefineCriteria.PaneDefineCriteria);
