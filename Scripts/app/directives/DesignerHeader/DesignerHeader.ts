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
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    class DesignerHeader implements ng.IDirective {
        public link: (scope: IDesignerHeaderScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public controller: (
            $rootScope: interfaces.IAppRootScope,
            $scope: IDesignerHeaderScope,
            element: ng.IAugmentedJQuery,
            attrs: ng.IAttributes,
            $http: ng.IHttpService,
            ngToast: any,
            PlanService: services.IPlanService
        ) => void;

        public templateUrl = '/AngularTemplate/DesignerHeader';
        public scope = {
            plan: '=',
            kioskMode: '=?'
        };
        public restrict = 'E';

        constructor(private Planervice: services.IPlanService) {
            DesignerHeader.prototype.link = (
                scope: IDesignerHeaderScope,
                element: ng.IAugmentedJQuery,
                attrs: ng.IAttributes) => {

                //Link function goes here
            };

            DesignerHeader.prototype.controller = (
                $rootScope: interfaces.IAppRootScope,
                $scope: IDesignerHeaderScope,
                $element: ng.IAugmentedJQuery,
                $attrs: ng.IAttributes,
                $http: ng.IHttpService,
                ngToast: any,
                PlanService: services.IPlanService) => {

                $scope.$watch('plan.planState', function (newValue, oldValue) {
                    switch (newValue) {
                        case 1:
                            // emit evet to control liner-progress bar
                            $rootScope.$broadcast(<any>designHeaderEvents.PLAN_EXECUTION_STOPPED);
                            break;
                        case 2:
                            // emit evet to control liner-progress bar
                            $rootScope.$broadcast(<any>designHeaderEvents.PLAN_EXECUTION_STARTED);
                            break;
                        default:
                            // emit evet to control liner-progress bar
                            $rootScope.$broadcast(<any>designHeaderEvents.PLAN_EXECUTION_STOPPED);
                            break;
                    }
                });

                $scope.editTitle = () => {
                    $scope.editing = true;
                };

                $scope.onTitleChange = () => {
                    $scope.editing = false;
                    var result = PlanService.update({ id: $scope.plan.id, name: $scope.plan.name, description: null });
                    result.$promise.then(() => { });
                };

                $scope.runPlan = () => {
                    // mark plan as Active
                    $scope.plan.planState = 2;                   
                    var promise = PlanService.runAndProcessClientAction($scope.plan.id);
                    
                    promise.then((container: model.ContainerDTO) => {
                        //if we have validation errors - reset plan state to Inactive. Plans with errors can't be activated    
                        if (container.validationErrors && container.validationErrors != null) {
                            for (var key in container.validationErrors) {
                                if (container.validationErrors.hasOwnProperty(key)) {
                                    $scope.plan.planState = 1;
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
                        $scope.plan.planState = 1;
                        return;
                    }

                    if (initialActivity.activityTemplate.category.toLowerCase() === "solution") {
                        initialActivity = initialActivity.childrenActivities[0];
                        if (initialActivity == null) {
                            // mark plan as Inactive
                            $scope.plan.planState = 1;
                            return;
                        }
                    }

                    if (initialActivity.activityTemplate.category.toLowerCase() !== "monitors") {
                        // mark plan as Inactive
                        $scope.plan.planState = 1;
                    }
                };

                $scope.deactivatePlan = () => {
                    var result = PlanService.deactivate({ planId: $scope.plan.id });
                    result.$promise.then((data) => {                        

                        // mark plan as inactive
                        $scope.plan.planState = 1;
                        var messageToShow = "Plan successfully deactivated";
                        ngToast.success(messageToShow);
                    })
                        .catch((err: any) => {
                            var messageToShow = "Failed to toggle Plan Status";
                            ngToast.danger(messageToShow);
                        });
                };
            };

            DesignerHeader.prototype.controller['$inject'] = ['$rootScope', '$scope', '$element', '$attrs', '$http', 'ngToast', 'PlanService'];
        }

        //The factory function returns Directive object as per Angular requirements
        public static Factory() {
            var directive = (PlanService: services.IPlanService) => {
                return new DesignerHeader(PlanService);
            };

            directive['$inject'] = ['PlanService'];
            return directive;
        }
    }

    app.directive('designerHeader', DesignerHeader.Factory());
}