/// <reference path="../../_all.ts" />
module dockyard.directives {
    'use strict';

    import pwd = dockyard.directives.paneWorkflowDesigner;

    // --------------------------------------------------------------------------------
    // ActivityChooser directive.
    // Main directive for ActivityChooser control.
    // --------------------------------------------------------------------------------
    export function ActivityChooser(): ng.IDirective {
        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/ActivityChooser',
            scope: {
                field: '=',
                currentAction: '=',
                plan: '='
            },
            controller: ['$rootScope', '$scope', '$http', '$q', 'SubPlanService', 'ActionService', '$modal',
                function (
                    $rootScope: ng.IRootScopeService,
                    $scope: IActivityChooserScope,
                    $http: ng.IHttpService,
                    $q: ng.IQService,
                    SubPlanService: services.ISubPlanService,
                    ActionService: services.IActionService,
                    $modal: any
                ) {
                    // Display Modal with PaneConfigureAction control.
                    var displayConfigureActivityModal = (
                        plan: model.PlanDTO,
                        activity: model.ActivityDTO
                    ) => {
                        // We have to use PlanBuilder's scope in order to avoid parent activity reconfiguration.
                        // Search for PlanBuilder's scope up in the scope hierarchy.
                        var pbScope = <ng.IScope>$scope;
                        while (pbScope.$parent && pbScope !== $rootScope) {
                            pbScope = pbScope.$parent;
                            if ((<controllers.IPlanBuilderScope>pbScope).isPlanBuilderScope) {
                                break;
                            }
                        }

                        // Create new child scope derived from PlanBuilder's scope.
                        var scope = <IACConfigureActivityControllerScope>pbScope.$new(true);
                        scope.plan = plan;
                        scope.activity = activity;

                        var configureActivityModal = $modal.open({
                            templateUrl: '/AngularTemplate/ActivityChooserConfigureDialog',
                            controller: 'ACConfigureActivityController',
                            scope: scope
                        });

                        // We don't catch Save button click event, since configure flow saves changes automatically.
                        // scope.activity will contain modified version of configured activity.
                    };

                    // Call Hub API to create activity.
                    var createActivity = (activityTemplate: model.ActivityTemplate, subPlanId: string) => {
                        var defered = $q.defer();

                        var activity = new model.ActivityDTO($scope.plan.id, subPlanId, null);
                        activity.activityTemplate = activityTemplate;

                        ActionService.save(activity).$promise
                            .then((activity: model.ActivityDTO) => {
                                displayConfigureActivityModal($scope.plan, activity);
                                console.log('activity.id = ', activity.id);
                                defered.resolve();
                            })
                            .catch((reason: any) => {
                                defered.reject(reason);
                            });

                        return defered.promise;
                    };

                    // Call Hub API to create subplan.
                    var createSubPlan = function () {
                        var defered = $q.defer();

                        var subplan = new model.SubPlanDTO(
                            null,
                            true,
                            $scope.plan.id,
                            $scope.currentAction.id,
                            'subplan-' + $scope.field.name
                        );

                        subplan.runnable = $scope.field.runnable;

                        SubPlanService.create(subplan).$promise
                            .then((subplan: model.SubPlanDTO) => {
                                defered.resolve(subplan);
                            })
                            .catch((reason: any) => {
                                defered.reject(reason);
                            });

                        return defered.promise;
                    };

                    // Display dialog to select activity, and possibly create subplan and blank selected activity.
                    var selectActivity = () => {
                        debugger;
                        if (!$scope.field.definedActivityTemplateId) {
                            var scope = <IACSelectActivityControllerScope>$scope.$new(true);
                            scope.tag = $scope.field.activityTemplateTag;

                            var selectActivityModal = $modal.open({
                                templateUrl: '/AngularTemplate/ActivityChooserSelectDialog',
                                controller: 'ACSelectActivityController',
                                scope: scope
                            });

                            selectActivityModal.result
                                .then((activityTemplate: model.ActivityTemplate) => {
                                    if (!$scope.field.subPlanId) {
                                        createSubPlan()
                                            .then((subplan: model.SubPlanDTO) => {
                                                console.log('subplan.id = ', subplan.subPlanId);
                                                createActivity(activityTemplate, subplan.subPlanId)
                                                    .then(() => {
                                                        $scope.field.subPlanId = subplan.subPlanId;
                                                        $scope.field.activityTemplateLabel = activityTemplate.label || activityTemplate.name;
                                                        $scope.field.activityTemplateId = activityTemplate.id;
                                                    });
                                            });
                                    }
                                    // This should never happen, just in case.
                                    else {
                                        createActivity(activityTemplate, $scope.field.subPlanId)
                                            .then(() => {
                                                $scope.field.subPlanId = $scope.field.subPlanId;
                                                $scope.field.activityTemplateLabel = activityTemplate.label || activityTemplate.name;
                                                $scope.field.activityTemplateId = activityTemplate.id;
                                            });
                                    }
                                });
                        }
                        else {
                            $http.get('/api/webservices/?id=' + $scope.field.definedActivityTemplateId)
                                .then(function (res) {
                                    var activityTemplate = <model.ActivityTemplate>res.data;

                                    createSubPlan()
                                        .then((subplan: model.SubPlanDTO) => {
                                            console.log('subplan.id = ', subplan.subPlanId);
                                            createActivity(activityTemplate, subplan.subPlanId)
                                                .then(() => {
                                                    $scope.field.subPlanId = subplan.subPlanId;
                                                    $scope.field.activityTemplateLabel = activityTemplate.label || activityTemplate.name;
                                                    $scope.field.activityTemplateId = activityTemplate.id;
                                                });
                                        });
                                });
                        }
                    };

                    // "Select" button handler in scope of root ActivityChooser control.
                    $scope.selectActivity = () => {
                        if (!$scope.field.subPlanId) {
                            selectActivity();
                        }
                        else {
                            $scope.$emit(
                                pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_LongRunningOperation],
                                new pwd.LongRunningOperationEventArgs(pwd.LongRunningOperationFlag.Started)
                            );

                            $http.post('/api/subplans/first_activity?id=' + $scope.field.subPlanId, null)
                                .then((res: ng.IHttpPromiseCallbackArg<model.ActivityDTO>) => {
                                    $scope.$emit(
                                        pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_LongRunningOperation],
                                        new pwd.LongRunningOperationEventArgs(pwd.LongRunningOperationFlag.Stopped)
                                    );

                                    var activity = res.data;
                                    if (!activity) {
                                        selectActivity();
                                    }
                                    else {
                                        displayConfigureActivityModal($scope.plan, activity);
                                    }
                                });
                        }
                    };

                    $scope.remove = () => {
                        if (!$scope.field.subPlanId) {
                            return;
                        }

                        SubPlanService.remove({ id: $scope.field.subPlanId }).$promise
                            .then(() => {
                                $scope.field.subPlanId = null;
                                $scope.field.activityTemplateLabel = null;
                            });
                    };
                }
            ]
        }
    }


    // --------------------------------------------------------------------------------
    // IActivityChooserScope interface.
    // Scope for ActivityChooser directive.
    // --------------------------------------------------------------------------------
    interface IActivityChooserScope extends ng.IScope {
        plan: model.PlanDTO;
        currentAction: model.ActivityDTO;
        field: model.ActivityChooser;

        selectActivity: () => void;
        remove: () => void;
    }


    // --------------------------------------------------------------------------------
    // ActivityChooserSelectActivityController controller.
    // Controller for handling SelectActivity modal triggered by ActivityChooser directive.
    // --------------------------------------------------------------------------------
    export var ACSelectActivityController = [
        '$scope',
        '$http',
        function ($scope: IACSelectActivityControllerScope, $http: ng.IHttpService) {
            // Perform HTTP-request to extract activity-templates from Hub.
            var _reloadData = () => {
                $scope.webServiceActivities = [];

                $http.post('/api/webservices/activities', null)
                    .then((res) => {
                        var webServiceActivities = <Array<model.WebServiceActionSetDTO>>res.data;
                        angular.forEach(webServiceActivities, (webServiceActivity) => {
                            if ($scope.tag) {
                                webServiceActivity.activities = webServiceActivity.activities.filter((a) => {
                                    return a.tags && a.tags.toUpperCase().indexOf($scope.tag.toUpperCase()) >= 0;
                                });
                            }

                            if (!webServiceActivity.activities || !webServiceActivity.activities.length) {
                                return;
                            }

                            $scope.webServiceActivities.push(webServiceActivity);
                        });
                    });
            };

            // Perform web-service selection.
            $scope.selectWebService = (webService: model.WebServiceActionSetDTO) => {
                $scope.selectedWebService = webService;
            };

            // Perform activity selection.
            $scope.selectActivityTemplate = (activityTemplate: model.ActivityTemplate) => {
                $scope.$close(activityTemplate);
            };

            // Perform web-service unselect.
            $scope.unselectWebService = () => {
                $scope.selectedWebService = null;
            };

            // Force reload data.
            _reloadData();
        }
    ];


    // --------------------------------------------------------------------------------
    // IACSelectActivityControllerScope interface.
    // Scope for ACSelectActivityController controller.
    // --------------------------------------------------------------------------------
    interface IACSelectActivityControllerScope extends ng.IScope {
        tag: string;

        webServiceActivities: Array<model.WebServiceActionSetDTO>;
        selectedWebService: model.WebServiceActionSetDTO;
        selectedActivityTemplate: model.ActivityTemplate;

        selectWebService: (webService: model.WebServiceActionSetDTO) => void;
        selectActivityTemplate: (activity: model.ActivityTemplate) => void;
        unselectWebService: () => void;
        
        $close: (result?: any) => void;
    }


    // --------------------------------------------------------------------------------
    // ActivityConfigurationController controller.
    // Controller for handling ConfigureActivity modal triggered by ActivityChooser directive.
    // --------------------------------------------------------------------------------
    export var ACConfigureActivityController = [
        '$scope',
        function ($scope: IACConfigureActivityControllerScope) {
            $scope.save = () => {
                $scope.$close();
            };
        }
    ];


    // --------------------------------------------------------------------------------
    // ActivityConfigurationController controller.
    // Controller for handling ConfigureActivity modal triggered by ActivityChooser directive.
    // --------------------------------------------------------------------------------
    interface IACConfigureActivityControllerScope extends ng.IScope {
        plan: model.PlanDTO,
        activity: model.ActivityDTO,

        save: () => void;

        $close: (result?: any) => void;
        $dismiss: (reason: string) => void;
    }
}

// Register all components in Angular infrastructure.
app.directive('activityChooser', dockyard.directives.ActivityChooser);
app.controller('ACSelectActivityController', dockyard.directives.ACSelectActivityController);
app.controller('ACConfigureActivityController', dockyard.directives.ACConfigureActivityController);
