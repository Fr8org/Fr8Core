module dockyard.controllers {
    'use strict';

    export interface ChangePasswordScope extends ng.IScope {
        CurrentPassword: string;
        NewPassword: string;
        ConfirmNewPassword: string;
        Submit: (isValid: boolean) => void;
        Message: any;
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
            $scope.Message = new Object();
        }

        private Submit(isValid, sub) {
            if (isValid) {
                this.UserService.update({
                    oldPassword: this.$scope.CurrentPassword,
                    newPassword: this.$scope.NewPassword,
                    confirmPassword: this.$scope.ConfirmNewPassword
                }).$promise.then(
                    (result) => {
                        this.$scope.Message.text = "Your password has been changed!";
                        this.$scope.Message.type = "info";
                    },
                    (failResponse) => {
                        this.$scope.Message.text = failResponse.data.details.exception.Message;
                        this.$scope.Message.type = "error";
                    });
            } else {
                this.$scope.Message.text = "There are still invalid fields below";
                this.$scope.Message.type = "error";
            }
        };
    }

    app.controller('ManageUserController', ManageUserController);
}