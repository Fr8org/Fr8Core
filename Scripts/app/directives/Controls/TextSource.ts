/// <reference path="../../_all.ts" />
module dockyard.directives.textSource {
    'use strict';

    export interface ITextSourceScope extends ng.IScope {
        field: model.TextSource;
        change: () => (field: model.ControlDefinitionDTO) => void;
        onChange: any;
        uniqueDirectiveId: number;
    }

    
    export function TextSource(): ng.IDirective {
        
        var uniqueDirectiveId = 1;
        var controller = ['$scope', ($scope: ITextSourceScope) => {
            $scope.uniqueDirectiveId = ++uniqueDirectiveId;
            $scope.onChange = (fieldName: string) => {
                if ($scope.change != null && angular.isFunction($scope.change)) {
                    $scope.change()($scope.field);
                }
            };
        }];

        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/TextSource',
            controller: controller,
            scope: {
                field: '=',
                change: '&'
            }
        };
    }

    app.directive('textSource', TextSource);
}