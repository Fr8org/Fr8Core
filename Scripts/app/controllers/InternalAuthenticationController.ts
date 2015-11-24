﻿module dockyard.controllers {

    export class InternalAuthenticationController {
        public static $inject = [
            '$scope',
            '$http',
            'urlPrefix'
        ];

        constructor(
            private $scope,
            private $http: ng.IHttpService,
            private urlPrefix: string) {

            $scope.authError = false;
            $scope.authErrorText = null;
            $scope.showDomain = $scope.mode == 3 ? 1 : 0;

            $scope.formData = {
                username: 'docusign_developer@dockyard.company',
                password: 'grolier34',
                domain: "dockyard.company"
            };

            $scope.submitForm = function () {
                if (!$scope.form.$valid) {
                    return;
                }


                var data = {
                    ActivityTemplateId: $scope.activityTemplateId,
                    Username: $scope.formData.username,
                    Password: $scope.formData.password,
                    Domain: $scope.formData.domain
                };

                $http.post('/authentication/token', data)
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
                    });
            };
        }
    }

    app.controller('InternalAuthenticationController', InternalAuthenticationController);
}