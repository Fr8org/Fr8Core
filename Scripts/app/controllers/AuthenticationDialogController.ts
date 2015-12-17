module dockyard.controllers {

    export interface IAuthenticationDialogScope {

    }


    export class AuthenticationDialogController {
        public static $inject = [
            '$scope'
        ];

        constructor(
            private $scope : IAuthenticationDialogScope
        ) {

        }
    }

    app.controller('AuthenticationDialogController', AuthenticationDialogController);
} 