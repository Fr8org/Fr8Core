/// <reference path="../_all.ts" />

module dockyard.controllers {
    'use strict';

    export interface IPageDefinitionListScope extends ng.IScope {
        pageDefinitions: Array<interfaces.IPageDefinitionVM>;
        showAddPageDefinitionModal: () => void;
        showModalWithPopulatedValues: (pageDefinition: interfaces.IPageDefinitionVM) => void;
    }

    class PageDefinitionListController {

        // $inject annotation.
        // It provides $injector with information about dependencies to be injected into constructor
        // it is better to have it close to the constructor, because the parameters must match in count and type.
        // See http://docs.angularjs.org/guide/di
        public static $inject = [
            '$scope',
            'PageDefinitionService',
            '$modal'
        ];

        constructor(
            private $scope: IPageDefinitionListScope,
            private PageDefinitionService: services.IPageDefinitionService,
            private $modal: any) {

            $scope.showAddPageDefinitionModal = <() => void>angular.bind(this, this.showAddPageDefinitionModal);

            $scope.showModalWithPopulatedValues = <(pageDefinition: interfaces.IPageDefinitionVM) => void>angular
                .bind(this, this.showModalWithPopulatedValues);

            PageDefinitionService.query().$promise.then(data => {
                $scope.pageDefinitions = data;                
            });
        }

        private showAddPageDefinitionModal() {

            this.$modal.open({
                animation: true,
                templateUrl: 'pageDefinitionFormModal',
                controller: 'PageDefinitionFormController',
                resolve: {
                    definitionId: function () {
                        return undefined;
                    }
                }
            })
                .result.then(() => {
                    this.PageDefinitionService.query().$promise.then(data => {
                        this.$scope.pageDefinitions = data;
                    });
                });
        }

        private showModalWithPopulatedValues(pageDefinition: interfaces.IPageDefinitionVM) {
            var definitionId = { value: pageDefinition.id };

            this.$modal.open({
                animation: true,
                templateUrl: 'pageDefinitionFormModal',
                controller: 'PageDefinitionFormController',
                resolve: {
                    definitionId: function () {
                        return definitionId;
                    }
                }
            })
                .result.then(() => {
                    this.PageDefinitionService.query().$promise.then(data => {
                        this.$scope.pageDefinitions = data;
                    });
                });
        }
    }

    app.controller('PageDefinitionListController', PageDefinitionListController);
}