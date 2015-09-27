/// <reference path="../_all.ts" />

module dockyard.controllers {
    'use strict';

    export interface IAccountListScope extends ng.IScope {
        users: Array<interfaces.IUserDTO>;
        openDetails(user: interfaces.IUserDTO);
    }

    class AccountListController {
        // $inject annotation.
        // It provides $injector with information about dependencies to be injected into constructor
        // it is better to have it close to the constructor, because the parameters must match in count and type.
        // See http://docs.angularjs.org/guide/di
        public static $inject = [
            '$scope',
            'UserService',
            '$state'
        ];

        constructor(
            private $scope: IAccountListScope,
            private UserService: services.IUserService,
            private $state: ng.ui.IStateService) {
            
            UserService.getAll().$promise.then(function (data) {
                $scope.users = data;
            });

            $scope.openDetails = function (user) {
                $state.go('accountDetails', { id: user.id });
            }
        }
    }

    app.controller('AccountListController', AccountListController);
}