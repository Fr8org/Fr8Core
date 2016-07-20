/// <reference path="../../_all.ts" />
module dockyard.directives.buildMessageAppender {
    'use strict';

    interface IBuildMessageAppenderScope extends ng.IScope {
        field: model.TextArea;
        buttonSet: Array<Array<String>>;
        currentAction: model.ActivityDTO;
        style: string;
        change: () => (field: model.ControlDefinitionDTO) => void;
        isDisabled: boolean;
        toolbars: string;
        dropDownListBox: model.DropDownList;
        insertToTextBox: (value: string) => void;
        onBlur: (field: model.TextArea) => void;

    }

    export function BuildMessageAppender(): ng.IDirective {

        
        var controller = ['$scope', '$element', 'textAngularManager', ($scope: IBuildMessageAppenderScope, element: ng.IAugmentedJQuery, textAngularManager: any) => {
            $scope.dropDownListBox = new model.DropDownList();
            $scope.dropDownListBox.label = "Select Fields";
            $scope.dropDownListBox.source = new model.FieldSource();
            $scope.dropDownListBox.source.requestUpstream = true;
            $scope.dropDownListBox.source.manifestType = "Field Description";
            $scope.onBlur = (field: model.TextArea) => {
               
            };

            $scope.insertToTextBox = () => {
                if (!angular.isUndefined($scope.dropDownListBox.selectedKey)) {
                    $scope.field.value += '[' + $scope.dropDownListBox.selectedKey + ']';
                }
            };
        }];

        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/BuildMessageAppender',
            controller: controller,
            scope: {
                plan: '=',
                field: '=',
                currentAction: '=',
                change: '&',
                isDisabled:'='
            }
        };
    }

    app.directive('buildMessageAppender', BuildMessageAppender);
}