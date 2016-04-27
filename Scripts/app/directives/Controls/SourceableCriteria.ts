/// <reference path="../../_all.ts" />
module dockyard.directives.sourceableCriteria {
    'use strict';

    import planEvents = dockyard.Fr8Events.Plan;

    export interface ISourceableCriteriaScope extends ng.IScope {
        field: model.SourceableCriteria;
        change: () => (field: model.ControlDefinitionDTO) => void;
        onChange: any;
        onFocus: any;
        uniqueDirectiveId: number;
        isFocused: boolean;
    }

    //Setup aliases
    import pca = dockyard.directives.paneConfigureAction;

    export function SourceableCriteria(): ng.IDirective {

        var uniqueDirectiveId = 1;
        var controller = ['$scope', ($scope: ISourceableCriteriaScope) => {
            $scope.uniqueDirectiveId = ++uniqueDirectiveId;
            $scope.onChange = (fieldName: string) => {
                if ($scope.change != null && angular.isFunction($scope.change)) {
                    $scope.change()($scope.field);

                    $scope.isFocused = false;
                    $scope.$emit(pca.MessageType[pca.MessageType.PaneConfigureAction_ConfigureFocusElement],
                        new pca.ConfigureFocusElementArgs(null));
                }
            };

            $scope.$on(<any>planEvents.ON_FIELD_FOCUS, function (event, args: pca.CallConfigureResponseEventArgs) {
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

        var link = function ($scope, $element, attrs) {
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
            templateUrl: '/AngularTemplate/SourceableCriteria',
            controller: controller,
            link: link,
            scope: {
                field: '=',
                change: '&',
                isFocused: "=?"
            }
        };
    }

    app.directive('sourceableCriteria', SourceableCriteria);
}