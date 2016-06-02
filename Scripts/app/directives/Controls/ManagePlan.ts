/// <reference path="../../_all.ts" />

module dockyard.directives {

    import pwd = dockyard.directives.paneWorkflowDesigner;

    'use strict';

    export function ManagePlan(): ng.IDirective {
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

        //var copyPlan = function ($q, $http, planId, planName): ng.IPromise<any> {
        //    var url = '/api/plans/copy?id=' + planId + '&name=' + planName;

        //    return $q(function (resolve, reject) {
        //        $http.post(url)
        //            .then(function (res) {
        //                resolve(res.data.id);
        //            })
        //            .catch(function (err) {
        //                reject(err);
        //            });
        //    });
        //};

        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/ManagePlan',
            scope: {
                currentAction: '='
            },
            controller: ['$scope', '$http', '$q', '$location',
                function (
                    $scope: IManagePlanScope,
                    $http: ng.IHttpService,
                    $q: ng.IQService,
                    $location: ng.ILocationService
                ) {
                    $scope.savePlan = function () {
                        if (!$scope.savePlanName) {
                            return;
                        }

                        $scope.copySuccess = null;
                        $scope.error = null;

                        $scope.$emit(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_LongRunningOperation], new pwd.LongRunningOperationEventArgs(pwd.LongRunningOperationFlag.Started));

                        getPlan($q, $http, $scope.currentAction.id)
                            .then(function (plan) {
                                // seems it was never worked
                                //copyPlan($q, $http, plan.plan.id, $scope.savePlanName)
                                //    .then(function (id) {
                                //        $scope.copySuccess = {
                                //            id: id,
                                //            name: $scope.savePlanName
                                //        };

                                //        $scope.savePlanName = null;
                                //    })
                                //    .catch(function (err) {
                                //        $scope.error = err;
                                //    })
                                //    .finally(function () {
                                //        $scope.$emit(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_LongRunningOperation], new pwd.LongRunningOperationEventArgs(pwd.LongRunningOperationFlag.Stopped));
                                //    });
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

    export interface IManagePlanScope extends ng.IScope {
        currentAction: model.ActivityDTO;
        error: string;
        savePlanName: string;
        copySuccess: any;
        savePlan: () => void;
    }
}

app.directive('managePlan', dockyard.directives.ManagePlan); 