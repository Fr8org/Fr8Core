/// <reference path="../../_all.ts" />
module dockyard.directives.controlContainer {
    
    'use strict';

    interface IControlContainerScope extends ng.IScope {
        plan: model.PlanDTO;
        field: model.ControlContainer;
        addTransition: () => void;
        getOperationField: (transition: model.ContainerTransitionField) => model.DropDownList;
        onOperationChange: (transition: model.ContainerTransitionField) => void;
        onTargetChange: (transition: model.ContainerTransitionField) => void;
        change: () => (field: model.ControlDefinitionDTO) => void;
        currentAction: model.ActivityDTO;
        removeTransition: (index: number) => void;
        PCA: directives.paneConfigureAction.IPaneConfigureActionController;
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    export function ControlContainer(): ng.IDirective {


        var controller = ['$scope', '$timeout', ($scope: IControlContainerScope, $timeout: ng.ITimeoutService) => {

            

            var triggerChange = () => {
                if ($scope.change != null && angular.isFunction($scope.change)) {
                    $scope.change()($scope.field);
                }

                //informJumpTargets();
            };
            
        }];

        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/ControlContainer',
            controller: controller,
            scope: {
                plan: '=',
                field: '=',
                currentAction: '='
            }
        };
    }

    app.directive('controlContainer', ControlContainer);
}