/// <reference path="../_all.ts" />

module dockyard.controllers {
    'use strict';

    export interface IManifestRegistryListScope extends ng.IScope {
        manifestRegistry: Array<interfaces.IManifestRegistryVM>;
        goToManifestSubmissionForm:() => void;
    }

    class ManifestRegistryListController {

        // $inject annotation.
        // It provides $injector with information about dependencies to be injected into constructor
        // it is better to have it close to the constructor, because the parameters must match in count and type.
        // See http://docs.angularjs.org/guide/di
        public static $inject = [
            '$scope',
            'ManifestRegistryService',
            '$modal',
            '$http'
        ];

        constructor(
            private $scope: IManifestRegistryListScope,
            private ManifestRegistryService: services.IManifestRegistryService,
            private $modal: any,
            private $http: any) {

            $scope.goToManifestSubmissionForm = function () {
                $http.get('api/manifest_registries/submit')
                     .then(url => {
                         window.open(url);
                    });
            };

            
            ManifestRegistryService.query().$promise.then(data => {
                    $scope.manifestRegistry = data;
            });

        }
    }

    app.controller('ManifestRegistryListController', ManifestRegistryListController);
}