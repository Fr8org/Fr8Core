/// <reference path="../../_all.ts" />

module dockyard.directives.designerHeader {
    'use strict';

    import designHeaderEvents = dockyard.Fr8Events.DesignerHeader;
    import pca = dockyard.directives.paneConfigureAction;

    export interface IDesignerHeaderScope extends ng.IScope {
        editing: boolean;
        editTitle(): void;
        onTitleChange(): void;
        runPlan(): void;
        deactivatePlan(): void;
        resetPlanStatus(): void;
        //sharePlan(): void;
        plan: model.PlanDTO;
        kioskMode: boolean;
        state: string;
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    export function DesignerHeader(): ng.IDirective {

        var controller = ['$rootScope', '$scope', '$element', '$attrs', '$http', 'ngToast', 'PlanService', 'ActivityTemplateHelperService', (
                $rootScope: interfaces.IAppRootScope,
                $scope: IDesignerHeaderScope,
                $element: ng.IAugmentedJQuery,
                $attrs: ng.IAttributes,
                $http: ng.IHttpService,
                ngToast: any,
                PlanService: services.IPlanService,
                ActivityTemplateHelperService: services.IActivityTemplateHelperService) => {

                $scope.$watch('plan.planState', function (newValue, oldValue) {
                    switch (newValue) {
                        case model.PlanState.Inactive:
                            // emit evet to control liner-progress bar
                            $rootScope.$broadcast(<any>designHeaderEvents.PLAN_EXECUTION_STOPPED);
                            break;
                        case model.PlanState.Executing:
                            // emit evet to control liner-progress bar
                            $rootScope.$broadcast(<any>designHeaderEvents.PLAN_EXECUTION_STARTED);
                            break;
                        case model.PlanState.Active:
                            break;
                        default:
                            // emit evet to control liner-progress bar
                            $rootScope.$broadcast(<any>designHeaderEvents.PLAN_EXECUTION_STOPPED);
                            break;
                    }
                });

                $scope.editTitle = () => {
                    if (!$scope.editing) {
                        $scope.editing = !$scope.editing;
                    }
                };

                $scope.onTitleChange = () => {
                    if ($scope.editing) {
                        $scope.editing = !$scope.editing;
                    }

                    var result = PlanService.update({ id: $scope.plan.id, name: $scope.plan.name, description: null });
                    result.$promise.then(() => { });
                };

                $scope.runPlan = () => {
                    // mark plan as Active                  
                    $scope.plan.planState = model.PlanState.Executing;                   
                    var promise = PlanService.runAndProcessClientAction($scope.plan.id);
                    
                    promise.then((container: model.ContainerDTO) => {
                        //if we have validation errors - reset plan state to Inactive. Plans with errors can't be activated   
                        if (container.currentPlanType === model.PlanType.OnGoing) {
                            $scope.plan.planState = model.PlanState.Active;
                        }
                        if (container.validationErrors && container.validationErrors != null) {
                            for (var key in container.validationErrors) {
                                if (container.validationErrors.hasOwnProperty(key)) {
                                    $scope.plan.planState = model.PlanState.Inactive;
                                    break;
                                }
                            }
                        }
                    });
                    promise.catch(error => {
                        $scope.deactivatePlan();
                        $rootScope.$broadcast(<any>designHeaderEvents.PLAN_EXECUTION_FAILED);
                    });
                    promise.finally(() => {
                        $scope.resetPlanStatus();

                        // This is to notify dashboad/view all page to reArrangePlans themselves so that plans get rendered in desired sections i.e Running or Plans Library
                        // This is required when user Run a plan and immediately navigates(before run completion) to dashboad or view all page in order 
                        // to make sure plans get rendered in desired sections
                        if (location.href.indexOf('/builder') === -1) {
                            $rootScope.$broadcast(<any>designHeaderEvents.PLAN_EXECUTION_COMPLETED_REARRANGE_PLANS, $scope.plan);
                            //$scope.$root.$broadcast("planExecutionCompleted", $scope.plan);
                        }
                    });
                };
                $scope.resetPlanStatus = () => {
                    var subPlan = $scope.plan.subPlans[0];
                    var initialActivity: interfaces.IActivityDTO = subPlan ? subPlan.activities[0] : null;
                    if (initialActivity == null) {
                        // mark plan as Inactive
                        $scope.plan.planState = model.PlanState.Inactive;
                        return;
                    }
                    var at = ActivityTemplateHelperService.getActivityTemplate(<model.ActivityDTO>initialActivity);
                    if (at.categories.some((value) => { return value.name.toLowerCase() === "solution"; })) {
                        initialActivity = initialActivity.childrenActivities[0];
                        if (initialActivity == null) {
                            // mark plan as Inactive
                            $scope.plan.planState = model.PlanState.Inactive;
                            return;
                        }
                    }

                    if (!at.categories.some((value) => { return (value.name.toLowerCase() === "triggers" || value.name.toLocaleLowerCase() === "solution");
                    })) {
                        // mark plan as Inactive
                        $scope.plan.planState = model.PlanState.Inactive;
                    }
                };

                $scope.$on(<any>designHeaderEvents.PLAN_IS_DEACTIVATED,
                    (event: ng.IAngularEvent, eventArgs: model.PlanDTO) => { $scope.plan.planState = model.PlanState.Inactive;});

                $scope.deactivatePlan = () => {
                    var result = PlanService.deactivate({ planId: $scope.plan.id });
                    result.$promise.then((data) => {                        

                        // mark plan as inactive
                        $scope.plan.planState = model.PlanState.Inactive;
                        var messageToShow = "Plan successfully deactivated";
                        ngToast.success(messageToShow);
                    })
                    .catch((err: any) => {
                        var messageToShow = "Failed to toggle Plan Status";
                        ngToast.danger(messageToShow);
                    });
                };
            }];
        return {
            restrict: 'E',
            scope: {
                editing: '=', 
                plan: '=',
                kioskMode: '=',
                state: '='
            },
            controller: controller,
            templateUrl: '/AngularTemplate/DesignerHeader'
        }
    }
    app.directive('designerHeader', DesignerHeader);
}