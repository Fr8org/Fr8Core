module dockyard.controllers {

    export class PaneSelectActionController {
        actionTypes: Array<model.ActivityTemplate> = []
        public static $inject = [
            '$scope',
            '$http'
        ];
        constructor(private $scope, private $http) {
            $scope.actionCategories = [];
            $http.get('/activities/available')
                .then(function (resp) {
                    $scope.actionCategories = resp.data;
                });

            $scope.actionTypeSelected = function (actionType) {
                $scope.$close(actionType);
            }
        }
    }

    app.controller('PaneSelectActionController', PaneSelectActionController);
}