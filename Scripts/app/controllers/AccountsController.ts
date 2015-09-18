/// <reference path="../_all.ts" />

module dockyard.controllers {
    'use strict';

    export interface IAccountsScope extends ng.IScope {
        users: Array<interfaces.IUserDTO>;
        showDetails(user: interfaces.IUserDTO);
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
            private $scope: IAccountsScope,
            private UserService: services.IUserService) {
            
            UserService.getAll().$promise.then(function (data) {
                $scope.users = data;
            });

            $scope.showDetails = function (user) {
                
            }
        }
    }

    app.controller('AccountsController', AccountsController);
}