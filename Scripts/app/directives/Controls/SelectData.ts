/// <reference path="../../_all.ts" />
module dockyard.directives {
    'use strict';


    interface ISelectDataControllerScope extends ng.IScope {
        field: model.SelectData;
        currentAction: model.ActivityDTO;
        plan: model.PlanDTO;

        select: () => void;
        configure: () => void;
    }


    export function SelectData(): ng.IDirective {
        var controller = [
            '$scope',
            '$http',
            'SubordinateSubplanService',
            'CrateHelper',
            '$window',
            function ($scope: ISelectDataControllerScope,
                $http: ng.IHttpService,
                SubordinateSubplanService: services.SubordinateSubplanService,
                CrateHelper: services.CrateHelper,
                $window: any) {

                $scope.select = () => {
                    SubordinateSubplanService
                        .selectActivityTemplate('Getter')
                        .then((activityTemplate: model.ActivityTemplate) => {
                            $scope.field.activityTemplateName = activityTemplate.name;
                            $scope.field.activityTemplateId = activityTemplate.id;
                        });
                };

                var isConfiguring = false;
                $scope.configure = () => {
                    if (isConfiguring) {
                        return;
                    }
                    isConfiguring = true;

                    $http.get('/api/activitytemplates/?id=' + $scope.field.activityTemplateId)
                        .then((res) => {
                            var activityTemplate = <model.ActivityTemplate>res.data;

                            SubordinateSubplanService
                                .createSubplanAndConfigureActivity(
                                $scope,
                                $scope.field.name,
                                $scope.plan,
                                $scope.currentAction,
                                $scope.field.subPlanId,
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
                        }).finally(() => { isConfiguring = false; });
                };
            }
        ];

        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/SelectData',
            controller: controller,
            scope: {
                field: '=',
                currentAction: '=',
                plan: '='
            }
        };
    }

    app.directive('selectData', SelectData);
}