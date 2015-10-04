module dockyard.controllers {

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
            $scope.formData = {
                username: '64684b41-bdfd-4121-8f81-c825a6a03582',
                password: 'grolier34'
            };

            $scope.submitForm = function () {
                if (!$scope.form.$valid) {
                    return;
                }

                debugger;

                var data = {
                    ActivityTemplateId: $scope.activityTemplateId,
                    Username: $scope.formData.username,
                    Password: $scope.formData.password
                };

                $http.post('/actions/authenticate', data)
                    .then(function () {
                        $scope.$close();
                    })
                    .catch(function () {
                        $scope.authError = true;
                    });
            };

            // $scope.actionTypeSelected = function (actionType) {
            //     $scope.$close();
            // }
        }
    }

    app.controller('InternalAuthenticationController', InternalAuthenticationController);
}