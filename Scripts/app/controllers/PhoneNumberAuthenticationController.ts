module dockyard.controllers {

    export class PhoneNumberAuthenticationController {
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

            $scope.inVerifyMode = false;

            $scope.formData = {
                clientId: '',
                phoneNumber: '',
                verificationCode: '',
                clientName: '',
                message: '',
            };

            $scope.isLoading = function () {
                return _loading;
            };

            $scope.sendCodeToPhone = function () {
                //if (!$scope.form.$valid) {
                //    return;
                //}
                var data = {
                    Terminal: $scope.terminal,
                    PhoneNumber : $scope.formData.phoneNumber
                };

                _loading = true;

                $http.post('/api/authentication/AuthenticatePhoneNumber', data)
                    .then(function (res: any) {

                        if (res.data.error) {
                            $scope.authErrorText = res.data.error;
                        }
                        else {
                            $scope.authError = false;
                            $scope.authErrorText = null;
                            $scope.formData.clientId = res.data.clientId;
                            $scope.formData.clientName = res.data.clientName;
                            $scope.formData.title = res.data.title;
                            $scope.formData.message = res.data.message;
                            $scope.inVerifyMode = true;
                        }
                    })
                    .catch(function () {
                        $scope.authError = true;
                        $scope.formData.message = '';
                    })
                    .finally(function () {
                        _loading = false;
                    });
            };


            $scope.verifyCodeAndAuthenticate = function () {
                //if (!$scope.form.$valid) {
                //    return;
                //}
                var data = {
                    Terminal: $scope.terminal,
                    PhoneNumber: $scope.formData.phoneNumber,
                    ClientId: $scope.formData.clientId,
                    ClientName: $scope.formData.clientName,
                    VerificationCode : $scope.formData.verificationCode
                };

                _loading = true;

                $http.post('/api/authentication/VerifyPhoneNumberCode', data)
                    .then(function (res: any) {

                        $scope.authError = "";
                        $scope.formData.message = '';

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
                            $scope.inVerifyMode = false;
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

    app.controller('PhoneNumberAuthenticationController', PhoneNumberAuthenticationController);
}