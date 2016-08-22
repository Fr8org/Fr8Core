module dockyard.controllers {

    export class SelectActionController {
        actionTypes: Array<model.ActivityTemplate> = []
        public static $inject = [
            '$scope',
            '$http'
        ];
        constructor(private $scope, private $http) {
            $scope.actionCategories = [];
            $http.get('/plan_nodes/available')
                .then(function (resp) {
                    $scope.actionCategories = resp.data;
                });

            $scope.actionTypeSelected = actionType => {
                $scope.$close(actionType);
            };

            $scope.cancel = () => {
                $scope.$dismiss();
            };
        }
    }

    app.controller('SelectActionController', SelectActionController);
}