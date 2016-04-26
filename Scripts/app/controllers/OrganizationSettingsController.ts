/// <reference path="../_all.ts" />

module dockyard.controllers.OrganizationSettingsController {
    'use strict';

    export interface IOrganizationSettingsControllerScope extends ng.IScope {
        name: string;
        themeName: string;
        color: string;
        logoUrl: string;
        addOrganization: () => void;
    }

    class OrganizationSettingsController {
        // $inject annotation.
        // It provides $injector with information about dependencies to be injected into constructor
        // it is better to have it close to the constructor, because the parameters must match in count and type.
        // See http://docs.angularjs.org/guide/di
        public static $inject = [
            '$scope',
            'OrganizationSettingsService',
            'UserService'
        ];

        constructor(
            private $scope: IOrganizationSettingsControllerScope,
            private OrganizationSettingsService: services.IOrganizationSettingsService,
            private UserService: services.IUserService) {

            var organization = OrganizationSettingsService.get(UserService.getCurrentUser().organizationId);
            

            $scope.addOrganization = function () {
                var organization: model.OrganizationDTO = {
                    name: $scope.name,
                    themeName: $scope.themeName,
                    color: $scope.color,
                    logoUrl: $scope.logoUrl
                }
                OrganizationSettingsService.add(organization);
            };

        }
    }

    app.controller('OrganizationSettingsController', OrganizationSettingsController);
}