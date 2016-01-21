/// <reference path="../_all.ts" />

module dockyard.controllers {
    'use strict';

    export interface IManifestRegistryFormScope extends ng.IScope {
        manifestDescription: interfaces.IManifestRegistryVM;
        submit: () => void;
        cancel: () => void;
    }

    class ManifestRegistryFormController {

        // $inject annotation.
        // It provides $injector with information about dependencies to be injected into constructor
        // it is better to have it close to the constructor, because the parameters must match in count and type.
        // See http://docs.angularjs.org/guide/di
        public static $inject = [
            '$scope',
            'UserService',
            'ManifestRegistryService',
            '$modalInstance'
        ];

        constructor(
            private $scope: IManifestRegistryFormScope,
            private UserService: services.IUserService,
            private ManifestRegistryService: services.IManifestRegistryService,
            private $modalInstance: any) {

            $scope.submit = <() => void> angular.bind(this, this.sumbitForm);
            $scope.cancel = <() => void> angular.bind(this, this.cancelForm);
        }

        private sumbitForm() {
            this.UserService.getCurrentUser().$promise.then(user => {
                this.$scope.manifestDescription.userAccountId = user.emailAddress;
                this.ManifestRegistryService.save(this.$scope.manifestDescription).$promise.then(manifestDescription => {
                    this.$modalInstance.close(manifestDescription);
                });
            });
            
        }

        private cancelForm() {
            this.$modalInstance.dismiss('cancel');
        }
    }

    app.controller('ManifestRegistryFormController', ManifestRegistryFormController);
}