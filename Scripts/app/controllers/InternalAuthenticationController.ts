module dockyard.controllers {

    export class InternalAuthenticationController {
        public static $inject = [
            '$scope',
            '$http'
        ];

        constructor(
            private $scope,
            private $http: ng.IHttpService) {

            var _loading = false;
            $scope.authError = false;
            $scope.authErrorText = null;
            $scope.showDomain = $scope.mode == 4 ? 1 : 0;   // 4 - AuthenticationMode.InternalModeWithDomain


            $scope.formData = {};
            // Checking for pre-defined account information
            $http.get('/api/authentication/demoAccountInfo', { params: { terminal: $scope.terminal.name } })
                .then(function (res: any) {
                    if (res.data && res.data.hasDemoAccount) {
                        $scope.formData = {
                            username: res.data.username,
                            password: res.data.password,
                            domain: res.data.domain
                        };
                    }
                });

            $scope.isLoading = function () {
                return _loading;
            };

            // At the moment, only Docusign provides demo service
            $scope.hasDemoService = function () {
                return $scope.terminalName == "terminalDocuSign";
            }

            $scope.submitForm = function () {
                if (!$scope.form.$valid) {
                    return;
                }

                _loading = true;

                var data = {
                    Terminal: $scope.terminal,
                    Username: $scope.formData.username,
                    Password: $scope.formData.password,
                    Domain: $scope.formData.domain,
                    IsDemoAccount: $scope.formData.isDemoAccount
                };

                $http.post('/api/authentication/token', data)
                    .then(function (res: any) {
                        if (res.data.error) {
                            $scope.authErrorText = res.data.error;
                        }
                        else {
                            $scope.authErrorText = null;
                            $scope.$close({
                                terminalId: res.data.terminalId,
                                terminalName: res.data.terminalName,
                                authTokenId: res.data.authTokenId
                            });
                        }
                    })
                    .catch(function () {
                        $scope.authError = true;
                    })
                    .finally(function () {
                        _loading = false;
                    });
            };
        }
    }

    app.controller('InternalAuthenticationController', InternalAuthenticationController);
}