/// <reference path="../_all.ts" />

module dockyard.controllers {
    'use strict';

    export interface IOrganizationSettingsControllerScope extends ng.IScope {
    }

    class OrganizationSettingsController {
        // $inject annotation.
        // It provides $injector with information about dependencies to be injected into constructor
        // it is better to have it close to the constructor, because the parameters must match in count and type.
        // See http://docs.angularjs.org/guide/di
        public static $inject = [
            '$scope',
            'OrganizationSettingsService'
        ];

        constructor(
            private $scope: IOrganizationSettingsControllerScope,
            private OrganizationSettingsService: services.IOrganizationSettingsService) {

            OrganizationSettingsService.add("","","","");

        }
    }

    app.controller('OrganizationSettingsController', OrganizationSettingsController);
}