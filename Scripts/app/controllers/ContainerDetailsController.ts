/// <reference path="../_all.ts" />

module dockyard.controllers {
    'use strict';

    export interface IContainerDetailsScope extends ng.IScope {
        container: interfaces.IContainerVM;
        errorMessage: string;
        id: string
    }

    class ContainerDetailsController {
        // $inject static.
        // http://nick.perfectedz.com/angular-typescript-di-tip/

        public static $inject = [
            '$scope',
            'ContainerService',
            '$stateParams'
        ];

        constructor(
            private $scope: IContainerDetailsScope,
            private ContainerService: services.IContainerService,
            private $stateParams: any) {
            
            // Load Container detailed information
            // ParseInt use the Radix otherwise Internet explorer gives different result

            $scope.id = $stateParams.id;
            // if (/^[0-9]+$/.test($scope.id) && parseInt($scope.id,10) > 0) {
            //     $scope.container = ContainerService.getSingle({ id: $stateParams.id });
            // }
            $scope.container = ContainerService.getSingle({ id: $stateParams.id });
        }
    }

    app.controller('ContainerDetailsController', ContainerDetailsController);
}