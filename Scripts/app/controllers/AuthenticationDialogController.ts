module dockyard.controllers {

    export interface IAuthenticationDialogScope extends ng.IScope {
        activities: Array<model.ActivityDTO>;
        terminals: Array<model.AuthenticationTokenTerminalDTO>;
        isWaitingForResponse: boolean;
        canBeAppliedToMultipleActivitieis: boolean;

        isLoading: () => boolean;
        isAllSelected: () => boolean;
        linkAccount: (terminal: model.AuthenticationTokenTerminalDTO) => void;
        apply: () => void;
        $close: (result: any) => void;
    }

    export class AuthenticationPopupBlockedDialogController {
        public static $inject = [
            '$scope',
            '$window'
        ];

        constructor(
            private $scope: ng.IScope,
            private $window: ng.IWindowService
        ) {
            (<any>$scope).hostname = $window.location.host || $window.location.hostname;
        }
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
            private $window,
            private $modal: any,
            private urlPrefix: string
        ) {

            // var _terminalActions = [];
            var _activities: Array<model.ActivityDTO> = [];
            var _loading = false;

            $scope.isWaitingForResponse = false;

            $scope.terminals = [];

            $scope.linkAccount = (terminal) => {
                $scope.isWaitingForResponse = true;
                if (terminal.authenticationType === 2 || terminal.authenticationType === 4) {
                    _authenticateInternal(terminal);
                }
                else if (terminal.authenticationType === 3) {
                    _authenticateExternal(terminal);
                }
                else if (terminal.authenticationType === 5) {
                    _authenticateWithPhoneNumber(terminal);
                }
            };

            $scope.apply = () => {
                if (!$scope.isAllSelected()) {
                    return;
                } else {
                    if ($window['analytics'] != null) {
                        $window['analytics'].track('Auth Dialog Ok');
                    }
                }

                var data = [];
                var authorizedActivities = [];
                var i, j;
                var terminalName;
                for (i = 0; i < _activities.length; ++i) {
                    var activity = _activities[i];
                    terminalName = activity.activityTemplate.terminalName;
                    for (j = 0; j < $scope.terminals.length; ++j) {
                        var terminal = $scope.terminals[j];
                        if (terminal.name === terminalName
                            && ((<any>terminal).useForAllActivities || (<any>activity).authorizeIsRequested)
                            && terminal.selectedAuthTokenId.toString() != activity.authTokenId) {
                            data.push({
                                actionId: activity.id,
                                authTokenId: terminal.selectedAuthTokenId,
                                isMain: (<any>terminal).isMain
                            });
                            authorizedActivities.push(activity.id);
                            break;
                        }
                    }
                }
                _loading = true;
                $http.post(urlPrefix + '/authentication/tokens/grant', data)
                    .then((res) => {
                        $scope.$close(authorizedActivities);
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
                    .then((data) => {
                        $scope.isWaitingForResponse = false;

                        var selectedAuthTokens = [];
                        if (typeof data != 'undefined') {
                            if (data.terminalId && data.authTokenId) {
                                selectedAuthTokens.push({
                                    terminalName: data.terminalName,
                                    authTokenId: data.authTokenId
                                });
                            }
                        }

                        _reloadTerminals(selectedAuthTokens);
                    }, () => { $scope.isWaitingForResponse = false; });
            };

            var _authenticateWithPhoneNumber = (terminal: model.AuthenticationTokenTerminalDTO) => {
                var modalScope = <any>$scope.$new(true);
                modalScope.terminal = terminal;
                modalScope.mode = terminal.authenticationType;
                modalScope.terminalName = terminal.name;

                $modal.open({
                    animation: true,
                    backdrop: 'static',
                    keyboard: false,
                    templateUrl: '/AngularTemplate/PhoneNumberAuthentication',
                    controller: 'PhoneNumberAuthenticationController',
                    scope: modalScope
                })
                    .result
                    .then((data) => {
                        $scope.isWaitingForResponse = false;

                        var selectedAuthTokens = [];
                        if (typeof data != 'undefined') {
                            if (data.terminalId && data.authTokenId) {
                                selectedAuthTokens.push({
                                    terminalName: data.terminalName,
                                    authTokenId: data.authTokenId
                                });
                            }
                        }

                        _reloadTerminals(selectedAuthTokens);
                    }, () => { $scope.isWaitingForResponse = false; });
            };

            var _authenticateExternal = function (terminal: model.AuthenticationTokenTerminalDTO) {
                var self = this;
                var childWindow;
                $scope.isWaitingForResponse = true;


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

                        //var _closeSplash = function () {
                        //    $scope.isWaitingForResponse = false;    
                        //    alert('!');
                        //    $scope.$apply();
                        //}

                        //if (typeof childWindow.attachEvent != "undefined") {
                        //    childWindow.attachEvent("onunload", _closeSplash);
                        //} else if (typeof childWindow.addEventListener != "undefined") {
                        //    childWindow.addEventListener("unload", _closeSplash, false);
                        //}


                        window.addEventListener('message', messageListener);

                        var isClosedHandler = function () {
                            if (childWindow && childWindow.closed) {
                                window.removeEventListener('message', messageListener);
                                $scope.isWaitingForResponse = false;
                                $scope.$apply();
                            }
                            else {
                                setTimeout(isClosedHandler, 500);
                            }
                        };
                        setTimeout(isClosedHandler, 500);

                        var isBlockedHandler = function () {
                            if (!childWindow || childWindow.outerHeight === 0) {
                                $scope.isWaitingForResponse = false;
                                $scope.$apply();

                                $modal.open({
                                    animation: true,
                                    backdrop: 'static',
                                    keyboard: false,
                                    templateUrl: '/AngularTemplate/AuthenticationPopupBlockedDialog',
                                    controller: 'AuthenticationPopupBlockedDialogController'
                                });
                            }
                        };
                        setTimeout(isBlockedHandler, 500);
                    });
            };

            var _combineTerminals = function (
                activities: Array<model.ActivityDTO>,
                allTerminals: Array<model.TerminalDTO>,
                authTokenTerminals: Array<model.AuthenticationTokenTerminalDTO>)
                : Array<model.AuthenticationTokenTerminalDTO> {

                var result = authTokenTerminals.filter((it) => {
                    for (var i = 0; i < $scope.activities.length; ++i) {
                        if ($scope.activities[i].activityTemplate.terminalName === it.name) {
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
                    if ((terminal = hasTerminal(allTerminals, activities[i].activityTemplate.terminalName))
                        && !hasTerminal(result, activities[i].activityTemplate.terminalName)) {

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
                //We check what activities are related to this terminal. If there are several activities and only part of them are manually requested authorization for
                //then we show advanced options and allow user to choose whether to apply auth token to all activities of the plan or only to the requested ones
                result.forEach(terminal => {
                    var terminalActivities = activities.filter(x => x.activityTemplate.terminalName === terminal.name);
                    var explicitlyRequestedActivities = terminalActivities
                        .filter(x => (<any>x).authorizeIsRequested === true);
                    (<any>terminal).showAdvancedOptions = explicitlyRequestedActivities.length < terminalActivities.length;
                    (<any>terminal).advancedOptionsAreExpanded = false;
                    (<any>terminal).useForAllActivities = true;
                });

                result = result.sort((x, y) => x.name < y.name ? -1 : x.name > y.name ? 1 : 0);

                return result;
            };

            var _reloadTerminals = function (preselectedTokens?: Array<{ terminalName: string, authTokenId: number, isMain: boolean }>) {
                // debugger; 
                var activities = $scope.activities || [];
                _activities = activities;

                _loading = true;

                var selectedAuthTokens: Array<{ terminalName: string, authTokenId: number, isMain: boolean }> = [];

                // Fill with preselected auth tokens.
                if (preselectedTokens) {
                    preselectedTokens.forEach(function (it) {
                        selectedAuthTokens.push(it);
                    });
                }

                // Save previously selected auth tokens.
                if ($scope.terminals) {
                    angular.forEach($scope.terminals, function (term) {
                        if (term.authTokens.length !== 0) {
                            selectedAuthTokens.push({
                                terminalName: term.name,
                                authTokenId: term.selectedAuthTokenId,
                                isMain: (<any>term).isMain
                            });
                        }
                    });
                }

                // Refresh terminals & auth-tokens list.
                $http.get(urlPrefix + '/terminals/all')
                    .then(function (res) {
                        var allTerminals = <Array<model.TerminalDTO>>res.data;

                        $http.get(urlPrefix + '/authentication/tokens')
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
                                            (<any>term).isMain = selectedAuthTokens[i].isMain
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
                                                (<any>term).isMain = selectedAuthTokens[i].isMain
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
    app.controller('AuthenticationPopupBlockedDialogController', AuthenticationPopupBlockedDialogController);
} 