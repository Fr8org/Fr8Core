/// <reference path="../../_all.ts" />
 
module dockyard.directives.paneDefineCriteria {
    'use strict';

    export function PaneDefineCriteria(): ng.IDirective {

        var onRender = function (eventArgs: RenderEventArgs, scope: IPaneDefineCriteriaScope) {
            console.log('PaneDefineCriteria::onRender', eventArgs);

            scope.fields = eventArgs.fields;
            scope.criteria = eventArgs.criteria.clone();
            scope.isVisible = true;

            if (!scope.criteria.conditions) {
                scope.criteria.conditions = [];
            }

            if (scope.criteria.conditions.length === 0) {
                var fieldKey = '';
                if (scope.fields && scope.fields.length > 0) {
                    fieldKey = scope.fields[0].key;
                }

                var condition = new model.Condition(fieldKey, 'gt', '');
                condition.validate();

                scope.criteria.conditions.push(condition);
            }
        };


        var onHide = function (scope: IPaneDefineCriteriaScope) {
            scope.isVisible = false;
            scope.criteria = null;
            scope.fields = [];
        };
        

        var removeCriteria = function (scope: IPaneDefineCriteriaScope) {
            var eventArgs = new CriteriaRemovingEventArgs(scope.criteria.id);
            scope.$emit(MessageType[MessageType.PaneDefineCriteria_CriteriaRemoving], eventArgs);
        };


        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/PaneDefineCriteria',
            scope: {},
            controller: ($scope: IPaneDefineCriteriaScope): void => {
                $scope.operators = [
                    { text: 'Greater than', value: 'gt' },
                    { text: 'Greater than or equal', value: 'gte' },
                    { text: 'Less than', value: 'lt' },
                    { text: 'Less than or equal', value: 'lte' },
                    { text: 'Equal', value: 'eq' },
                    { text: 'Not equal', value: 'neq' }
                ];

                $scope.defaultOperator = 'gt';

                $scope.$on(MessageType[MessageType.PaneDefineCriteria_Render],
                    (event: ng.IAngularEvent, eventArgs: RenderEventArgs) => onRender(eventArgs, $scope));

                $scope.$on(MessageType[MessageType.PaneDefineCriteria_Hide],
                    (event: ng.IAngularEvent) => onHide($scope));

                $scope.removeCriteria = function () {
                    removeCriteria($scope);
                };
            }
        };
    }
}

app.directive('paneDefineCriteria', dockyard.directives.paneDefineCriteria.PaneDefineCriteria);
