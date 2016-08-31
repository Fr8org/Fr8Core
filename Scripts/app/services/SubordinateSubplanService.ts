/// <reference path="../_all.ts" />

module dockyard.services {

    import pwd = dockyard.directives.paneWorkflowDesigner;

    // --------------------------------------------------------------------------------
    // Interface for SubordinateSubplanService.
    // --------------------------------------------------------------------------------
    export interface ISubordinateSubplanService {
        // Display modal dialog to select activity template.
        selectActivityTemplate: (tag: string) => ng.IPromise<model.ActivityTemplate>;

        // Crate subplan, create activity based on activity template, display modal dialog to configure created activity.
        createSubplanAndConfigureActivity: (
            $scope: ng.IScope,
            subPlanName: string,
            parentPlan: model.PlanDTO,
            parentActivity: model.ActivityDTO,
            existingSubPlanId: string,
            activityTemplate: model.ActivityTemplate
        ) => ng.IPromise<model.SubordinateSubplan>;


    }


    // --------------------------------------------------------------------------------
    // Default implementation for ISubordinateSubplanService interface.
    // --------------------------------------------------------------------------------
    export class SubordinateSubplanService implements ISubordinateSubplanService {
        constructor(
            private $rootScope: ng.IRootScopeService,
            private $http: ng.IHttpService,
            private $q: ng.IQService,
            private $modal: any,
            private SubPlanService: services.ISubPlanService,
            private ActionService: services.IActionService,
            private ActivityTemplateHelperService: services.IActivityTemplateHelperService
        ) {
        }

        public selectActivityTemplate(tag: string)
            : ng.IPromise<model.ActivityTemplate> {

            var q = this.$q.defer<model.ActivityTemplate>();

            var scope = <ISelectActivityControllerScope>this.$rootScope.$new(true);
            scope.tag = tag;

            var selectActivityModal = this.$modal.open({
                templateUrl: '/AngularTemplate/SelectActivityDialog',
                controller: 'SelectActivityController',
                scope: scope
            });

            selectActivityModal.result
                .then((activityTemplate: model.ActivityTemplate) => {
                    q.resolve(activityTemplate);
                })
                .catch((reason: any) => {
                    q.reject(reason);
                });

            return q.promise;
        }

        public createSubplanAndConfigureActivity(
            $scope: ng.IScope,
            subPlanName: string,
            parentPlan: model.PlanDTO,
            parentActivity: model.ActivityDTO,
            existingSubPlanId: string,
            activityTemplate: model.ActivityTemplate): ng.IPromise<model.SubordinateSubplan> {

            // Call Hub API to create subplan.
            var createSubPlan = (plan: model.PlanDTO, activity: model.ActivityDTO,
                name: string): ng.IPromise<model.SubPlanDTO> => {

                var defered = this.$q.defer<model.SubPlanDTO>();

                var subplan = new model.SubPlanDTO(
                    null,
                    true,
                    plan.id,
                    activity.id,
                    'subplan-' + name
                );
                
                this.SubPlanService.create(subplan).$promise
                    .then((subplan: model.SubPlanDTO) => {
                        defered.resolve(subplan);
                    })
                    .catch((reason: any) => {
                        defered.reject(reason);
                    });

                return defered.promise;
            };

            // Call Hub API to create activity.
            var createActivity = (activityTemplate: model.ActivityTemplate, plan: model.PlanDTO, subPlanId: string): ng.IPromise<model.ActivityDTO> => {
                var defered = this.$q.defer<model.ActivityDTO>();

                var activity = new model.ActivityDTO(plan.id, subPlanId, null);
                activity.activityTemplate = this.ActivityTemplateHelperService.toSummary(<interfaces.IActivityTemplateVM>activityTemplate);
                this.ActionService.save(activity).$promise
                    .then((activity: model.ActivityDTO) => {
                        displayConfigureActivityModal(plan, activity)
                            .finally(() => {
                                defered.resolve(activity);
                            });
                    })
                    .catch((reason: any) => {
                        defered.reject(reason);
                    });

                return defered.promise;
            };

            // Display Modal with PaneConfigureAction control.
            var displayConfigureActivityModal = (
                plan: model.PlanDTO,
                activity: model.ActivityDTO
            ): ng.IPromise<any> => {
                // We have to use PlanBuilder's scope in order to avoid parent activity reconfiguration.
                // Search for PlanBuilder's scope up in the scope hierarchy.
                var pbScope = <ng.IScope>$scope;
                while (pbScope.$parent && pbScope !== this.$rootScope) {
                    pbScope = pbScope.$parent;
                    if ((<controllers.IPlanBuilderScope>pbScope).isPlanBuilderScope) {
                        break;
                    }
                }

                // Stop plan-builder animation.
                $scope.$emit(
                    pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_LongRunningOperation],
                    new pwd.LongRunningOperationEventArgs(pwd.LongRunningOperationFlag.Stopped)
                );

                // Create new child scope derived from PlanBuilder's scope.
                var scope = <IConfigureActivityControllerScope>pbScope.$new(true);
                scope.plan = plan;
                scope.activity = activity;
                scope.view = null;
                scope.isReConfiguring = false;
                scope.mode = 'plan';

                var configureActivityModal = this.$modal.open({
                    templateUrl: '/AngularTemplate/ConfigureActivityDialog',
                    controller: 'ConfigureActivityController',
                    scope: scope
                });

                return <ng.IPromise<any>>configureActivityModal.result;
            };
            
            // Method logic.
            var result = this.$q.defer<model.SubordinateSubplan>();

            // Start plan-builder animation.
            $scope.$emit(
                pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_LongRunningOperation],
                new pwd.LongRunningOperationEventArgs(pwd.LongRunningOperationFlag.Started)
            );

            if (!existingSubPlanId) {
                createSubPlan(parentPlan, parentActivity, subPlanName)
                    .then((subplan: model.SubPlanDTO) => {
                        createActivity(activityTemplate, parentPlan, subplan.id)
                            .then((activity: model.ActivityDTO) => {
                                result.resolve(new model.SubordinateSubplan(subplan.id, activity.id));
                            })
                            .catch((reason: any) => {
                                result.reject(reason);
                            });
                    })
                    .catch((reason: any) => {
                        result.reject(reason);
                    });
            }
            else {
                this.$http.post('/api/subplans/activities?id=' + existingSubPlanId + "&filter=first", null)
                    .then((res: ng.IHttpPromiseCallbackArg<model.ActivityDTO>) => {
                        var activity = res.data;

                        // activity template was changed, need to create new activity
                        if (activity.activityTemplate.name !== activityTemplate.name ||
                            activity.activityTemplate.version !== activityTemplate.version ||
                            activity.activityTemplate.terminalName !== activityTemplate.terminal.name ||
                            activity.activityTemplate.terminalVersion !== activityTemplate.terminal.version) {

                            this.ActionService.deleteById({ id: activity.id })
                                .$promise.then(() => {
                                    createActivity(activityTemplate, parentPlan, existingSubPlanId)
                                        .then((activity: model.ActivityDTO) => {
                                            result.resolve(new model
                                                .SubordinateSubplan(existingSubPlanId, activity.id));
                                        })
                                        .catch((reason: any) => {
                                            result.reject(reason);
                                        });
                                }).catch((reason: any) => {
                                    result.reject(reason);
                                });
                        } else {
                            displayConfigureActivityModal(parentPlan, activity)
                                .then(() => {
                                    result.resolve(new model.SubordinateSubplan(existingSubPlanId, activity.id));
                                });    
                        }
                    });
            }

            return result.promise;
        }
    }


    // --------------------------------------------------------------------------------
    // SelectActivityController controller.
    // Controller for handling SelectActivity modal.
    // --------------------------------------------------------------------------------
    export var SelectActivityController = ['$scope','$http','ActivityTemplateHelperService',
        ($scope: ISelectActivityControllerScope, $http: ng.IHttpService, activityTemplateHelperService: services.IActivityTemplateHelperService) => {
            var reloadData = () => {
                $scope.webServiceActivities = [];
                var activityTemplateCategories = activityTemplateHelperService.getAvailableActivityTemplatesInCategories();
                console.log(activityTemplateCategories);
                angular.forEach(activityTemplateCategories, (webServiceActivity) => {
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
            };

            // Perform web-service selection.
            $scope.selectWebService = (webService: interfaces.IActivityCategoryDTO) => {
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
            reloadData();
        }
    ];


    // --------------------------------------------------------------------------------
    // ISelectActivityControllerScope interface.
    // Scope for SelectActivityController controller.
    // --------------------------------------------------------------------------------
    interface ISelectActivityControllerScope extends ng.IScope {
        tag: string;

        webServiceActivities: Array<interfaces.IActivityCategoryDTO>;
        selectedWebService: interfaces.IActivityCategoryDTO;
        selectedActivityTemplate: model.ActivityTemplate;
        selectWebService: (webService: interfaces.IActivityCategoryDTO) => void;
        selectActivityTemplate: (activity: model.ActivityTemplate) => void;
        unselectWebService: () => void;
        $close: (result?: any) => void;
    }


    // --------------------------------------------------------------------------------
    // ActivityConfigurationController controller.
    // Controller for handling ConfigureActivity modal triggered by ActivityChooser directive.
    // --------------------------------------------------------------------------------
    export var ConfigureActivityController = [
        '$scope',
        function ($scope: IConfigureActivityControllerScope) {
            $scope.save = () => {
                $scope.$close();
            };
        }
    ];


    // --------------------------------------------------------------------------------
    // ActivityConfigurationController controller.
    // Controller for handling ConfigureActivity modal triggered by ActivityChooser directive.
    // --------------------------------------------------------------------------------
    interface IConfigureActivityControllerScope extends ng.IScope {
        plan: model.PlanDTO,
        activity: model.ActivityDTO,
        view: string;
        isReConfiguring: boolean;
        mode: string;

        save: () => void;

        $close: (result?: any) => void;
        $dismiss: (reason: string) => void;
    }
}

app.service('SubordinateSubplanService',
    [
        '$rootScope',
        '$http',
        '$q',
        '$modal',
        'SubPlanService',
        'ActionService',
        'ActivityTemplateHelperService',
        dockyard.services.SubordinateSubplanService
    ]
);

app.controller('SelectActivityController', dockyard.services.SelectActivityController);
app.controller('ConfigureActivityController', dockyard.services.ConfigureActivityController);
