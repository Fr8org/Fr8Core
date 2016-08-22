module dockyard.controllers {
    'use strict';

    export interface ChangePasswordScope extends ng.IScope {
        CurrentPassword: string;
        NewPassword: string;
        ConfirmNewPassword: string;
        Submit: (isValid: boolean) => void;
        Message: string;
    }

    class ManageUserController {
        public static $inject = [
            '$rootScope',
            '$scope',
            '$q',
            'UserService'
        ];

        constructor(
            private $rootScope: interfaces.IAppRootScope,
            private $scope: ChangePasswordScope,
            private $q: ng.IQService,
            private UserService: services.IUserService) {

            $scope.Submit = <(isValid: boolean) => void>angular.bind(this, this.Submit);
            $scope.NewPassword = "";
            $scope.ConfirmNewPassword = "";
            $scope.CurrentPassword = "";
        }

        private Submit(isValid) {
            if (isValid) {
                this.UserService.update({
                    oldPassword: this.$scope.CurrentPassword,
                    newPassword: this.$scope.NewPassword,
                    confirmPassword: this.$scope.ConfirmNewPassword
                }).$promise.then(
                    (result) => {
                        this.$scope.Message = "Your password has been changed!";
                    },
                    (failResponse) => {
                        this.$scope.Message = failResponse.data.details.exception.Message;
                    });
            } else {
                this.$scope.Message = "There are still invalid fields below";
            }
        };
    }

    app.controller('ManageUserController', ManageUserController);
}