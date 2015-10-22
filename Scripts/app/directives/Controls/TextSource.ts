/// <reference path="../../_all.ts" />
module dockyard.directives.textSource {
    'use strict';

    export interface ITextSourceScope extends ng.IScope {
        field: model.TextSource;
        change: () => (fieldName: string) => void;
        onChange: any;

    }

    export function TextSource(): ng.IDirective {
        var controller = ['$scope', function ($scope: ITextSourceScope) {
            $scope.onChange = (fieldName: string) => {
                if ($scope.change != null && angular.isFunction($scope.change)) {
                    $scope.change()($scope.field.name);
                }
            }
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