/// <reference path="../../_all.ts" />
module dockyard.directives {
    'use strict';

    // --------------------------------------------------------------------------------
    // Template for SelectActivity modal.
    // --------------------------------------------------------------------------------
    const SelectActivityTemplate =
        '<div class="modal-header">\
            <h4 ng-if="!selectedWebService">Please, select web-service</h4>\
            <h4 ng-if="selectedWebService" style="cursor: pointer;">\
                <i class="fa fa-arrow-left" style="font-size: 16px; display: inline-block" ng-click="unselectWebService()"></i>\
                <span style="display: inline-block;" ng-click="unselectWebService()">Activities</span>\
            </h4>\
        </div>\
        <div class="modal-body">\
            <div style="overflow-y: auto; max-height: 500px">\
                <div ng-if="!selectedWebService">\
                    <div ng-repeat="webService in webServiceActivities" ng-click="selectWebService(webService)" style="margin: 0 0 5px 0; cursor: pointer;">\
                        <img ng-src="{{webService.webServiceIconPath}}" style="width: 48px; height: 48px;" />\
                        <span style="font-size: 1.3em">{{webService.webServiceName}}</span>\
                    </div>\
                </div>\
                <div ng-if="selectedWebService">\
                    <div ng-repeat="activity in selectedWebService.activities" ng-click="selectActivityTemplate(activity)" style="cursor: pointer;">\
                        <i class="fa fa-cogs" style="font-size: 24px; display: inline-block; vertical-align: middle;"></i>\
                        <span style="font-size: 1.3em; display: inline-block; margin: 20px 0 20px 0; vertical-align: middle;">{{activity.label}}</span>\
                    </div>\
                </div>\
            </div>\
        </div>';

    // --------------------------------------------------------------------------------
    // Template for ConfigureActivity modal.
    // --------------------------------------------------------------------------------
    const ConfigureActivityTemplate =
        '<div class="modal-header">\
            <h4>Please, configure activity {{activity.activityTemplate.label}}</h4>\
        </div>\
        <div class="modal-body">\
            <div style="overflow-y: auto; max-height: 500px">\
                <pane-configure-action plan="plan" current-action="activity"></pane-configure-action>\
            </div>\
        </div>\
        <div class="modal-footer">\
            <button class="btn btn-primary" ng-click="save()">Save</button>\
        </div>';

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
                            template: ConfigureActivityTemplate,
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
                            template: SelectActivityTemplate,
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
                            $http.post('/api/subplans/first_activity?id=' + $scope.field.subPlanId, null)
                                .then((res: ng.IHttpPromiseCallbackArg<model.ActivityDTO>) => {
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
            var origClose = $scope.$close;
            var origDismiss = $scope.$dismiss;

            $scope.save = () => {
                $scope.$close();
            };

            $scope.$close = (result?: any) => {
                debugger;
                origClose(result);
            };

            $scope.$dismiss = (reason: string) => {
                debugger;
                origDismiss(reason);
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
