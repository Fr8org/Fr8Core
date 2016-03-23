/// <reference path="../../_all.ts" />
module dockyard.directives.controlContainer {
    
    'use strict';

    interface IControlContainerScope extends ng.IScope {
        plan: model.PlanDTO;
        field: model.ControlContainer;
        addControl: () => void;
        change: () => (field: model.ControlDefinitionDTO) => void;
        removeMetaDescription: (index: number) => void;
        currentAction: model.ActivityDTO;
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    export function ControlContainer(): ng.IDirective {


        var controller = ['$scope', '$compile', '$element', '$timeout', '$modal', ($scope: IControlContainerScope,
            $compile: ng.ICompileService, $element: ng.IAugmentedJQuery, $timeout: ng.ITimeoutService, $modal: any) => {

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

            $scope.addControl = () => {
                var modalInstance = $modal.open({
                    animation: true,
                    templateUrl: 'TextTemplate-ControlContainerSelectionModal',
                    controller: 'ControlContainer__MetaDescSelectionController',
                    size: 'sm'
                });

                modalInstance.result.then(onControlSelected);
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

    app.directive('delayedControl', ['$compile', ($compile: ng.ICompileService) => ({
        scope: {
            currentAction: '=',
            field: '=',
            plan: '='
        },
        template: '',
        link: (scope: ng.IScope,elem: ng.IAugmentedJQuery,attr: ng.IAttributes) => {
            elem.append("<configuration-control plan='plan' current-action='currentAction'' field='field'></configuration-control>");
            $compile(elem.contents())(scope);
        }
    })]);

    app.controller('ControlContainer__MetaDescSelectionController', ['$scope', '$modalInstance', ($scope: any, $modalInstance: any): void => {

        //we have 3 meta descriptions for now
        $scope.metaDescriptions = [ new model.TextBoxMetaDescriptionDTO(), new model.TextBlockMetaDescriptionDTO(), new model.FilePickerMetaDescriptionDTO() ];

        $scope.selectControl = (control: model.ControlMetaDescriptionDTO) => {
            $modalInstance.close(control);
        };

        $scope.cancel = () => {
            $modalInstance.dismiss();
        };

    }]);
}