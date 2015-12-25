module dockyard.controllers {

    import pwd = dockyard.directives.paneWorkflowDesigner;

    export interface IManageAuthTokenScope extends ng.IScope {
        terminals: Array<interfaces.IManageAuthToken_TerminalVM>;
        revokeToken: (authToken: model.ManageAuthToken_AuthTokenDTO) => void;
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

            $scope.revokeToken = function (authToken: model.ManageAuthToken_AuthTokenDTO) {
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