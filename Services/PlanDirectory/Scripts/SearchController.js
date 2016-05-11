angular.module('PlanDirectoryApp')
    .controller('SearchController', [
        '$scope',
        '$http',
        'urlPrefix',
        function ($scope, $http) {
            $scope.searchForm = {
                searchText: null
            };

            $scope.submitSearch = function () {
                $http.post(urlPrefix + '/api/')

                return false;
            };
        }
    ]);