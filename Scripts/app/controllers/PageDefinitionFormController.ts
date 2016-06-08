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
            'definitionId'
        ];

        constructor(
            private $scope: IPageDefinitionFormScope,
            private PageDefinitionService: services.IPageDefinitionService,
            private $modalInstance: any,
            private definitionId) {

            if (definitionId !== undefined) {

                PageDefinitionService.getPageDefinition({ id: definitionId.value }).$promise.then(data => {
                    this.$scope.pageDefinition = data;
                });
            }

            $scope.submit = isValid => {
                if (isValid) {
                    this.PageDefinitionService.save(this.$scope.pageDefinition)
                        .$promise.then(pageDefinition => {
                            this.$modalInstance.close(pageDefinition);
                        });
                }
            }

            $scope.cancel = <() => void>angular.bind(this, this.cancelForm);
        }

        private cancelForm() {
            this.$modalInstance.dismiss('cancel');
        }

    }

    app.controller('PageDefinitionFormController', PageDefinitionFormController);
}