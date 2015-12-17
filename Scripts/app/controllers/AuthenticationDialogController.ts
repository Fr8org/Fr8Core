module dockyard.controllers {

    export interface IAuthenticationDialogScope extends ng.IScope {
        actionIds: Array<number>;
        terminals: Array<model.ManageAuthToken_TerminalDTO>;

        linkAccount: (terminal: model.ManageAuthToken_TerminalDTO) => void;
        apply: () => void;
    }


    export class AuthenticationDialogController {
        public static $inject = [
            '$scope',
            '$http',
            '$modal',
            'urlPrefix'
        ];

        constructor(
            private $scope: IAuthenticationDialogScope,
            private $http: ng.IHttpService,
            private $modal: any,
            private urlPrefix: string
            ) {

            var terminalActions = [];

            $scope.linkAccount = function (terminal) {
                if (terminal.authenticationType === 2) {
                    _authenticateInternal(terminal);
                }
            };

            $scope.apply = function () {
            };

            var _authenticateInternal = function (terminal: model.ManageAuthToken_TerminalDTO) {
                var modalScope = <any>$scope.$new(true);
                modalScope.actionId = null;
                modalScope.mode = terminal.authenticationType;

                $modal.open({
                    animation: true,
                    templateUrl: '/AngularTemplate/InternalAuthentication',
                    controller: 'InternalAuthenticationController',
                    scope: modalScope
                })
                .result
                .then(() => _reloadTerminals());
            };

            var _reloadTerminals = function () {
                var actionIds = $scope.actionIds || [];

                $http.post(
                    urlPrefix + '/ManageAuthToken/TerminalsByActions',
                    actionIds
                )
                .then(function (res) {
                    var terminals: Array<model.ManageAuthToken_TerminalDTO> = [];
                    terminalActions = <any>res.data;

                    var i, j, wasAdded;
                    for (i = 0; i < terminalActions.length; ++i) {
                        wasAdded = false;

                        for (j = 0; j < terminals.length; ++j) {
                            if (terminals[j].id === terminalActions[i].terminal.id) {
                                wasAdded = true;
                                break;
                            }
                        }

                        if (!wasAdded) {
                            terminals.push(<model.ManageAuthToken_TerminalDTO>terminalActions[i].terminal);
                        }
                    }

                    $scope.terminals = terminals;
                });
            };

            _reloadTerminals();
        }
    }

    app.controller('AuthenticationDialogController', AuthenticationDialogController);
} 