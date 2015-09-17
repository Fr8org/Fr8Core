/// <reference path="../_all.ts" />

module dockyard.controllers {
    'use strict';

    export interface IProcessTemplateScope extends ng.IScope {
    }

    class AccountsController {
        // $inject annotation.
        // It provides $injector with information about dependencies to be injected into constructor
        // it is better to have it close to the constructor, because the parameters must match in count and type.
        // See http://docs.angularjs.org/guide/di
        public static $inject = [
            '$rootScope',
            '$scope',
            'UserService'
        ];

        constructor(
            private $rootScope: interfaces.IAppRootScope,
            private $scope: IProcessTemplateScope,
            private UserService: services.IUserService) {

            UserService.getAll().$promise.then(function (data) {
                console.log(data);
            });
        }
    }

    app.controller('AccountsController', AccountsController);
}