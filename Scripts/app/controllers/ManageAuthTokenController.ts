module dockyard.controllers {

    import pwd = dockyard.directives.paneWorkflowDesigner;

    export interface IManageAuthTokenScope extends ng.IScope {
        terminals: Array<interfaces.IAuthenticationTokenTerminalVM>;
        revokeToken: (authToken: model.AuthenticationTokenDTO) => void;
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

            var _reloadTerminals = function () {
                $scope.terminals = ManageAuthTokenService.list();
            };

            $scope.revokeToken = function (authToken: model.AuthenticationTokenDTO) {
                $scope.$emit(
                    pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_LongRunningOperation],
                    new pwd.LongRunningOperationEventArgs(pwd.LongRunningOperationFlag.Started)
                );

                ManageAuthTokenService.revoke(authToken)
                    .$promise
                    .finally(function () {
                        _reloadTerminals();

                        $scope.$emit(
                            pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_LongRunningOperation],
                            new pwd.LongRunningOperationEventArgs(pwd.LongRunningOperationFlag.Stopped)
                        );
                    });
            };

            _reloadTerminals();
        }
    }

    app.controller('ManageAuthTokenController', ManageAuthTokenController);
}