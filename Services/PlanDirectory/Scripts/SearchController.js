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

            $scope.pageSize = 5;
            $scope.totalCount = 0;
            $scope.planTemplates = [];
            $scope.currentPage = 1;
            $scope.pages = [];

            var doSearch = function (pageStart) {
                var url = urlPrefix + '/api/plantemplates/search'
                    + '?text=' + $scope.searchForm.searchText
                    + '&pageStart=' + pageStart
                    + '&pageSize=' + $scope.pageSize;

                Metronic.blockUI({ animate: true });

                var promise = $q(function (resolve, reject) {
                    $http.post(url)
                        .then(function (res) {
                            $scope.totalCount = res.data.TotalCount;
                            $scope.planTemplates = res.data.PlanTemplates;

                            $scope.currentPage = pageStart;
                            $scope.pages = [];
                            for (var i = 0; i < Math.ceil(res.data.TotalCount / $scope.pageSize) ; ++i) {
                                $scope.pages.push(i + 1);
                            }

                            Metronic.unblockUI();
                        });
                });

                return promise;
            };

            $scope.submitSearch = function (pageStart) {
                doSearch(pageStart);
            };

            doSearch(1);
        }
    ]);