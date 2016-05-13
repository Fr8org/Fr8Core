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

        reArrangePlans: (plan: interfaces.IPlanVM) => void;
        runningStatus: any;
        updatePlansLastUpdated: (id: any, date: any) => void;
        doesOrganizationExists: boolean;

        filter: any;

        inActiveQuery: model.PlanQueryDTO;
        inActivePromise: ng.IPromise<model.PlanResultDTO>;
        inActivePlans: model.PlanResultDTO;
        getInactivePlans: () => void;
        removeInactiveFilter: () => void;

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
            'PlanService',
            '$modal',
            '$state',
            'UIHelperService',
            'UserService',
            'PusherNotifierService'
        ];

        constructor(
            private $scope: IPlanListScope,
            private PlanService: services.IPlanService,
            private $modal,
            private $state: ng.ui.IStateService,
            private uiHelperService: services.IUIHelperService,
            private UserService: services.IUserService,
            private PusherNotifierService: services.IPusherNotifierService
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
            $scope.inActiveQuery.status = 1;
            $scope.inActiveQuery.planPerPage = 10;
            $scope.inActiveQuery.page = 1;
            $scope.inActiveQuery.orderBy = "-lastUpdated";
            this.getInactivePlans();

            $scope.activeQuery = new model.PlanQueryDTO();
            $scope.activeQuery.status = 2;
            $scope.activeQuery.planPerPage = 10;
            $scope.activeQuery.page = 1;
            $scope.activeQuery.orderBy = "-lastUpdated";
            this.getActivePlans();

            $scope.executePlan = <(plan: interfaces.IPlanVM) => void>angular.bind(this, this.executePlan);
            $scope.goToPlanPage = <(plan: interfaces.IPlanVM) => void>angular.bind(this, this.goToPlanPage);
            $scope.goToPlanDetailsPage = <(plan: interfaces.IPlanVM) => void>angular.bind(this, this.goToPlanDetailsPage);
            $scope.deletePlan = <(plan: interfaces.IPlanVM) => void>angular.bind(this, this.deletePlan);
            $scope.deactivatePlan = <(plan: interfaces.IPlanVM) => void>angular.bind(this, this.deactivatePlan);
            $scope.reArrangePlans = <(plan: interfaces.IPlanVM) => void>angular.bind(this, this.reArrangePlans);
            $scope.updatePlansLastUpdated = <(id: any, date: any) => void>angular.bind(this, this.updatePlanLastUpdated);
            $scope.getInactivePlans = <() => void>angular.bind(this, this.getInactivePlans);
            $scope.getActivePlans = <() => void>angular.bind(this, this.getActivePlans);
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
                                     PusherNotifierService.bindEventToChannel('fr8pusher_' + data.emailAddress, dockyard.services.pusherNotifierExecutionEvent, (data: any) => {
                                             this.updatePlanLastUpdated(data.PlanId, data.PlanLastUpdated);
                                     })
                    if (angular.isNumber(data.organizationId)) {
                        $scope.doesOrganizationExists = true;
                    }
                    else {
                        $scope.doesOrganizationExists = false;
                    }                                         ;
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

        private reArrangePlans(plan) {
            var planIndex = null;
            if (plan.planState === 1) {
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
        }
        
        private deactivatePlan(plan) {
            this.PlanService.deactivate({ planId: plan.id }).$promise.then((result) => {
                this.getActivePlans();
                this.getInactivePlans();
            }, () => {
                //deactivation failed
                //TODO show some kind of error message
            });
        }
        private updatePlanLastUpdated(id, date) {
            for (var i = 0; i < this.$scope.activePlans.plans.length; i++) {
                if (!this.$scope.activePlans.plans[i].id){
                                           break;
                }
                if (this.$scope.activePlans.plans[i].id === id) {
                    this.$scope.activePlans.plans[i].lastUpdated = date;
                                           break;
                                       }
                             }
                   }
        private executePlan(plan, $event) {
            // If Plan is inactive, activate it in-order to move under Running section
            var isInactive = plan.planState === 1;
            if (isInactive) {
                plan.planState = 2;
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
                        if (isInactive && data && data.currentPlanType === 1) {
                            // mark plan as Inactive as it is Run Once and then rearrange
                            plan.planState = 1;
                            this.reArrangePlans(plan);
                            this.getInactivePlans();
                            //this.$scope.inActivePlans = this.PlanService.getbystatus({ id: null, status: 1, category: '' });
                        }
                    })
                    .catch((failResponse) => {
                        if (failResponse.data.details === "GuestFail") {
                            location.href = "DockyardAccount/RegisterGuestUser";
                        } else {
                            if (isInactive && failResponse.toLowercase() === '1') {
                                // mark plan as Inactive as it is Run Once and then rearrange
                                plan.planState = 1;
                                this.reArrangePlans(plan);
                                this.getInactivePlans();
                                //this.$scope.inActivePlans = this.PlanService.getbystatus({ id: null, status: 1, category: '' });
                            }
                        }
                    });
            }
        }

        private goToPlanPage(planId) {
            this.$state.go('planBuilder', { id: planId });
        }

        private goToPlanDetailsPage(planId) {
            this.$state.go('planDetails', { id: planId });
        }

        private deletePlan(planId: string, isActive: number) {
            //to save closure of our controller
            var self = this;

            var alertMessage = new model.AlertDTO();
            alertMessage.title = "Delete Confirmation";
            alertMessage.body = "Are you sure that you wish to delete this Plan?";

            this.uiHelperService
                .openConfirmationModal(alertMessage).then(() => {
                    //Deletion confirmed
                    this.PlanService.delete({ id: planId }).$promise.then(() => {
                        var procTemplates = isActive === 2 ? self.$scope.activePlans.plans : self.$scope.inActivePlans.plans;
                        //now loop through our existing templates and remove from local memory
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