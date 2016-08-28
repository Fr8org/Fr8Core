/// <reference path="../../_all.ts" />
module dockyard.directives.controlContainer {
    
    'use strict';

    interface IMetaControlContainerScope extends ng.IScope {
        plan: model.PlanDTO;
        field: model.MetaControlContainer;
        addControl: () => void;        
        change: (field: model.ControlDefinitionDTO) => void;
        removeMetaDescription: (index: number) => void;
        currentAction: model.ActivityDTO;
        getIndex: (field: model.ControlMetaDescriptionDTO) => number;

    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    export function MetaControlContainer(): ng.IDirective {
        var controller = ['$scope', '$modal', 'SubPlanService','$interval',
            ($scope: IMetaControlContainerScope, $modal: any, SubPlanService: services.ISubPlanService, $interval) => {
                var triggerChange = () => {
                    
                if ($scope.change != null && angular.isFunction($scope.change)) {
                    $scope.change($scope.field);
                }
            };

            var onControlSelected = (control: model.ControlMetaDescriptionDTO) => {
                $scope.field.metaDescriptions.push(control);
                triggerChange();
            };

            $scope.removeMetaDescription = (index: number) => {
                // Search for "subPlanId" property of nested "SelectData" controls.
                var existingSubPlanId : string = null;
                var i, j, control;
                for (i = 0; i < $scope.field.metaDescriptions.length; ++i) {
                    for (j = 0; j < $scope.field.metaDescriptions[i].controls.length; ++j) {
                        control = $scope.field.metaDescriptions[i].controls[j];
                        if (control.type === 'SelectData' && control.subPlanId) {
                            existingSubPlanId = control.subPlanId;
                            break;
                        }
                    }

                    if (existingSubPlanId) {
                        break;
                    }
                }

                // If no SubPlanId found, then simply remove control from array.
                if (!existingSubPlanId) {
                    $scope.field.metaDescriptions.splice(index, 1);
                }
                // If SubPlanId found, delete subplan first, and then remove control from array.
                else {
                    SubPlanService.delete({ id: existingSubPlanId })
                        .$promise
                        .then(() => {
                            $scope.field.metaDescriptions.splice(index, 1);
                        });
                }
            };
            
            $scope.getIndex = (control: model.ControlMetaDescriptionDTO) => {
                var ix = 1;

                for (var i = 0; i < $scope.field.metaDescriptions.length; i++)
                {
                    if ($scope.field.metaDescriptions[i] === control) {
                        return ix;
                    }
                    else if ($scope.field.metaDescriptions[i].type === control.type) {
                        ix++;
                    }
                } 

                return ix;
           }

            $scope.addControl = () => {
                // it means onChange was fired by Clicking, and modal window will not add control
                // yes it`s funny and wrong, we need have helper for parent scope search
                
                if ((<any>$scope.$parent.$parent.$parent.$parent.$parent.$parent).processing) {

                    var tryAgain = $interval($scope.addControl,1000);
                    return;
                }

                //if (PlanBuilder.processing) {
                //    return;
                //}

                var modalInstance = $modal.open({
                    animation: true,
                    templateUrl: 'TextTemplate-ControlContainerSelectionModal',
                    controller: 'MetaControlContainer__MetaDescSelectionController',
                    size: 'sm'
                });

                modalInstance.result.then(onControlSelected);
            };

        }];

        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/MetaControlContainer',
            controller: controller,
            //require: { PlanBuilder: '^^' },
            scope: {
                plan: '=',
                field: '=',
                currentAction: '=',
                change: '='
            }
        };
    }

    app.directive('metaControlContainer', MetaControlContainer);

    app.controller('MetaControlContainer__MetaDescSelectionController', ['$scope', '$modalInstance', ($scope: any, $modalInstance: any): void => {

        $scope.metaDescriptions = [
            new model.TextBoxMetaDescriptionDTO(),
            new model.TextBlockMetaDescriptionDTO(),
            new model.FilePickerMetaDescriptionDTO(),
            new model.SelectDataMetaDescriptionDTO(),
            new model.DropDownListMetaDescriptionDTO(),
            new model.RadioGroupMetaDescriptionDTO(),
            new model.CheckBoxMetaDescriptionDTO()
        ];

        $scope.selectControl = (control: model.ControlMetaDescriptionDTO) => {
            $modalInstance.close(control);
        };

        $scope.cancel = () => {
            $modalInstance.dismiss();
        };

    }]);
}