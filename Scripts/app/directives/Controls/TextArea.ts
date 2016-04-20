/// <reference path="../../_all.ts" />
module dockyard.directives.textBlock {
    'use strict';

    export interface ITextAreaScope extends ng.IScope {
        field: model.TextArea;
        buttonSet: Array<Array<String>>;
        style:string;
        isDisabled: boolean;
        toolbars: string;
        blur: () => void;
        onBlur: (field: model.ControlDefinitionDTO) => void;
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    export function TextArea(): ng.IDirective {

        var _availableButtons = ['h1', 'h2', 'h3', 'h4', 'h5', 'h6', 'p', 'bold', 'italics', 'underline', 'ul', 'undo', 'redo', 'html', 'insertImage', 'insertLink'];
        var _disabledButtons = [];
        var _buttonSet = [_availableButtons, _disabledButtons];

        var controller = ['$scope', ($scope: ITextAreaScope) => {
            $scope.blur = () => {
                if ($scope.onBlur && angular.isFunction($scope.onBlur)) {
                    $scope.onBlur($scope.field);
                }
            }
            $scope.buttonSet = _buttonSet;
            $scope.style = $scope.field.isReadOnly ? "readOnlyTextArea" : null;
            $scope.isDisabled = $scope.field.isReadOnly;
            $scope.toolbars = $scope.field.isReadOnly ? "[]" : null;

        }];

        return {
            restrict: 'E',
            replace: true,
            templateUrl: '/AngularTemplate/TextArea',
            controller: controller,
            scope: {
                field: '=',
                onBlur: '&'
            }
        };
    }

    app.directive('textArea', TextArea);
}