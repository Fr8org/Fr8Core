/// <reference path="../../_all.ts" />
module dockyard.directives.controlList {
    
    'use strict';

    interface IControlListScope extends ng.IScope {
        plan: model.PlanDTO;
        field: model.ControlList;
        addControlGroup: () => void;
        removeControlGroup: (index: number) => void;
        change: () => (field: model.ControlDefinitionDTO) => void;
        currentAction: model.ActivityDTO;
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    export function ControlList(): ng.IDirective {

        var controller = ['$scope', ($scope: IControlListScope) => {

            var triggerChange = () => {
                if ($scope.change != null && angular.isFunction($scope.change)) {
                    $scope.change()($scope.field);
                }
            };

            $scope.addControlGroup = () => {
                //let's read the template
                var controls = angular.copy($scope.field.templateContainer.template);
                $scope.field.controlGroups.push(controls);
                triggerChange();
            };

            $scope.removeControlGroup = (index: number) => {
                $scope.field.controlGroups.splice(index, 1);
                triggerChange();
            };
        }];

        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/ControlList',
            controller: controller,
            scope: {
                plan: '=',
                field: '=',
                currentAction: '='
            }
        };
    }

    app.directive('controlList', ControlList);
}