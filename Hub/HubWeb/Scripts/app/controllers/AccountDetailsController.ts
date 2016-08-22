/// <reference path="../_all.ts" />

module dockyard.controllers {
    'use strict';

    export interface IAccountDetailsScope extends ng.IScope {
        user: interfaces.IUserDTO;
        profiles: Array<interfaces.IProfileDTO>;
        submit: (isValid: boolean) => void;
        cancel: () => void;
    }

    class AccountDetailsController {
        // $inject annotation.
        // It provides $injector with information about dependencies to be injected into constructor
        // it is better to have it close to the constructor, because the parameters must match in count and type.
        // See http://docs.angularjs.org/guide/di
        public static $inject = [
            '$scope',
            '$state',
            'UserService'
        ];

        constructor(
            private $scope: IAccountDetailsScope,
            private $state: ng.ui.IStateService,
            private UserService: services.IUserService) {
            //            private $state: ng.ui.IStateService) {

            UserService.getProfiles().$promise.then(function (data) {
                $scope.profiles = data;
            });

            UserService.get({ id: $state.params['id'] }).$promise.then(function (data) {
                $scope.user = data;
            });

            //Save button
            $scope.submit = function (isValid) {
                if (isValid) {
                    var result = UserService.updateUserProfile($scope.user);
                    result.$promise.then(() => { $state.go('accounts'); });
                }
            };

            $scope.cancel = function () {
                $state.go('accounts');
            };
        }
    }

    app.controller('AccountDetailsController', AccountDetailsController);
}