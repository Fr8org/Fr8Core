/// <reference path="../_all.ts" />

module dockyard.controllers {
    'use strict';

    export interface IPageDefinitionFormScope extends ng.IScope {
        pageDefinition: interfaces.IPageDefinitionVM;
        selectedDefinition: interfaces.IPageDefinitionVM;
        submit: (isValid: boolean) => void;
        cancel: () => void;
        errorMessage: string;
    }

    class PageDefinitionFormController {

        // $inject annotation.
        // It provides $injector with information about dependencies to be injected into constructor
        // it is better to have it close to the constructor, because the parameters must match in count and type.
        // See http://docs.angularjs.org/guide/di
        public static $inject = [
            '$scope',
            'PageDefinitionService',
            '$modalInstance',
            'definitionTitle'
        ];

        constructor(
            private $scope: IPageDefinitionFormScope,
            private PageDefinitionService: services.IPageDefinitionService,
            private $modalInstance: any,
            private definitionTitle) {

            if (definitionTitle !== undefined) {

                PageDefinitionService.getPageDefinition({ title: definitionTitle.value }).$promise.then(data => {
                    this.$scope.pageDefinition = data;
                });
            }

            $scope.submit = isValid => {
                this.PageDefinitionService.save(this.$scope.pageDefinition)
                    .$promise.then(pageDefinition => {
                        this.$modalInstance.close(pageDefinition);
                    });
            


            //this.PageDefinitionService.checkVersionAndName({
                //    version: this.$scope.pageDefinition.version,
                //    name: this.$scope.pageDefinition.name
                //    }).$promise
                //    .then(result => {
                //        $scope.isNameVersionOk = result.value;
                //        if (isValid && $scope.isNameVersionOk) {
                //            this.PageDefinitionService.save(this.$scope.pageDefinition).$promise.then(manifestDescription => {
                //                this.$modalInstance.close(manifestDescription);

                //            });
                //        }
                //        else {
                //            $scope.errorMessage = "(Version, Name) fields must be unique in ManifestRegistry";
                //        }
                //    });

            }

            $scope.cancel = <() => void>angular.bind(this, this.cancelForm);
        }

        private cancelForm() {
            this.$modalInstance.dismiss('cancel');
        }

    }

    app.controller('PageDefinitionFormController', PageDefinitionFormController);
}