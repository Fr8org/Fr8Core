/// <reference path="../_all.ts" />

module dockyard.controllers {
    'use strict';

    export interface IPageDefinitionListScope extends ng.IScope {
        pageDefinitions: Array<interfaces.IPageDefinitionVM>;
        openDetails(pd: interfaces.IPageDefinitionVM);
        dtOptionsBuilder: any;
    }

    class PageDefinitionListController {
        // $inject annotation.
        // It provides $injector with information about dependencies to be injected into constructor
        // it is better to have it close to the constructor, because the parameters must match in count and type.
        // See http://docs.angularjs.org/guide/di
        public static $inject = [
            '$scope',
            'PageDefinitionService',
            '$state',
            'DTOptionsBuilder'
        ];

        constructor(
            private $scope: IPageDefinitionListScope,
            private PageDefinitionService: services.IPageDefinitionService,
            private $state: ng.ui.IStateService,
            private DTOptionsBuilder) {

            $scope.dtOptionsBuilder = DTOptionsBuilder.newOptions().withOption('language', {
                'sEmptyTable': '',
                'zeroRecords': ''
            });

            PageDefinitionService.getAll().$promise.then(function (data) {
                $scope.pageDefinitions = data;

                // reconfigure the message 

                $scope.dtOptionsBuilder = DTOptionsBuilder.newOptions().withOption('language', {
                    'sEmptyTable': 'No data available in table',
                    'zeroRecords': 'No matching records found'
                });
            });

            $scope.openDetails = function (pd) {
                $state.go('', { id: pd.title });
            }
        }
    }

    app.controller('PageDefinitionListController', PageDefinitionListController);
}