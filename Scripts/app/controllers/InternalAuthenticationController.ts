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
            $scope.mode = $scope.mode;

            // 4 - AuthenticationMode.InternalModeWithDomain
            $scope.showDomain = $scope.mode == 4 ? 1 : 0;

            $scope.formData = {
                username: 'docusign_developer@dockyard.company',
                password: 'grolier34',
                domain: "dockyard.company",
                isDemoAccount: false
            };

            $scope.isLoading = function () {
                return _loading;
            };

            $scope.hasDemoService = function () {
                return $scope.terminalName == "terminalDocuSign";
            }

            $scope.submitForm = function () {
                if (!$scope.form.$valid) {
                    return;
                }
                var data = {
                    Terminal: $scope.terminal,
                    Username: $scope.formData.username,
                    Password: $scope.formData.password,
                    Domain: $scope.formData.domain,
                    IsDemoAccount: $scope.formData.isDemoAccount
                };

                _loading = true;

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