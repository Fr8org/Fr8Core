angular.module('PlanDirectoryApp')
    .controller('SearchController', [
        '$scope',
        '$http',
        '$q',
        '$uibModal',
        'urlPrefix',
        function ($scope, $http, $q, $uibModal, urlPrefix) {
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

                            resolve();
                        })
                        .catch(function (err) {
                            reject(err);
                        })
                        .finally(function () {
                            Metronic.unblockUI();
                        });
                });

                return promise;
            };

            var checkAuthentication = function () {
                var url = urlPrefix + '/api/authentication/is_authenticated';

                var promise = $q(function (resolve, reject) {
                    $http.get(url)
                        .then(function (res) {
                            resolve(res.data.authenticated);
                        })
                        .catch(function (err) {
                            reject(err);
                        });
                });

                return promise;
            };

            $scope.submitSearch = function (pageStart) {
                doSearch(pageStart);
            };

            $scope.createPlan = function (planTemplate) {
                Metronic.blockUI({ animate: true });

                checkAuthentication()
                    .then(function (authenticated) {
                        if (!authenticated) {
                            $uibModal.open({
                                templateUrl: '/AuthenticateDialog.html',
                                controller: 'AuthenticateDialogController'
                            });

                            Metronic.unblockUI();
                        }
                        else {
                            var url = urlPrefix + '/api/plantemplates/createplan';
                            var data = { planTemplateId: planTemplate.PlanTemplateId };
                            $http.post(url, data)
                                .then(function () {
                                    $uibModal.open({
                                        templateUrl: '/PlanCreatedDialog.html',
                                        controller: 'PlanCreatedDialogController'
                                    });

                                    Metronic.unblockUI();
                                });
                        }
                    })
                    .finally(function () {
                        Metronic.unblockUI();
                    });
            };

            doSearch(1);
        }
    ])
    .controller('AuthenticateDialogController', [
        '$scope',
        function ($scope) {
        }
    ])
    .controller('PlanCreatedDialogController', [
        '$scope',
        function ($scope) {
        }
    ]);
