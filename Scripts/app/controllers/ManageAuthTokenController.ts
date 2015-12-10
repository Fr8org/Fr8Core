module dockyard.controllers {

    export interface IManageAuthTokenScope extends ng.IScope {
        terminals: Array<interfaces.IManageAuthToken_TerminalVM>;
    }


    export class ManageAuthTokenController {
        public static $inject = [
            '$scope',
            'ManageAuthTokenService'
        ];

        constructor(
            private $scope: IManageAuthTokenScope,
            private ManageAuthTokenService: services.IManageAuthTokenService
            ) {

            var promise = ManageAuthTokenService.get().$promise;
            promise.then(function (data) {
                $scope.terminals = <Array<interfaces.IManageAuthToken_TerminalVM>>(<any>data);
            });
        }
    }

    app.controller('ManageAuthTokensController', ManageAuthTokenController);
}