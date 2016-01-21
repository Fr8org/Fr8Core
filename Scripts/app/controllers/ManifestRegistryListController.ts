/// <reference path="../_all.ts" />

module dockyard.controllers {
    'use strict';

    export interface IManifestRegistryListScope extends ng.IScope {
        manifestRegistry: Array<interfaces.IManifestRegistryVM>;
        showAddManifestDescriptionModal: () => void;
    }

    class ManifestRegistryListController {

        // $inject annotation.
        // It provides $injector with information about dependencies to be injected into constructor
        // it is better to have it close to the constructor, because the parameters must match in count and type.
        // See http://docs.angularjs.org/guide/di
        public static $inject = [
            '$scope',
            'UserService',
            'ManifestRegistryService',
            '$modal'
        ];

        constructor(
            private $scope: IManifestRegistryListScope,
            private UserService: services.IUserService,
            private ManifestRegistryService: services.IManifestRegistryService,
            private $modal: any) {

            $scope.showAddManifestDescriptionModal = <() => void> angular.bind(this, this.showAddManifestDescriptionModal);

            this.UserService.getCurrentUser().$promise.then(user => {
                ManifestRegistryService.query({ userAccountId: user.emailAddress }).$promise.then(data => {
                    $scope.manifestRegistry = data;
                });
            });

            
        }

        private showAddManifestDescriptionModal() {
            this.$modal.open({
                animation: true,
                templateUrl: 'manifestDescriptionFormModal',
                controller: 'ManifestRegistryFormController'
            })
                .result.then(manifestDescription => {
                this.$scope.manifestRegistry.push(manifestDescription);
                });
        }
    }

    app.controller('ManifestRegistryListController', ManifestRegistryListController);
}