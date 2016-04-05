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
            controller: ['$rootScope', '$scope', '$http', 'SubPlanService', 'ActionService', '$modal',
                function (
                    $rootScope: ng.IRootScopeService,
                    $scope: IActivityChooserScope,
                    $http: ng.IHttpService,
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
                    var createActivity = (activityTemplate: model.ActivityTemplate) => {
                        var activity = new model.ActivityDTO($scope.plan.id, $scope.field.subPlanId, null);
                        activity.activityTemplate = activityTemplate;

                        ActionService.save(activity).$promise
                            .then((activity: model.ActivityDTO) => {
                                $scope.field.activityTemplateLabel = activityTemplate.label || activityTemplate.name;

                                displayConfigureActivityModal($scope.plan, activity);
                            });
                    };

                    // Display dialog to select activity, and possibly create subplan and blank selected activity.
                    var selectActivity = () => {
                        var selectActivityModal = $modal.open({
                            templateUrl: '/AngularTemplate/ActivityChooserSelectDialog',
                            controller: 'ACSelectActivityController'
                        });

                        selectActivityModal.result
                            .then((activityTemplate: model.ActivityTemplate) => {
                                if (!$scope.field.subPlanId) {
                                    var subplan = new model.SubPlanDTO(null, true, $scope.plan.id, 'subplan-' + $scope.field.name);
                                    SubPlanService.create(subplan).$promise
                                        .then((subplan: model.SubPlanDTO) => {
                                            $scope.field.subPlanId = subplan.subPlanId;
                                            createActivity(activityTemplate);
                                        });
                                }
                                else {
                                    createActivity(activityTemplate);
                                }
                            });
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
