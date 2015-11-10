/// <reference path="../../_all.ts" />

module dockyard.directives {
    'use strict';

    export function ManageRoute(): ng.IDirective {
        var getRouteId = function ($q, $http, actionId): ng.IPromise<any> {
            var url = '/routes/getByAction/' + actionId;

            return $q(function (resolve, reject) {
                $http.get(url)
                    .then(function (res) {
                        resolve(res.data.id);
                    })
                    .catch(function (err) {
                        reject(err);
                    });
            });
        };

        var runContainer = function ($q, $http, routeId): ng.IPromise<any> {
            var url = '/api/containers/launch?routeId=' + routeId;

            return $q(function (resolve, reject) {
                $http.post(url)
                    .then(function () {
                        resolve();
                    })
                    .catch(function (err) {
                        reject(err);
                    });
            });
        };

        var copyRoute = function ($q, $http, routeId, routeName): ng.IPromise<any> {
            var url = '/routes/copy?id=' + routeId + '&name=' + routeName;

            return $q(function (resolve, reject) {
                $http.post(url)
                    .then(function (res) {
                        resolve(res.data.id);
                    })
                    .catch(function (err) {
                        reject(err);
                    });
            });
        };

        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/ManageRoute',
            scope: {
                currentAction: '='
            },
            controller: ['$scope', '$http', '$q', 
                function (
                    $scope: IManageRouteScope,
                    $http: ng.IHttpService,
                    $q: ng.IQService
                ) {
                    var _isBusy = false;

                    $scope.isBusy = function () {
                        return _isBusy;
                    };

                    $scope.runNow = function () {
                        $scope.error = null;
                        _isBusy = true;

                        getRouteId($q, $http, $scope.currentAction.id)
                            .then(function (routeId) {
                                runContainer($q, $http, routeId)
                                    .catch(function (err) {
                                        $scope.error = err;
                                    })
                                    .finally(function () {
                                        _isBusy = false;
                                    });
                            })
                            .catch(function (err) {
                                $scope.error = err;
                                _isBusy = false;
                            });
                    };

                    $scope.saveRoute = function () {
                        if (!$scope.saveRouteName) {
                            return;
                        }

                        $scope.copySuccess = null;
                        $scope.error = null;
                        _isBusy = true;

                        getRouteId($q, $http, $scope.currentAction.id)
                            .then(function (routeId) {
                                copyRoute($q, $http, routeId, $scope.saveRouteName)
                                    .then(function (id) {
                                        $scope.copySuccess = {
                                            id: id,
                                            name: $scope.saveRouteName
                                        };

                                        $scope.saveRouteName = null;
                                    })
                                    .catch(function (err) {
                                        $scope.error = err;
                                    })
                                    .finally(function () {
                                        _isBusy = false;
                                    });
                            })
                            .catch(function (err) {
                                $scope.error = err;
                                _isBusy = true;
                            });
                    };
                }
            ]
        }
    }

    export interface IManageRouteScope extends ng.IScope {
        currentAction: model.ActionDTO;
        error: string;
        saveRouteName: string;
        copySuccess: any;
        runNow: () => void;
        saveRoute: () => void;
        isBusy: () => boolean;
    }
}

app.directive('manageRoute', dockyard.directives.ManageRoute); 