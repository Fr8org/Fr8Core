/// <reference path="../../_all.ts" />
module dockyard.directives.controlContainer {
    
    'use strict';

    interface IMetaControlContainerScope extends ng.IScope {
        plan: model.PlanDTO;
        field: model.MetaControlContainer;
        addControl: () => void;
        change: () => (field: model.ControlDefinitionDTO) => void;
        removeMetaDescription: (index: number) => void;
        currentAction: model.ActivityDTO;
        getIndex: (field: model.ControlMetaDescriptionDTO) => number;
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    export function MetaControlContainer(): ng.IDirective {
        var controller = ['$scope', '$modal', ($scope: IMetaControlContainerScope, $modal: any) => {
            var triggerChange = () => {
                if ($scope.change != null && angular.isFunction($scope.change)) {
                    $scope.change()($scope.field);
                }
            };

            var onControlSelected = (control: model.ControlMetaDescriptionDTO) => {
                $scope.field.metaDescriptions.push(control);
                triggerChange();
            };

            $scope.removeMetaDescription = (index: number) => {
                $scope.field.metaDescriptions.splice(index, 1);
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
            scope: {
                plan: '=',
                field: '=',
                currentAction: '='
            }
        };
    }

    app.directive('metaControlContainer', MetaControlContainer);

    app.controller('MetaControlContainer__MetaDescSelectionController', ['$scope', '$modalInstance', ($scope: any, $modalInstance: any): void => {

        //we have 3 meta descriptions for now
        $scope.metaDescriptions = [
            new model.TextBoxMetaDescriptionDTO(),
            new model.TextBlockMetaDescriptionDTO(),
            new model.FilePickerMetaDescriptionDTO(),
            new model.GenerateExternalObjectChooserDTO()
        ];

        $scope.selectControl = (control: model.ControlMetaDescriptionDTO) => {
            $modalInstance.close(control);
        };

        $scope.cancel = () => {
            $modalInstance.dismiss();
        };

    }]);
}