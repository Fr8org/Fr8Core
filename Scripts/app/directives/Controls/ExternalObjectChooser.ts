/// <reference path="../../_all.ts" />
module dockyard.directives {
    'use strict';

    interface IExternalObjectChooserControllerScope extends ng.IScope {
        field: model.ExternalObjectChooser;
        currentAction: model.ActivityDTO;
        plan: model.PlanDTO;

        configure: () => void;
    }

    export function ExternalObjectChooser(): ng.IDirective {
        var controller = [
            '$scope',
            '$http',
            'SubordinateSubplanService',
            'CrateHelper',
            function ($scope: IExternalObjectChooserControllerScope,
                $http: ng.IHttpService,
                SubordinateSubplanService: services.ISubordinateSubplanService,
                CrateHelper: services.CrateHelper) {

                $scope.configure = () => { 
                    $http.get('/api/activitytemplates/?id=' + $scope.field.activityTemplateId)
                        .then((res) => {
                            var activityTemplate = <model.ActivityTemplate>res.data;

                            $http.post('/api/activities/subordinate?id=' + $scope.currentAction.id, null)
                                .then((subordinateActivityResp: ng.IHttpPromiseCallbackArg<model.ActivityDTO>) => {

                                    SubordinateSubplanService
                                        .createSubplanAndConfigureActivity(
                                        $scope,
                                        $scope.field.name,
                                        $scope.plan,
                                        $scope.currentAction,
                                        subordinateActivityResp.data.parentPlanNodeId,
                                        activityTemplate)
                                        .then((subplanInfo: model.SubordinateSubplan) => {
                                            $scope.field.subPlanId = subplanInfo.subPlanId;

                                            $http.get('/api/activities?id=' + subplanInfo.activityId)
                                                .then((res) => {
                                                    var activity = <model.ActivityDTO>res.data;
                                                    var eoHandles = CrateHelper.findByManifestType(activity.crateStorage, 'External Object Handles', true);

                                                    var externalObjectName = null;
                                                    if (eoHandles && eoHandles.contents) {
                                                        var contents = <any>eoHandles.contents;
                                                        if (contents.Handles && contents.Handles.length > 0) {
                                                            externalObjectName = contents.Handles[0].Description;
                                                        }
                                                    }
                                                    $scope.field.externalObjectName = externalObjectName;
                                                });
                                        });

                                });
                        });
                };
            }
        ];

        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/ExternalObjectChooser',
            controller: controller,
            scope: {
                field: '=',
                currentAction: '=',
                plan: '='
            }
        };
    }

    app.directive('externalObjectChooser', ExternalObjectChooser);
}
