/// <reference path="../../_all.ts" />

module dockyard.directives {
    import pwd = dockyard.directives.paneWorkflowDesigner;
    'use strict';

    export interface IRunPlanButtonScope extends ng.IScope {
        currentAction: model.ActivityDTO;
        error: string;
        runNow: () => void;
    }

    export function RunPlanButton(): ng.IDirective {
        var runContainer = function ($q, $http, planId): ng.IPromise<any> {
            var url = '/api/plans/run?planId=' + planId;

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

        var getPlan = function ($q, $http, actionId): ng.IPromise<any> {
            var url = '/api/plans?activity_id=' + actionId;

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

        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/RunPlanButton',
            scope: {
                currentAction: '=',
            },
            controller: ['$scope', '$http', '$q', '$location',
                function (
                    $scope: IRunPlanButtonScope,
                    $http: ng.IHttpService,
                    $q: ng.IQService,
                    $location: ng.ILocationService
                ) {

                    $scope.runNow = function () {
                        $scope.error = null;

                        $scope.$emit(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_LongRunningOperation], new pwd.LongRunningOperationEventArgs(pwd.LongRunningOperationFlag.Started));

                        getPlan($q, $http, $scope.currentAction.id)
                            .then(function (plan) {
                                runContainer($q, $http, plan.plan.id)
                                    .then(function (container) {
                                        var path = '/findObjects/' + container.id + '/results';
                                        $location.path(path);
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
}

app.directive('runPlanButton', dockyard.directives.RunPlanButton); 