/// <reference path="../../_all.ts" />
module dockyard.directives.textBlock {
    'use strict';

    export interface ITextAreaScope extends ng.IScope {
        field: model.TextAreaControlDefinitionDTO;
        buttonSet: Array<Array<String>>;
        style:string;
        isDisabled: boolean;
        toolbars:string;
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    //class TextArea implements ng.IDirective {
    export function TextArea(): ng.IDirective {

        var _availableButtons = ['h1', 'h2', 'h3', 'h4', 'h5', 'h6', 'p', 'bold', 'italics', 'underline', 'ul', 'undo', 'redo', 'html', 'insertImage', 'insertLink'];
        var _disabledButtons = [];
        var _buttonSet = [_availableButtons, _disabledButtons];

        var controller = ['$scope', ($scope: ITextAreaScope) => {
            
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
                field: '='
            }
        };
    }

    app.directive('textArea', TextArea);
}