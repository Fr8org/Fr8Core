/// <reference path="../_all.ts" />

module dockyard.controllers {
    'use strict';

    export interface IManifestRegistryListScope extends ng.IScope {
        manifestRegistry: Array<interfaces.IManifestRegistryVM>;
        showAddManifestDescriptionModal: () => void;
        showModalWithPopulatedValues: (manifestDescription: interfaces.IManifestRegistryVM) => void;
        goToPrefilledManifestSubmissionForm: (manifestDescription: interfaces.IManifestRegistryVM) => void;
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

            $scope.goToPrefilledManifestSubmissionForm = <(manifestDescription: interfaces.IManifestRegistryVM) => void>angular.bind(this, this.goToPrefilledManifestSubmissionForm);

            ManifestRegistryService.query().$promise.then(data => {
                    $scope.manifestRegistry = data;
            });

        }

        private goToPrefilledManifestSubmissionForm(manifestDescription: interfaces.IManifestRegistryVM) {

            var url = "https://docs.google.com/forms/d/1Uc8-_qLmYl3dMs8v8fDlEjPbsO7HfZz6ugReNKq-XmY/viewform?entry.580225613=#ManifestType&entry.786138775=#Version&entry.560315139=#SampleJson&entry.255557102=#Description&entry.24056202=#RegBy"
                .replace("#ManifestType", manifestDescription.name)
                .replace("#Version", manifestDescription.version)
                .replace("#SampleJson", manifestDescription.sampleJSON)
                .replace("#Description", manifestDescription.description)
                .replace("#RegBy", manifestDescription.registeredBy);
            window.open(url);
        };
    }

    app.controller('ManifestRegistryListController', ManifestRegistryListController);
}