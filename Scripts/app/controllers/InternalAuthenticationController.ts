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

            // 3 - AuthenticationMode.InternalModeWithDomain
            $scope.showDomain = $scope.mode == 3 ? 1 : 0;

            $scope.formData = {
                username: 'docusign_developer@dockyard.company',
                password: 'grolier34',
                domain: "dockyard.company"
            };

            $scope.isLoading = function () {
                return _loading;
            };

            $scope.submitForm = function () {
                if (!$scope.form.$valid) {
                    return;
                }

                var data = {
                    TerminalId: $scope.terminalId,
                    Username: $scope.formData.username,
                    Password: $scope.formData.password,
                    Domain: $scope.formData.domain
                };

                _loading = true;

                $http.post('/api/authentication/token', data)
                    .then(function (res: any) {

                        if (res.data.error) {
                            $scope.authErrorText = res.data.error;
                        }
                        else {
                            $scope.authErrorText = null;
                            $scope.$close();
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