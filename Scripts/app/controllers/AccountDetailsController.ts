/// <reference path="../_all.ts" />

module dockyard.controllers {
    'use strict';

    export interface IAccountDetailsScope extends ng.IScope {
        user: interfaces.IUserDTO;
    }

    class AccountDetailsController {
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
            private $scope: IAccountDetailsScope,
            private UserService: services.IUserService,
            private $state: ng.ui.IState) {

            UserService.get({ id: $state.params.id }).$promise.then(function (data) {
                $scope.user = data;
            });
        }
    }

    app.controller('AccountDetailsController', AccountDetailsController);
}