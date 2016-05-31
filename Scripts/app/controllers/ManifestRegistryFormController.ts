/// <reference path="../_all.ts" />

module dockyard.controllers {
    'use strict';
    
    export interface IManifestRegistryFormScope extends ng.IScope {
        manifestDescription: interfaces.IManifestRegistryVM;
        selectedDescription: interfaces.IManifestRegistryVM;
        submit: (isValid: boolean) => void;
        cancel: () => void;
        isNameVersionOk: boolean;
        errorMessage: string;
    }

    export interface BoolValue extends ng.resource.IResource<any> {
        value: boolean;
    }

    class ManifestRegistryFormController {

        // $inject annotation.
        // It provides $injector with information about dependencies to be injected into constructor
        // it is better to have it close to the constructor, because the parameters must match in count and type.
        // See http://docs.angularjs.org/guide/di
        public static $inject = [
            '$scope',
            'ManifestRegistryService',
            '$modalInstance',
            'descriptionName'
        ];

        constructor(
            private $scope: IManifestRegistryFormScope,
            private ManifestRegistryService: services.IManifestRegistryService,
            private $modalInstance: any,
            private descriptionName) {

            if (descriptionName !== undefined) {
                
                ManifestRegistryService.getDescriptionWithLastVersion({name: descriptionName.value }).$promise.then(data => {
                    this.$scope.manifestDescription = data;
                });
            }

            $scope.submit = isValid => {

                this.ManifestRegistryService.checkVersionAndName( {version:this.$scope.manifestDescription.version,  name: this.$scope.manifestDescription.name }).$promise
                    .then(result => {
                    $scope.isNameVersionOk = result.value;
                        if (isValid && $scope.isNameVersionOk) {
                            this.ManifestRegistryService.save(this.$scope.manifestDescription).$promise.then(manifestDescription => {
                                this.$modalInstance.close(manifestDescription);
                               
                            });
                        }
                        else {
                            $scope.errorMessage = "(Version, Name) fields must be unique in ManifestRegistry";
                        }
                    });
                
            }

            $scope.cancel = <() => void> angular.bind(this, this.cancelForm);
        }

        private cancelForm() {
            this.$modalInstance.dismiss('cancel');
        }

    }

    app.controller('ManifestRegistryFormController', ManifestRegistryFormController);
}