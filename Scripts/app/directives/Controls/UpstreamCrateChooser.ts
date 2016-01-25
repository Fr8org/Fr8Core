/// <reference path="../../_all.ts" />
module dockyard.directives.upstreamCrateChooser {
    'use strict';

    export interface IUpstreamCrateChooserScope extends ng.IScope {
        field: model.TextSource;
        change: () => (field: model.ControlDefinitionDTO) => void;
        onChange: any;
    }


    export function UpstreamCrateChooser(): ng.IDirective {

        var uniqueDirectiveId = 1;
        var controller = ['$scope', ($scope: IUpstreamCrateChooserScope) => {
            
            $scope.onChange = (fieldName: string) => {
                if ($scope.change != null && angular.isFunction($scope.change)) {
                    $scope.change()($scope.field);
                }
            };
        }];

        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/UpstreamCrateChooser',
            controller: controller,
            scope: {
                field: '=',
                change: '&'
            }
        };
    }

    app.directive('upstreamCrateChooser', UpstreamCrateChooser);
}