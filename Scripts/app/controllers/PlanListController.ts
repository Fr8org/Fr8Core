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
        activatePlan: (plan: interfaces.IPlanVM) => void;
        deactivatePlan: (plan: interfaces.IPlanVM) => void;
        dtOptionsBuilder: any;
        dtColumnDefs: any;
        activePlans: Array<interfaces.IPlanVM>;
        inActivePlans: Array<interfaces.IPlanVM>;
        activePlansCategory: Array<interfaces.IPlanVM>;
        inActivePlansCategory: Array<interfaces.IPlanVM>;
        reArrangePlans: (plan: interfaces.IPlanVM) => void;
        runningStatus: any;
        updatePlansLastUpdated: (id: any, date: any) => void;
        isOrganizationExists: boolean;
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
            'DTOptionsBuilder',
            'DTColumnDefBuilder',
            '$state',
            'UIHelperService',
            'UserService',
            'PusherNotifierService'
        ];

        constructor(
            private $scope: IPlanListScope,
            private PlanService: services.IPlanService,
            private $modal,
            private DTOptionsBuilder,
            private DTColumnDefBuilder,
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

            //Load Process Templates view model
            $scope.dtOptionsBuilder = DTOptionsBuilder.newOptions().withPaginationType('full_numbers').withDisplayLength(10);
            $scope.dtColumnDefs = this.getColumnDefs();
            $scope.activePlans = PlanService.getbystatus({ id: null, status: 2, category: '' });
            $scope.inActivePlans = PlanService.getbystatus({ id: null, status: 1, category: '' });
            $scope.activePlansCategory = PlanService.getbystatus({ id: null, status: 2, category: 'report' });
            $scope.inActivePlansCategory = PlanService.getbystatus({ id: null, status: 1, category: 'report' });
            $scope.executePlan = <(plan: interfaces.IPlanVM) => void>angular.bind(this, this.executePlan);
            $scope.goToPlanPage = <(plan: interfaces.IPlanVM) => void>angular.bind(this, this.goToPlanPage);
            $scope.goToPlanDetailsPage = <(plan: interfaces.IPlanVM) => void>angular.bind(this, this.goToPlanDetailsPage);
            $scope.deletePlan = <(plan: interfaces.IPlanVM) => void>angular.bind(this, this.deletePlan);
            $scope.activatePlan = <(plan: interfaces.IPlanVM) => void>angular.bind(this, this.activatePlan);
            $scope.deactivatePlan = <(plan: interfaces.IPlanVM) => void>angular.bind(this, this.deactivatePlan);
            $scope.reArrangePlans = <(plan: interfaces.IPlanVM) => void>angular.bind(this, this.reArrangePlans);
            $scope.updatePlansLastUpdated = <(id: any, date: any) => void>angular.bind(this, this.updatePlanLastUpdated);
            
                UserService.getCurrentUser().$promise.then(data => {
                                     PusherNotifierService.bindEventToChannel('fr8pusher_' + data.emailAddress, dockyard.services.pusherNotifierExecutionEvent, (data: any) => {
                                             this.updatePlanLastUpdated(data.PlanId, data.PlanLastUpdated);
                    });
                    if (angular.isNumber(data.organizationId) && data.organizationId !== 0) {
                         $scope.isOrganizationExists = true;
                    }
                    else {
                        $scope.isOrganizationExists = false;
                    }
                                        
                });
        }

        private reArrangePlans(plan) {
            var planIndex = null;
            if (plan.planState === 1) {
                planIndex = this.$scope.activePlans.map(function (r) { return r.id }).indexOf(plan.id);
                if (planIndex > -1) {
                    this.$scope.inActivePlans.push(plan);
                    this.$scope.activePlans.splice(planIndex, 1);
                    this.$scope.activePlans = this.$scope.activePlans;
                }
            } else {
                planIndex = this.$scope.inActivePlans.map(function (r) { return r.id }).indexOf(plan.id);
                if (planIndex > -1) {
                    this.$scope.activePlans.push(plan);
                    this.$scope.inActivePlans.splice(planIndex, 1);
                    this.$scope.inActivePlans = this.$scope.inActivePlans;
                }
            }
        }

        private getColumnDefs() {
            return [
                this.DTColumnDefBuilder.newColumnDef(0),
                this.DTColumnDefBuilder.newColumnDef(1),
                this.DTColumnDefBuilder.newColumnDef(2),
                    // .renderWith(function (data, type, full, meta) {
                    //     if (data != null || data != undefined) {
                    //         var dateValue = new Date(data);
                    //         if (dateValue.getFullYear() == 1)
                    //             return "";
                    //         var date = dateValue.toLocaleDateString() + ' ' + dateValue.toLocaleTimeString();
                    //         return date;
                    //     }
                    // }),
                this.DTColumnDefBuilder.newColumnDef(3).notSortable(),
                this.DTColumnDefBuilder.newColumnDef(4).notSortable()
            ];
        }

        private activatePlan(plan) {
            this.PlanService.activate({ planId: plan.id, planBuilderActivate: false }).$promise.then((result) => {
                if (result != null && result.status === "validation_error" && result.redirectToPlan) {
                    this.goToPlanPage(plan.id);
                }
                else {
                    location.reload();
                }
            }, (failResponse) => {
                //activation failed
                if (failResponse.data.details === "GuestFail") {
                    location.href = "DockyardAccount/RegisterGuestUser";
                }
            });

        }
        private deactivatePlan(plan) {
            this.PlanService.deactivate({ planId: plan.id }).$promise.then((result) => {
                this.$scope.inActivePlans = this.PlanService.getbystatus({ id: null, status: 1, category: '' });
                this.$scope.activePlans = this.PlanService.getbystatus({ id: null, status: 2, category: '' });
                //location.reload();
            }, () => {
                //deactivation failed
                //TODO show some kind of error message
            });
        }
        private updatePlanLastUpdated(id, date) {
                       for (var i = 0; i < this.$scope.activePlans.length; i++) {
                                 if (!this.$scope.activePlans[i].id)
                                           break;
                                 if (this.$scope.activePlans[i].id == id) {
                                           this.$scope.activePlans[i].lastUpdated = date;
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
                            this.$scope.inActivePlans = this.PlanService.getbystatus({ id: null, status: 1, category: '' });
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
                                this.$scope.inActivePlans = this.PlanService.getbystatus({ id: null, status: 1, category: '' });
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
                        var procTemplates = isActive === 2 ? self.$scope.activePlans : self.$scope.inActivePlans;
                        //now loop through our existing templates and remove from local memory
                        for (var i = 0; i < procTemplates.length; i++) {
                            if (procTemplates[i].id === planId) {
                                procTemplates.splice(i, 1);
                                break;
                            }
                        }
                    });
                });
        }
    }
    app.controller('PlanListController', PlanListController);
}