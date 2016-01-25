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

            var populateListItems = (crateSelectionField: model.CrateSelectionField) => {
                var ddList = Array<model.ControlDefinitionDTO>();
                ddList.push(crateSelectionField.manifestType);
                ddList.push(crateSelectionField.label);
                crateHelper.populateListItemsFromDataSource(ddList, $scope.currentAction.crateStorage);
            };

            for (var i = 0; i < $scope.field.selectedCrates.length; i++) {
                populateListItems($scope.field.selectedCrates[i]);
            }
            
            $scope.onChange = (fieldName: string) => {
                if ($scope.change != null && angular.isFunction($scope.change)) {
                    $scope.change()($scope.field);
                }
            };

            $scope.addRow = () => {
                var crateChooserRow = new model.CrateSelectionField();
                crateChooserRow.label = new model.DropDownList();
                crateChooserRow.label.type = 'DropDownList';
                crateChooserRow.label.name = $scope.field.name + '_lbl_dropdown_' + $scope.field.selectedCrates.length;
                crateChooserRow.label.source = $scope.field.selectedCrates[0].label.source;
                crateChooserRow.manifestType = new model.DropDownList();
                crateChooserRow.manifestType.type = 'DropDownList';
                crateChooserRow.manifestType.name = $scope.field.name + '_mnfst_dropdown_' + $scope.field.selectedCrates.length;
                crateChooserRow.manifestType.source = $scope.field.selectedCrates[0].manifestType.source;
                populateListItems(crateChooserRow);
                $scope.field.selectedCrates.push(crateChooserRow);
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