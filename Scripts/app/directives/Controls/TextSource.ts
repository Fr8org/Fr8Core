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
                    $scope.$emit(pca.MessageType[pca.MessageType.PaneConfigureAction_ConfigureFocusElement],
                        new pca.ConfigureFocusElementArgs(null));
                }
            };

            $scope.$on("onFieldFocus", function(event, args:pca.CallConfigureResponseEventArgs) {
                if ($scope.field.name === args.focusElement.name) {
                    $scope.isFocused = true;
                } else {
                    $scope.isFocused = false;
                }
            });

            $scope.onFocus = (fieldName: string) => {
                $scope.$emit(pca.MessageType[pca.MessageType.PaneConfigureAction_ConfigureFocusElement],
                    new pca.ConfigureFocusElementArgs($scope.field));
            };
        }];

        var link = function($scope, $element, attrs) {
            //watch function for programatically setting focus on html element
            $scope.$watch("isFocused", function (currentValue, previousValue) {
                if (currentValue === true && !previousValue) {
                    var theElement = $element.find("input[type='text']")[0];
                    theElement.focus();
                    $scope.field.valueSource = 'specific';
                }
            });
        }

        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/TextSource',
            controller: controller,
            link: link,
            scope: {
                field: '=',
                change: '&',
                isFocused : "=?"
            }
        };
    }

    app.directive('textSource', TextSource);
}