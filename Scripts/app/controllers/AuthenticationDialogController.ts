module dockyard.controllers {

    export interface IAuthenticationDialogScope extends ng.IScope {
        activities: Array<model.ActivityDTO>;
        terminals: Array<model.AuthenticationTokenTerminalDTO>;

        isLoading: () => boolean;
        isAllSelected: () => boolean;
        linkAccount: (terminal: model.AuthenticationTokenTerminalDTO) => void;
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

            // var _terminalActions = [];
            var _activities: Array<model.ActivityDTO> = [];
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
                var terminalName;
                for (i = 0; i < _activities.length; ++i) {
                    terminalName = _activities[i].activityTemplate.terminal.name;
                    for (j = 0; j < $scope.terminals.length; ++j) {
                        if ($scope.terminals[j].name === terminalName) {
                            data.push({
                                actionId: _activities[i].id,
                                authTokenId: $scope.terminals[j].selectedAuthTokenId,
                                isMain: (<any>$scope.terminals[j]).isMain
                            });
                            break;
                        }
                    }
                }

                _loading = true;

                $http.post(urlPrefix + '/authentication/granttokens', data)
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

            var _authenticateInternal = (terminal: model.AuthenticationTokenTerminalDTO) => {
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
                                terminalName: data.terminalName,
                                authTokenId: data.authTokenId
                            });
                        }

                        _reloadTerminals(selectedAuthTokens);
                    });
            };

            var _authenticateExternal = function (terminal: model.AuthenticationTokenTerminalDTO) {
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
                            terminalName: event.data.terminalName,
                            authTokenId: event.data.authTokenId
                        });
                    }

                    _reloadTerminals(selectedAuthTokens);
                };
                
                $http
                    .get('/api/authentication/initial_url?terminal=' + terminal.name + '&version=' + terminal.version)
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

            var _combineTerminals = function (
                activities: Array<model.ActivityDTO>,
                allTerminals: Array<model.TerminalDTO>,
                authTokenTerminals: Array<model.AuthenticationTokenTerminalDTO>)
                    : Array<model.AuthenticationTokenTerminalDTO> {

                var result = authTokenTerminals.filter((it) => {
                    for (var i = 0; i < $scope.activities.length; ++i) {
                        if ($scope.activities[i].activityTemplate.terminal.name === it.name) {
                            return true;
                        }
                    }
                                
                    return false;
                });

                var hasTerminal = (
                    terminals: Array<any>,
                    name: string): boolean => {

                    for (var i = 0; i < terminals.length; ++i) {
                        if (terminals[i].name === name) {
                            return terminals[i];
                        }
                    }

                    return null;
                };

                for (var i = 0; i < activities.length; ++i) {
                    var terminal;
                    if ((terminal = hasTerminal(allTerminals, activities[i].activityTemplate.terminal.name))
                        && !hasTerminal(result, activities[i].activityTemplate.terminal.name)) {

                        var item = new model.AuthenticationTokenTerminalDTO(
                            terminal.name,
                            [],
                            terminal.authenticationType,
                            null
                        );
                        item.label = terminal.label;
                        item.version = terminal.version;

                        result.push(item);
                    }
                }

                result = result.sort((x, y) => x.name < y.name ? -1 : x.name > y.name ? 1 : 0);

                return result;
            };

            var _reloadTerminals = function (preselectedTokens?: Array<{ terminalName: string, authTokenId: number }>) {
                var activities = $scope.activities || [];
                _activities = activities;

                _loading = true;

                var selectedAuthTokens: Array<{ terminalName: string, authTokenId: number }> = [];

                activities.forEach((it) => {
                    if (it.authTokenId) {
                        selectedAuthTokens.push({
                            terminalName: it.activityTemplate.terminal.name,
                            authTokenId: <any>it.authTokenId
                        });
                    }
                });

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
                                terminalName: term.name,
                                authTokenId: term.selectedAuthTokenId
                            });
                        }
                    });
                }

                // Refresh terminals & auth-tokens list.
                $http.get(urlPrefix + '/terminals/all')
                    .then(function (res) {
                        var allTerminals = <Array<model.TerminalDTO>>res.data;

                        $http.get(urlPrefix + '/authentication/usertokens')
                            .then(function (res) {
                                var authTokenTerminals = <Array<model.AuthenticationTokenTerminalDTO>>res.data;
                                var terminals = _combineTerminals(activities, allTerminals, authTokenTerminals);
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
                                            if (selectedAuthTokens[i].terminalName == term.name) {
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
                    })
                    .catch(function () {
                        _loading = false;
                    });
            };

            _reloadTerminals();
        }
    }

    app.controller('AuthenticationDialogController', AuthenticationDialogController);
} 