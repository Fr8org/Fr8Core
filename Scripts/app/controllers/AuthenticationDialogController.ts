module dockyard.controllers {

    export interface IAuthenticationDialogScope extends ng.IScope {
        actionIds: Array<number>;
        terminals: Array<model.ManageAuthToken_TerminalDTO>;

        isLoading: () => boolean;
        isAllSelected: () => boolean;
        linkAccount: (terminal: model.ManageAuthToken_TerminalDTO) => void;
        apply: () => void;
        $close: () => void;
    }

    export class AuthenticationDialogController {
        public static $inject = [
            '$scope',
            '$http',
            '$window',
            '$modal',
            'urlPrefix'
        ];

        constructor(
            private $scope: IAuthenticationDialogScope,
            private $http: ng.IHttpService,
            private $window: ng.IWindowService,
            private $modal: any,
            private urlPrefix: string
            ) {

            var _terminalActions = [];
            var _loading = false;

            $scope.terminals = [];

            $scope.linkAccount = (terminal) => {
                if (terminal.authenticationType === 2 || terminal.authenticationType === 4) {
                    _authenticateInternal(terminal);
                }
                else if (terminal.authenticationType === 3) {
                    _authenticateExternal(terminal);
                }
            };

            $scope.apply = () => {
                if (!$scope.isAllSelected()) {
                    return;
                }

                var data = [];

                var i, j;
                var terminalId;
                for (i = 0; i < _terminalActions.length; ++i) {
                    terminalId = _terminalActions[i].terminal.id;
                    for (j = 0; j < $scope.terminals.length; ++j) {
                        if ($scope.terminals[j].id === terminalId) {
                            data.push({
                                actionId: _terminalActions[i].actionId,
                                authTokenId: $scope.terminals[j].selectedAuthTokenId,
                                isMain: (<any>$scope.terminals[j]).isMain
                            });
                            break;
                        }
                    }
                }

                _loading = true;

                $http.post(urlPrefix + '/ManageAuthToken/apply', data)
                    .then((res) => {
                        $scope.$close();
                    })
                    .finally(() => {
                        _loading = false;
                    });
            };

            $scope.isLoading = function () {
                return _loading;
            };

            $scope.isAllSelected = function () {
                var i;
                for (i = 0; i < $scope.terminals.length; ++i) {
                    if (!(<any>$scope.terminals[i]).selectedAuthTokenId) {
                        return false;
                    }
                }

                return true;
            };

            var _authenticateInternal = (terminal: model.ManageAuthToken_TerminalDTO) => {
                var modalScope = <any>$scope.$new(true);
                modalScope.terminal = terminal;
                modalScope.mode = terminal.authenticationType;
                modalScope.terminalName = terminal.name;

                $modal.open({
                        animation: true,
                        templateUrl: '/AngularTemplate/InternalAuthentication',
                        controller: 'InternalAuthenticationController',
                        scope: modalScope
                    })
                    .result
                    .then(data => {
                        var selectedAuthTokens = [];
                        if (data.terminalId && data.authTokenId) {
                            selectedAuthTokens.push({
                                terminalId: data.terminalId,
                                authTokenId: data.authTokenId
                            });
                        }

                        _reloadTerminals(selectedAuthTokens);
                    });
            };

            var _authenticateExternal = function (terminal: model.ManageAuthToken_TerminalDTO) {
                var self = this;
                var childWindow;
                
                var messageListener = function (event) {
                    if (!event.data || event.data.type != 'external-auth-success') {
                        return;
                    }
                
                    childWindow.close();

                    var selectedAuthTokens = [];
                    if (event.data.terminalId && event.data.authTokenId) {
                        selectedAuthTokens.push({
                            terminalId: event.data.terminalId,
                            authTokenId: event.data.authTokenId
                        });
                    }

                    _reloadTerminals(selectedAuthTokens);
                };
                
                $http
                    .get('/api/authentication/initial_url?id=' + terminal.id)
                    .then(res => {
                        var url = (<any>res.data).url;
                        childWindow = $window.open(url, 'AuthWindow', 'width=400, height=500, location=no, status=no');
                        window.addEventListener('message', messageListener);
                
                        var isClosedHandler = function () {
                            if (childWindow.closed) {
                                window.removeEventListener('message', messageListener);
                            }
                            else {
                                setTimeout(isClosedHandler, 500);
                            }
                        };
                        setTimeout(isClosedHandler, 500);
                    });
            };

            var _reloadTerminals = function (preselectedTokens?) {
                var actionIds = $scope.actionIds || [];

                _loading = true;

                var selectedAuthTokens = [];

                // Fill with preselected auth tokens.
                if (preselectedTokens) {
                    preselectedTokens.forEach(function (it) {
                        selectedAuthTokens.push(it);
                    });
                }

                // Save previously selected auth tokens.
                if ($scope.terminals) {
                    angular.forEach($scope.terminals, function (term) {
                        if (term.selectedAuthTokenId) {
                            selectedAuthTokens.push({
                                terminalId: term.id,
                                authTokenId: term.selectedAuthTokenId
                            });
                        }
                    });
                }

                // Refresh terminals & auth-tokens list.
                $http.post(
                    urlPrefix + '/ManageAuthToken/AuthenticateTerminalsByActivities',
                    actionIds
                )
                .then(function (res) {
                    var terminals: Array<model.ManageAuthToken_TerminalDTO> = [];
                    _terminalActions = <any>res.data;

                    var i, j, wasAdded;
                    for (i = 0; i < _terminalActions.length; ++i) {
                        wasAdded = false;

                        for (j = 0; j < terminals.length; ++j) {
                            if (terminals[j].id === _terminalActions[i].terminal.id) {
                                wasAdded = true;
                                break;
                            }
                        }

                        if (!wasAdded) {
                            terminals.push(<model.ManageAuthToken_TerminalDTO>_terminalActions[i].terminal);
                        }
                    }

                    $scope.terminals = terminals;

                    // Explicitly selected current token (in case when user chooses another token for existing action).
                    angular.forEach(terminals, function (term) {
                        var i;
                        for (i = 0; i < term.authTokens.length; ++i) {
                            if (term.authTokens[i].isSelected) {
                                term.selectedAuthTokenId = term.authTokens[i].id;
                                break;
                            }
                        }
                    });

                    // Restore previously selected tokens.
                    angular.forEach(terminals, function (term) {
                        var i;

                        if (!term.selectedAuthTokenId) {
                            for (i = 0; i < selectedAuthTokens.length; ++i) {
                                if (selectedAuthTokens[i].terminalId == term.id) {
                                    term.selectedAuthTokenId = selectedAuthTokens[i].authTokenId;
                                    break;
                                }
                            }
                        }
                    });
                })
                .finally(function () {
                    _loading = false;
                });
            };

            _reloadTerminals();
        }
    }

    app.controller('AuthenticationDialogController', AuthenticationDialogController);
} 