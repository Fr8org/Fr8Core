/// <reference path="../../_all.ts" />
module dockyard.directives.upstreamCrateChooser {
    'use strict';

    export interface IUpstreamCrateChooserScope extends ng.IScope {
        field: model.UpstreamCrateChooser;
        change: () => (field: model.ControlDefinitionDTO) => void;
        addRow: () => void;
        removeRow: (rowIndex: number) => void;
        onChange: any;
        currentAction: model.ActivityDTO;
        isDisabled:boolean;
    }


    export function UpstreamCrateChooser(): ng.IDirective {
        var controller = ['$scope', 'CrateHelper', ($scope: IUpstreamCrateChooserScope, crateHelper: services.CrateHelper) => {

            var populateListItems = (crateDetails: model.CrateDetails) => {
                var ddList = Array<model.ControlDefinitionDTO>();
                ddList.push(crateDetails.manifestType);
                ddList.push(crateDetails.label);
                crateHelper.populateListItemsFromDataSource(ddList, $scope.currentAction.crateStorage);
            };

            for (var i = 0; i < $scope.field.selectedCrates.length; i++) {
                populateListItems($scope.field.selectedCrates[i]);
            }
            
            $scope.onChange = () => {
                if ($scope.change != null && angular.isFunction($scope.change)) {
                    $scope.change()($scope.field);
                }
            };

            $scope.addRow = () => {
                var labelChooser = new model.DropDownList();
                labelChooser.type = 'DropDownList';
                labelChooser.name = $scope.field.name + '_lbl_dropdown_' + $scope.field.selectedCrates.length;
                labelChooser.source = $scope.field.selectedCrates[0].label.source;
                var manifestChooser = new model.DropDownList();
                manifestChooser.type = 'DropDownList';
                manifestChooser.name = $scope.field.name + '_mnfst_dropdown_' + $scope.field.selectedCrates.length;
                manifestChooser.source = $scope.field.selectedCrates[0].manifestType.source;
                var crateDetails = new model.CrateDetails();
                crateDetails.label = labelChooser;
                crateDetails.manifestType = manifestChooser;
                populateListItems(crateDetails);
                $scope.field.selectedCrates.push(crateDetails);
            };

            $scope.removeRow = (rowIndex: number) => {
                $scope.field.selectedCrates.splice(rowIndex, 1);
            };
        }];

        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/UpstreamCrateChooser',
            controller: controller,
            scope: {
                field: '=',
                change: '&',
                currentAction: '=',
                isDisabled:'='
            }
        };
    }

    app.directive('upstreamCrateChooser', UpstreamCrateChooser);
}