/// <reference path="../../_all.ts" />
module dockyard.directives.textSource {
    'use strict';

    export interface ITextSourceScope extends ng.IScope {
        field: model.TextSource;
        change: () => (field: model.ControlDefinitionDTO) => void;
        onChange: any;
        onFocus: any;
        onBlur: any;
        uniqueDirectiveId: number;
        isFocused : boolean;
    }

    //Setup aliases
    import pca = dockyard.directives.paneConfigureAction;
    
    export function TextSource(): ng.IDirective {
        
        var uniqueDirectiveId = 1;
        var controller = ['$scope', ($scope: ITextSourceScope) => {
            $scope.uniqueDirectiveId = ++uniqueDirectiveId;
            $scope.onChange = (fieldName: string) => {
                if ($scope.change != null && angular.isFunction($scope.change)) {
                    $scope.change()($scope.field);
                    $scope.isFocused = false;
                }
            };

            $scope.$on("onFieldFocus", function(event, args:pca.CallConfigureResponseEventArgs) {
                console.log("onFieldFocus is called");
                if ($scope.field.name === args.focusElement.name) {
                    $scope.isFocused = true;
                } else {
                    $scope.isFocused = false;
                }
            });

            $scope.onFocus = (fieldName: string) => {
                console.log("call onfocus from inside TextSource");
                $scope.$emit(pca.MessageType[pca.MessageType.PaneConfigureAction_ConfigureFocusElement],
                    new pca.ConfigureFocusElementArgs($scope.field));
            };
        }];

        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/TextSource',
            controller: controller,
            link: function ($scope, $element, attrs) {
                $scope.$watch("isFocused", function (currentValue, previousValue) {
                    console.log("field.isFocused called " + currentValue);
                    if (currentValue === true && !previousValue) {
                        var textSource = $element.find("input[type='text']");
                        if (textSource != undefined) {
                            textSource[0].focus();
                        }
                    } else {
                        var textSource = $element.find("input[type='text']");
                        if (textSource != undefined) {
                            textSource[0].blur();
                        }
                    }
                });
            },
            scope: {
                field: '=',
                change: '&',
                isFocused : "=?"
            }
        };
    }

    app.directive('textSource', TextSource);
}