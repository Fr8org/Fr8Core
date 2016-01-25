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
    }


    export function UpstreamCrateChooser(): ng.IDirective {
        var controller = ['$scope', 'CrateHelper', ($scope: IUpstreamCrateChooserScope, crateHelper: services.CrateHelper) => {

            var populateListItems = (ddlb: model.DropDownList) => {
                var ddList = Array<model.ControlDefinitionDTO>();
                ddList.push(ddlb);
                crateHelper.populateListItemsFromDataSource(ddList, $scope.currentAction.crateStorage);
            };

            crateHelper.populateListItemsFromDataSource($scope.field.selectedCrates, $scope.currentAction.crateStorage);
            
            $scope.onChange = () => {
                if ($scope.change != null && angular.isFunction($scope.change)) {
                    $scope.change()($scope.field);
                }
            };

            $scope.addRow = () => {
                var labelChooser = new model.DropDownList();
                labelChooser.type = 'DropDownList';
                labelChooser.name = $scope.field.name + '_lbl_dropdown_' + $scope.field.selectedCrates.length;
                labelChooser.source = $scope.field.selectedCrates[0].source;
                populateListItems(labelChooser);
                $scope.field.selectedCrates.push(labelChooser);
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
                currentAction: '='
            }
        };
    }

    app.directive('upstreamCrateChooser', UpstreamCrateChooser);
}