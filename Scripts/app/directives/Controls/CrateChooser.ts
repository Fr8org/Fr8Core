/// <reference path="../../_all.ts" />
module dockyard.directives.crateChooser {
    'use strict';

    export interface ICrateChooserScope extends ng.IScope {
        field: model.CrateChooser;
        change: () => (field: model.ControlDefinitionDTO) => void;
        selectCrate: () => void;
        onChange: any;
        currentAction: model.ActivityDTO;
    }


    export function CrateChooser(): ng.IDirective {
        var controller = ['$scope', 'CrateHelper', '$modal', ($scope: ICrateChooserScope, crateHelper: services.CrateHelper, $modal: any) => {

            var populateListItems = (crateDetails: model.CrateDetails) => {
                var ddList = Array<model.ControlDefinitionDTO>();
                ddList.push(crateDetails.manifestType);
                ddList.push(crateDetails.label);
                crateHelper.populateListItemsFromDataSource(ddList, $scope.currentAction.crateStorage);
            };

            
            $scope.onChange = () => {
                if ($scope.change != null && angular.isFunction($scope.change)) {
                    $scope.change()($scope.field);
                }
            };

            $scope.selectCrate = () => {
                var modalInstance = $modal.open({
                    animation: true,
                    templateUrl: 'TextTemplate-CrateChooserSelectionModal',
                    controller: 'CrateChooser__CrateSelectorModalController',
                    size: 'm'
                });

                //modalInstance.result.then(OnExistingFileSelected);
            };
        }];

        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/CrateChooser',
            controller: controller,
            scope: {
                field: '=',
                change: '&',
                currentAction: '='
            }
        };
    }

    app.directive('crateChooser', CrateChooser);


    /*
A simple controller for Listing existing files dialog.
Note: here goes a simple (not really a TypeScript) way to define a controller. 
Not as a class but as a lambda function.
*/
    app.controller('CrateChooser__CrateSelectorModalController', ['$scope', '$modalInstance', ($scope: any, $modalInstance: any): void => {

        
        $scope.ok = () => {
            $modalInstance.close();
        };

        $scope.cancel = () => {
            $modalInstance.dismiss();
        };

    }]);
}