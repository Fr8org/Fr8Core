/// <reference path="../_all.ts" />

module dockyard.controllers {
    'use strict';

    export interface IManifestRegistryListScope extends ng.IScope {
        manifestRegistry: Array<interfaces.IManifestRegistryVM>;
        showAddManifestDescriptionModal: () => void;
        showModalWithPopulatedValues: (manifestDescription: interfaces.IManifestRegistryVM) => void;
    }

    class ManifestRegistryListController {

        // $inject annotation.
        // It provides $injector with information about dependencies to be injected into constructor
        // it is better to have it close to the constructor, because the parameters must match in count and type.
        // See http://docs.angularjs.org/guide/di
        public static $inject = [
            '$scope',
            'ManifestRegistryService',
            '$modal'
        ];

        constructor(
            private $scope: IManifestRegistryListScope,
            private ManifestRegistryService: services.IManifestRegistryService,
            private $modal: any) {

            $scope.showAddManifestDescriptionModal = <() => void> angular.bind(this, this.showAddManifestDescriptionModal);
            $scope.showModalWithPopulatedValues = <(manifestDescription: interfaces.IManifestRegistryVM) => void> angular.bind(this, this.showModalWithPopulatedValues);

            ManifestRegistryService.query().$promise.then(data => {
                    $scope.manifestRegistry = data;
            });

        }

        private showAddManifestDescriptionModal() {
            
            this.$modal.open({
                animation: true,
                templateUrl: 'manifestDescriptionFormModal',
                controller: 'ManifestRegistryFormController',
                resolve: {
                    descriptionName: function () {
                        return undefined;
                    }
                }
            })
                .result.then(manifestDescription => {
                    this.$scope.manifestRegistry.push(manifestDescription);
                });
        }

        private showModalWithPopulatedValues(manifestDescription: interfaces.IManifestRegistryVM) {
            var descriptionName = { value: manifestDescription.name };

            this.$modal.open({
                animation: true,
                templateUrl: 'manifestDescriptionFormModal',
                controller: 'ManifestRegistryFormController',
                resolve: {
                    descriptionName: function () {
                        return descriptionName;
                    }
                }
            })
                .result.then(manifestDescription => {
                    this.$scope.manifestRegistry.push(manifestDescription);
                    this.ManifestRegistryService.query().$promise.then(data => {
                        this.$scope.manifestRegistry = data;
                    });
                });
        }

    }

    app.controller('ManifestRegistryListController', ManifestRegistryListController);
}