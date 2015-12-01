/// <reference path="../../_all.ts" />

module dockyard.directives {
    'use strict';

    export function RunRoutePane($compile: ng.ICompileService): ng.IDirective {
        var runContainer = function ($q, $http, routeId): ng.IPromise<any> {
            var url = '/api/routes/run?routeId=' + routeId;

            return $q(function (resolve, reject) {
                $http.post(url)
                    .then(function (res) {
                        resolve(res.data);
                    })
                    .catch(function (err) {
                        reject(err);
                    });
            });
        };

        var getRoute = function ($q, $http, actionId): ng.IPromise<any> {
            var url = '/api/routes/getByAction/' + actionId;

            return $q(function (resolve, reject) {
                $http.get(url)
                    .then(function (res) {
                        resolve(res.data);
                    })
                    .catch(function (err) {
                        reject(err);
                    });
            });
        };

        var link = (scope: IRunRoutePaneScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => {
            if (angular.isArray(scope.field.children) || scope.field.children.length > 0) {
                $compile('<div ng-repeat="control in field.children"><configuration-control current-action="currentAction" field="control" /></div>')(scope, function (cloned, scope) {
                    element.find('#content').replaceWith(cloned);
                });
            }
        };

        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/RunRoutePane',
            link: link,
            scope: {
                currentAction: '=',
                field: '=',
            },
            controller: ['$scope', '$http', '$q', '$location', 
                function (
                    $scope: IRunRoutePaneScope,
                    $http: ng.IHttpService,
                    $q: ng.IQService,
                    $location: ng.ILocationService
                ) {
                    var _isBusy = false;

                    $scope.isBusy = function () {
                        return _isBusy;
                    };

                    $scope.runNow = function () {
                        $scope.error = null;
                        _isBusy = true;

                        getRoute($q, $http, $scope.currentAction.id)
                            .then(function (route) {
                                runContainer($q, $http, route.id)
                                    .then(function (container) {
                                        var path = '/findObjects/' + container.id + '/results';
                                        $location.path(path);
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
                                _isBusy = false;
                            });
                    };
                }
            ]
        }
    }

    export interface IRunRoutePaneScope extends ng.IScope {
        currentAction: model.ActionDTO;
        field: model.RunRoutePane;
        error: string;
        runNow: () => void;
        isBusy: () => boolean;
    }
}

app.directive('runRoutePane', dockyard.directives.RunRoutePane); 