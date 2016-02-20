/// <reference path="../../_all.ts" />

module dockyard.directives {

    import pwd = dockyard.directives.paneWorkflowDesigner;

    'use strict';

    export function ManageRoute(): ng.IDirective {
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
        
        var copyRoute = function ($q, $http, planId, routeName): ng.IPromise<any> {
            var url = '/api/routes/copy?id=' + planId + '&name=' + routeName;

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
            controller: ['$scope', '$http', '$q', '$location', 
                function (
                    $scope: IManageRouteScope,
                    $http: ng.IHttpService,
                    $q: ng.IQService,
                    $location: ng.ILocationService
                ) {
                    $scope.saveRoute = function () {
                        if (!$scope.saveRouteName) {
                            return;
                        }

                        $scope.copySuccess = null;
                        $scope.error = null;

                        $scope.$emit(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_LongRunningOperation], new pwd.LongRunningOperationEventArgs(pwd.LongRunningOperationFlag.Started));

                        getRoute($q, $http, $scope.currentAction.id)
                            .then(function (route) {
                                copyRoute($q, $http, route.id, $scope.saveRouteName)
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
                                    $scope.$emit(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_LongRunningOperation], new pwd.LongRunningOperationEventArgs(pwd.LongRunningOperationFlag.Stopped));
                                    });
                            })
                            .catch(function (err) {
                                $scope.error = err;
                                $scope.$emit(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_LongRunningOperation], new pwd.LongRunningOperationEventArgs(pwd.LongRunningOperationFlag.Stopped));
                            });
                    };
                }
            ]
        }
    }

    export interface IManageRouteScope extends ng.IScope {
        currentAction: model.ActivityDTO;
        error: string;
        saveRouteName: string;
        copySuccess: any;
        saveRoute: () => void;
    }
}

app.directive('manageRoute', dockyard.directives.ManageRoute); 