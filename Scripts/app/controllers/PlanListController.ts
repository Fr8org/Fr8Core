/// <reference path="../_all.ts" />

/*
    Detail (view/add/edit) controller
*/
module dockyard.controllers {
    'use strict';

    import designHeaderEvents = dockyard.Fr8Events.DesignerHeader;

    export interface IPlanListScope extends ng.IScope {
        executePlan: (plan: interfaces.IPlanVM) => void;
        goToPlanPage: (plan: interfaces.IPlanVM) => void;
        goToPlanDetailsPage: (plan: interfaces.IPlanVM) => void;
        deletePlan: (plan: interfaces.IPlanVM) => void;
        deactivatePlan: (plan: interfaces.IPlanVM) => void;
        addPlan: () => void;

        reArrangePlans: (plan: interfaces.IPlanVM) => void;
        runningStatus: any;
        updatePlansLastUpdated: (id: any, date: any) => void;
        doesOrganizationExists: boolean;

        createTemplate: (plan: interfaces.IPlanVM)=>void;

        filter: any;

        inActiveQuery: model.PlanQueryDTO;
        inActivePromise: ng.IPromise<model.PlanResultDTO>;
        inActivePlans: model.PlanResultDTO;

        executingQuery: model.PlanQueryDTO;
        executingPromise: ng.IPromise<model.PlanResultDTO>;
        executingPlans: model.PlanResultDTO;
        getExecutingPlans: () => void;

        activeOrExecutingPlans: (plans: model.PlanDTO) => boolean;
        getInactivePlans: () => void;
        removeInactiveFilter: () => void;

        Query: model.PlanQueryDTO;
        activeQuery: model.PlanQueryDTO;
        activePromise: ng.IPromise<model.PlanResultDTO>;
        activePlans: model.PlanResultDTO;
        getActivePlans: () => void;
        removeActiveFilter: () => void;

    }

    /*
        List controller
    */
    class PlanListController {
        // $inject annotation.
        // It provides $injector with information about dependencies to be injected into constructor
        // it is better to have it close to the constructor, because the parameters must match in count and type.
        // See http://docs.angularjs.org/guide/di
        public static $inject = [
            '$scope',
            '$rootScope',
            'PlanService',
            '$modal',
            '$state',
            'UIHelperService',
            'UserService',
            'UINotificationService'
        ];

        constructor(
            private $scope: IPlanListScope,
            private $rootScope: IPlanListScope,
            private PlanService: services.IPlanService,
            private $modal,
            private $state: ng.ui.IStateService,
            private uiHelperService: services.IUIHelperService,
            private UserService: services.IUserService,
            private uiNotificationService: interfaces.IUINotificationService
            ) {

            $scope.$on('$viewContentLoaded', () => {
                // initialize core components
                Metronic.initAjax();
            });

            // This is to reArrangePlans so that plans get rendered in desired sections i.e Running or Plans Library
            $scope.$on(<any>designHeaderEvents.PLAN_EXECUTION_COMPLETED_REARRANGE_PLANS, (event, plan) => {
                this.reArrangePlans(plan);
            });

            $scope.filter = {
                options: {
                    debounce: 500
                }
            };

            $scope.inActiveQuery = new model.PlanQueryDTO();
            $scope.inActiveQuery.status = model.PlanState.Inactive;
            $scope.inActiveQuery.planPerPage = 10;
            $scope.inActiveQuery.page = 1;
            $scope.inActiveQuery.orderBy = "-lastUpdated";

            $scope.activeQuery = new model.PlanQueryDTO();
            $scope.activeQuery.status = model.PlanState.Active;
            $scope.activeQuery.planPerPage = 10;
            $scope.activeQuery.page = 1;
            $scope.activeQuery.orderBy = "-lastUpdated";

            $scope.executingQuery = new model.PlanQueryDTO();
            $scope.executingQuery.status = model.PlanState.Executing;
            $scope.executingQuery.planPerPage = 10;
            $scope.executingQuery.page = 1;
            $scope.executingQuery.orderBy = "-lastUpdated";

            $scope.Query = new model.PlanQueryDTO();
            $scope.Query.planPerPage = 20;
            $scope.Query.page = 1;
            $scope.Query.orderBy = "-lastUpdated";
            this.getPlans();

            $scope.executePlan = <(plan: interfaces.IPlanVM) => void>angular.bind(this, this.executePlan);
            $scope.goToPlanPage = <(plan: interfaces.IPlanVM) => void>angular.bind(this, this.goToPlanPage);
            $scope.goToPlanDetailsPage = <(plan: interfaces.IPlanVM) => void>angular.bind(this, this.goToPlanDetailsPage);
            $scope.deletePlan = <(plan: interfaces.IPlanVM) => void>angular.bind(this, this.deletePlan);
            $scope.deactivatePlan = <(plan: interfaces.IPlanVM) => void>angular.bind(this, this.deactivatePlan);
            $scope.reArrangePlans = <(plan: interfaces.IPlanVM) => void>angular.bind(this, this.reArrangePlans);
            $scope.getInactivePlans = <() => void>angular.bind(this, this.getInactivePlans);
            $scope.getActivePlans = <() => void>angular.bind(this, this.getActivePlans);
            $scope.getExecutingPlans = <() => void>angular.bind(this, this.getExecutingPlans);
            $scope.removeInactiveFilter = <() => void>angular.bind(this, this.removeInactiveFilter);
            $scope.removeActiveFilter = <() => void>angular.bind(this, this.removeActiveFilter);

            $scope.$watch('inActiveQuery.filter', (newValue, oldValue) => {
                var bookmark: number = 1;
                if (!oldValue) {
                    bookmark = $scope.inActiveQuery.page;
                }
                if (newValue !== oldValue) {
                    $scope.inActiveQuery.page = 1;
                }
                if (!newValue) {
                    $scope.inActiveQuery.page = bookmark;
                }
                this.getInactivePlans();
            });

            $scope.activeOrExecutingPlans = function (plan: model.PlanDTO) {
                return plan.planState === model.PlanState.Active || plan.planState === model.PlanState.Executing; 
            }

            $scope.addPlan = function () {
               var plan = new dockyard.model.PlanDTO();
               plan.planState = dockyard.model.PlanState.Inactive;
               plan.visibility = { hidden: false, public: false };
               var result = PlanService.save(plan);

               result.$promise.then(() => {
                    $state.go('plan', { id: result.id });
               });
            };

            $scope.$watch('activeQuery.filter', (newValue, oldValue) => {
                var bookmark: number = 1;
                if (!oldValue) {
                    bookmark = $scope.activeQuery.page;
                }
                if (newValue !== oldValue) {
                    $scope.activeQuery.page = 1;
                }
                if (!newValue) {
                    $scope.activeQuery.page = bookmark;
                }
                this.getActivePlans();
            });      

            UserService.getCurrentUser().$promise.then(data => {
                $scope.doesOrganizationExists = angular.isNumber(data.organizationId);
            });
        }

        private removeInactiveFilter() {
            this.$scope.inActiveQuery.filter = null;
            this.$scope.filter.showInactive = false;
            this.getInactivePlans();
        }

        private removeActiveFilter() {
            this.$scope.activeQuery.filter = null;
            this.$scope.filter.showActive = false;
            this.getActivePlans();
        }

        private getPlans() {
            this.PlanService.getByQuery(this.$scope.Query).$promise
                .then((data: model.PlanResultDTO) => {
                    this.$scope.inActivePlans = new model.PlanResultDTO();
                    this.$scope.inActivePlans.currentPage = 1;
                    this.$scope.inActivePlans.plans = [];
                    this.$scope.inActivePlans.totalPlanCount = 0;
                    this.$scope.activePlans = new model.PlanResultDTO();
                    this.$scope.activePlans.currentPage = 1;
                    this.$scope.activePlans.plans = [];
                    this.$scope.activePlans.totalPlanCount = 0;
                    data.plans.map(
                        plan => {
                            if (plan.planState === model.PlanState.Inactive) {
                                this.$scope.inActivePlans.plans.push(plan);
                                this.$scope.inActivePlans.totalPlanCount++;
                            } else if (plan.planState === model.PlanState.Active || plan.planState === model.PlanState.Executing) {
                                this.$scope.activePlans.plans.push(plan);
                                this.$scope.activePlans.totalPlanCount++;
                            } 
                        }
                    );
                });
        }

        private getInactivePlans() {
            this.$scope.inActivePromise = this.PlanService.getByQuery(this.$scope.inActiveQuery).$promise;
            this.$scope.inActivePromise.then((data: model.PlanResultDTO) => {
                this.$scope.inActivePlans = data;
            });
        }

        private getActivePlans() {
            this.$scope.activePromise = this.PlanService.getByQuery(this.$scope.activeQuery).$promise;
            this.$scope.activePromise.then((data: model.PlanResultDTO) => {
                this.$scope.activePlans = data;
            });
        }

        private getExecutingPlans() {
            this.$scope.executingPromise = this.PlanService.getByQuery(this.$scope.executingQuery).$promise;
            this.$scope.executingPromise.then((data: model.PlanResultDTO) => {
                this.$scope.activePlans.plans.concat(data.plans);
                this.$scope.activePlans.totalPlanCount += data.totalPlanCount;
            });
        }

        private reArrangePlans(plan) {
            var planIndex = null;
            if (plan.planState === model.PlanState.Inactive) {
                planIndex = this.$scope.activePlans.plans.map(function (r) { return r.id }).indexOf(plan.id);
                if (planIndex > -1) {
                    this.$scope.inActivePlans.plans.push(plan);
                    ++this.$scope.inActivePlans.totalPlanCount;
                    this.$scope.activePlans.plans.splice(planIndex, 1);
                    --this.$scope.activePlans.totalPlanCount;
                }
            } else {
                planIndex = this.$scope.inActivePlans.plans.map(function (r) { return r.id }).indexOf(plan.id);
                if (planIndex > -1) {
                    this.$scope.activePlans.plans.push(plan);
                    ++this.$scope.activePlans.totalPlanCount;
                    this.$scope.inActivePlans.plans.splice(planIndex, 1);
                    --this.$scope.inActivePlans.totalPlanCount;
                }
            } 

            if ((this.$scope.activePlans.plans.length == 0) && this.$scope.inActivePlans.plans.length > 0) {
                this.$rootScope.$broadcast(<any>designHeaderEvents.PLAN_EXECUTION_STOPPED);
            }
            if (this.$scope.inActivePlans.plans.length == 0 && this.$scope.activePlans.plans.length > 0) {
                this.$rootScope.$broadcast(<any>designHeaderEvents.PLAN_EXECUTION_STARTED);
            }
        }
        
        private deactivatePlan(plan) {
            this.PlanService.deactivate({ planId: plan.id }).$promise.then((result) => {
                this.getActivePlans();
                this.getExecutingPlans();
                this.getInactivePlans();
            }, () => {
                // Deactivation failed
                //TODO show some kind of error message
            });
        }

        private executePlan(plan, $event) {
            // If Plan is inactive, activate it in-order to move under Running section
            var isInactive = plan.planState === model.PlanState.Inactive;
            if (isInactive) {
                plan.planState = model.PlanState.Executing;
                this.reArrangePlans(plan);
            }
            if ($event.ctrlKey) {
                this.$modal.open({
                    animation: true,
                    templateUrl: '/AngularTemplate/_AddPayloadModal',
                    controller: 'PayloadFormController', resolve: { planId: () => plan.id }
                });
            }
            else {
                this.PlanService
                    .runAndProcessClientAction(plan.id)
                    .then((data) => {
                        if (isInactive && data && data.currentPlanType == model.PlanType.RunOnce) {
                            // Mark plan as Inactive as it is Run Once and then rearrange
                            plan.planState = model.PlanState.Inactive;
                            this.reArrangePlans(plan);
                            this.getInactivePlans();
                            // this.$scope.inActivePlans = this.PlanService.getbystatus({ id: null, status: 1, category: '' });
                        }
                        if (data.currentPlanType === model.PlanType.OnGoing) {
                            plan.planState = model.PlanState.Active;
                        }
                    })
                    .catch((failResponse) => {
                        if (failResponse.data.details === "GuestFail") {
                            location.href = "Account/RegisterGuestUser";
                        } else {
                            if (isInactive) {
                                // Mark plan as Inactive as it is Run Once and then rearrange
                                plan.planState = model.PlanState.Inactive;
                                this.reArrangePlans(plan);
                                this.getInactivePlans();
                                this.goToPlanPage(plan.id);
                                // this.$scope.inActivePlans = this.PlanService.getbystatus({ id: null, status: 1, category: '' });
                            }
                        }
                    });
            }
        }

        private goToPlanPage(planId) {
            this.$state.go('plan', { id: planId });
        }

        private goToPlanDetailsPage(planId) {
            this.$state.go('plan.details', { id: planId });
        }

        private deletePlan(planId: string, isActive: number) {
            // Saves closure of our controller
            var self = this;

            var alertMessage = new model.AlertDTO();
            alertMessage.title = "Delete Confirmation";
            alertMessage.body = "Are you sure that you wish to delete this Plan?";

            this.uiHelperService
                .openConfirmationModal(alertMessage).then(() => {
                    // Deletion confirmed
                    this.PlanService.delete({ id: planId }).$promise.then(() => {
                        var procTemplates = isActive === 2 ? self.$scope.activePlans.plans : self.$scope.inActivePlans.plans;
                        // Now loop through our existing templates and remove from local memory
                        for (var i = 0; i < procTemplates.length; i++) {
                            if (procTemplates[i].id === planId) {
                                procTemplates.splice(i, 1);
                                break;
                            }
                        }
                        if (isActive === 2 && self.$scope.activePlans.plans.length < 1) {
                            self.getActivePlans();
                        }
                        else if (isActive !== 2 && self.$scope.inActivePlans.plans.length < 1) {
                            self.getInactivePlans();
                        }
                    });
                });
        }
    }
    app.controller('PlanListController', PlanListController);
}