angular.module('PlanDirectoryApp', ['ui.bootstrap'])
    .controller('SearchController', [
        '$scope',
        '$http',
        '$q',
        '$uibModal',
        'urlPrefix',
        '$location',
        '$window',
        function ($scope, $http, $q, $uibModal, urlPrefix, $location, $window) {
            $scope.searchForm = {
                searchText: ''
            };

            $scope.searched = false;
            $scope.pageSize = 5;
            $scope.totalCount = 0;
            $scope.planTemplates = [];
            $scope.currentPage = 1;
            $scope.pages = [];
            $scope.activityCategories = [];
            $scope.selectedCategories = [];

            var doSearch = function (pageStart) {
                var url = urlPrefix + '/api/v1/plan_templates/search'
                    + '?text=' + $scope.searchForm.searchText
                    + '&pageStart=' + pageStart
                    + '&pageSize=' + $scope.pageSize;

                Metronic.blockUI({ animate: true });

                var promise = $q(function (resolve, reject) {
                    $http.get(url)
                        .then(function (res) {
                            $scope.totalCount = res.data.totalCount;
                            $scope.planTemplates = res.data.planTemplates;

                            $scope.currentPage = pageStart;
                            $scope.pages = [];
                            for (var i = 0; i < Math.ceil(res.data.totalCount / $scope.pageSize) ; ++i) {
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

            var isPredefinedCategory = function (categoryId) {
                return [
                    '417DD061-27A1-4DEC-AECD-4F468013FD24',
                    '29EFB1D7-A9EA-41C5-AC60-AEF1F520E814',
                    '69FB6D2C-2083-4696-9457-B7B152D358C2',
                    'AFD7E981-A21A-4B05-B0B1-3115A5448F22',
                    'F9DF2AC2-2F10-4D21-B97A-987D46AD65B0'
                ].some(function (id) {
                    return id.toUpperCase() === categoryId.toUpperCase();
                });
            };

            var extractActivityCategories = function () {
                $http.get(urlPrefix + '/api/v1/activity_templates/')
                    .then(function (res) {
                        $scope.activityCategories = res.data.filter(function (it) { return !isPredefinedCategory(it.id); });
                    });
            };

            var isCategorySelected = function (ac) {
                return $scope.selectedCategories.some(function (it) { return it.id === ac.id; });
            };

            var addCategorySelection = function (ac) {
                $scope.selectedCategories.push(ac);
            };

            var removeCategorySelection = function (ac) {
                var i;
                for (i = 0; i < $scope.selectedCategories.length; ++i) {
                    if ($scope.selectedCategories[i].id === ac.id) {
                        $scope.selectedCategories.splice(i, 1);
                        break;
                    }
                }
            };

            var checkAuthentication = function () {
                var url = urlPrefix + '/api/v1/authentication/is_authenticated';

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

            var checkPrivileged = function () {
                var url = urlPrefix + '/api/v1/authentication/is_privileged';

                var promise = $q(function (resolve, reject) {
                    $http.get(url)
                        .then(function (res) {
                            resolve(res.data.privileged);
                        })
                        .catch(function (err) {
                            reject(err);
                        });
                });

                return promise;
            };

            $scope.submitSearch = function (pageStart) {
                doSearch(pageStart);
                $scope.searched = true;
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
                            var url = urlPrefix + '/api/v1/plan_templates/createplan/?id=' + planTemplate.parentPlanId;
                            $http.post(url, null)
                                .then(function (data) {
                                    Metronic.unblockUI();
                                    window.location.href = data.data.redirectUrl;
                                });
                        }
                    })
                    .finally(function () {
                        Metronic.unblockUI();
                    });
            };

            $scope.planDetails = function (planTemplate) {
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
                            var url = urlPrefix + '/api/v1/plan_templates/details_page?id=' + planTemplate.parentPlanId;
                            $http.get(url)
                                .then(function (res) {
                                    if (!res.data) {
                                        $uibModal.open({
                                            templateUrl: '/NoDetailsPageDialog.html',
                                            controller: 'NoDetailsPageDialogController'
                                        });
                                    }
                                    else {
                                        window.open(urlPrefix + '/' + res.data, '_blank');
                                    }
                                })
                                .finally(function () {
                                    Metronic.unblockUI();
                                });
                        }
                    })
                    .finally(function () {
                        Metronic.unblockUI();
                    });
            };

            $scope.removePlan = function (planTemplate) {
                Metronic.blockUI({ animate: true });

                var url = urlPrefix + '/api/v1/plan_templates/?id=' + planTemplate.parentPlanId;
                $http.delete(url)
                    .then(function (data) {
                        Metronic.unblockUI();
                        doSearch($scope.currentPage);
                    });
            };

            $scope.toggleCategorySelection = function (ac) {
                if (isCategorySelected(ac)) {
                    removeCategorySelection(ac);
                }
                else {
                    addCategorySelection(ac);
                }
            };

            $scope.isCategorySelected = function (ac) {
                return isCategorySelected(ac);
            };

            $scope.anyCategorySelected = function () {
                return $scope.selectedCategories.length;
            };

            $scope.showCategoryPage = function () {
                var categories = $scope.selectedCategories.map(function (it) { return it.name; });

                $http.post(urlPrefix + '/api/v1/page_definitions/category_page', categories)
                    .then(function (res) {
                        if (res.data) {
                            $window.location.href = res.data.url;
                        }
                        else {
                            $uibModal.open({
                                templateUrl: '/NoPageDefinitionDialog.html',
                                controller: 'NoPageDefinitionDialogController'
                            });
                        }
                    });
            };

            checkPrivileged()
                .then(function (privileged) {
                    $scope.privileged = privileged;
                });
            extractActivityCategories();

            if ($location.search().planSearch) {
                $scope.searchForm.searchText = $location.search().planSearch;
                $scope.searched = true;
            }

            doSearch(1);
        }
    ])
    .controller('AuthenticateDialogController', [
        '$scope',
        function ($scope) {
        }
    ])
    .controller('NoPageDefinitionDialogController', [
        '$scope',
        function ($scope) {
        }
    ])
    .controller('NoDetailsPageDialogController', [
        '$scope',
        function ($scope) {
        }
    ]);
