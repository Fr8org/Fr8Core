angular.module('PlanDirectoryApp')
    .controller('SearchController', [
        '$scope',
        '$http',
        '$q',
        'urlPrefix',
        function ($scope, $http, $q, urlPrefix) {
            $scope.searchForm = {
                searchText: ''
            };

            $scope.pageSize = 4;
            $scope.totalCount = 0;
            $scope.planTemplates = [];

            var doSearch = function (pageStart) {
                var url = urlPrefix + '/api/plantemplates/search'
                    + '?text=' + $scope.searchForm.searchText
                    + '&pageStart=' + pageStart
                    + '&pageSize=' + $scope.pageSize;

                var promise = $q(function (resolve, reject) {
                    $http.post(url)
                        .then(function (res) {
                            $scope.totalCount = res.data.TotalCount;
                            $scope.planTemplates = res.data.PlanTemplates;
                        });
                });

                return promise;
            };

            $scope.submitSearch = function () {
                doSearch(1);
            };

            doSearch(1);
        }
    ]);