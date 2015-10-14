/// <reference path="../_all.ts" />

module dockyard.controllers {
    'use strict';

    export interface IContainerDetailsScope extends ng.IScope {
        container: interfaces.IContainerVM;
        errorMessage: string;
        id: string
    }

    class ContainerDetailsController {
        // $inject annotation.
        // It provides $injector with information about dependencies to be injected into constructor
        // it is better to have it close to the constructor, because the parameters must match in count and type.
        // See http://docs.angularjs.org/guide/di
        public static $inject = [
            '$rootScope',
            '$scope',
            'ContainerService',
            '$stateParams'
        ];

        constructor(
            private $rootScope: interfaces.IAppRootScope,
            private $scope: IContainerDetailsScope,
            private ContainerService: services.IContainerService,
            private $stateParams: any) {
            
            // Load Container detailed information
            $scope.id = $stateParams.id;
            if (/^[0-9]+$/.test($scope.id) && parseInt($scope.id) > 0) {
                $scope.container = ContainerService.getSingle({ id: $stateParams.id });
            }
        }
    }

    app.controller('ContainerDetailsController', ContainerDetailsController);
}