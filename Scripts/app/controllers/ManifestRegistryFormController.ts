/// <reference path="../_all.ts" />

module dockyard.controllers {
    'use strict';
    
    export interface IManifestRegistryFormScope extends ng.IScope {
        manifestDescription: interfaces.IManifestRegistryVM;
        submit: (formValidAndNotPending: boolean) => void;
        cancel: () => void;
        onChangeNameVersion: () => void; 
        validationModel: string;
        errorMessage: string;
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

        private submitted = false;
       
        constructor(
            private $scope: IManifestRegistryFormScope,
            private UserService: services.IUserService,
            private ManifestRegistryService: services.IManifestRegistryService,
            private $q: ng.IQService,
            private $modalInstance: any) {

            $scope.submit = isValid => {
                
                this.UserService.getCurrentUser().$promise.then(user => {
                    this.$scope.manifestDescription.userAccountId = user.emailAddress;
                    this.ManifestRegistryService.checkVersionAndName(this.$scope.validationModel, this.$scope.manifestDescription.userAccountId)
                })
                    .then(isNameVersionValid => {
                        if (isValid && isNameVersionValid) {
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
            $scope.onChangeNameVersion = <() => void> angular.bind(this, this.onChangeNameVersion);
        }

        private cancelForm() {
            this.$modalInstance.dismiss('cancel');
        }

        private onChangeNameVersion() {
            this.$scope.validationModel = this.$scope.manifestDescription.version + ':' + this.$scope.manifestDescription.name;
        }

        
    }

    app.directive('uniqueVersionName', dockyard.directives.validators.UniqueVersionName.instance)
        .controller('ManifestRegistryFormController', ManifestRegistryFormController);
}