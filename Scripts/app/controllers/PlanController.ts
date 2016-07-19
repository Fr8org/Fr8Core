/// <reference path="../_all.ts" />

/*
    Detail (view/add/edit) controller
*/

module dockyard.controllers {
    'use strict';

    // This is a fix for incomplit ts defenition for angular-ui module
    interface ngState extends ng.ui.IState {
        current: ng.ui.IState;
    }

    export interface IPlanScope extends ng.IScope {
        planId: string;
        fields: Array<model.Field>;
        current: model.PlanBuilderState;
    }


    //Setup aliases
    import pwd = dockyard.directives.paneWorkflowDesigner;
    import pca = dockyard.directives.paneConfigureAction;
    import psa = dockyard.directives.paneSelectAction;
    import designHeaderEvents = dockyard.Fr8Events.DesignerHeader;
    import planEvents = dockyard.Fr8Events.Plan;

    export class PlanController {
        // $inject annotation.
        // It provides $injector with information about dependencies to be injected into constructor
        // it is better to have it close to the constructor, because the parameters must match in count and type.
        // See http://docs.angularjs.org/guide/di


        public static $inject = [
            '$scope',
            '$state',
            'PlanService',
            '$timeout',
            'AuthService',
            '$stateParams'
        ];

        private _longRunningActionsCounter: number;
        private _loading = false;

        constructor(
            private $scope: IPlanBuilderScope,
            private $state: ng.ui.IStateService,
            private PlanService: services.IPlanService,
            private $timeout: ng.ITimeoutService,
            private AuthService: services.AuthService,
            private $stateParams: ng.ui.IStateParamsService
        ) {
            this.$scope.isPlanBuilderScope = true;
            this.$scope.isReConfiguring = false;

            this.$scope.current = new model.PlanBuilderState();


            this.$scope.view = $stateParams['view'];
            this.$scope.viewMode = $stateParams['viewMode'];


            this.$scope.$on('$stateChangeStart', (event, toState, toParams, fromState, fromParams, options) => {
                this.handleBackButton(event, toState, toParams, fromState, fromParams, options);
            });

            this.processState($state);
        }

        private handleBackButton(event, toState, toParams, fromState, fromParams, options) {

            if (fromParams.viewMode === "plan" && toParams.viewMode === undefined && fromState.name === "planBuilder" && toState.name === "planBuilder") {
                event.preventDefault();
                this.$state.go("planList");
            }

            if (toParams.viewMode === "plan" && fromParams.viewMode === undefined && fromState.name === "planBuilder" && toState.name === "planBuilder") {
                this.reloadFirstActions();
            }
        }

        private startLoader() {
            this._loading = true;
        }

        private stopLoader() {
            this._loading = false;
        }

        private setAdvancedEditingMode() {
            this.$scope.editingMode = 'advanced';
        }


        private processState($state: ngState) {
            if ($state.params.solutionName) {
                this.$scope.solutionName = $state.params.solutionName;
                var isGuid = /\w{8}-\w{4}-\w{4}-\w{4}-\w{12}/.test($state.params.solutionName);
                if (isGuid) {
                    this.$scope.planId = $state.params.solutionName;
                } else {
                    return this.createNewSolution($state.params.solutionName);
                }
            } else {
                this.$scope.planId = $state.params.id;
            }


            this.loadPlan($state.params.viewMode);
        }


        private createNewSolution(solutionName: string) {
            var plan = this.PlanService.createSolution({
                solutionName: solutionName
            });
            plan.$promise.then((curPlan: interfaces.IPlanFullDTO) => {
                this.$scope.planId = curPlan.plan.id;
                this.onPlanLoad('solution', curPlan);
            });
        }

        private loadPlan(mode = 'plan') {
            var planPromise = this.PlanService.getFull({ id: this.$scope.planId });
            planPromise.$promise.then(this.onPlanLoad.bind(this, mode));
        }

        private reloadFirstActions() {
            this.$timeout(() => {
                if (this.$scope.current.plan.planState != dockyard.model.PlanState.Running) {
                    this.$scope.current.plan.subPlans.forEach(
                        plan => {
                            if (plan.activities.length > 0) {
                                this.$scope.reConfigureAction(plan.activities[0])
                            }
                        });
                }
            }, 1500);
        }

        private onPlanLoad(mode: string, curPlan: interfaces.IPlanFullDTO) {
            this.AuthService.setCurrentPlan(<interfaces.IPlanVM>curPlan.plan);
            this.AuthService.clear();

            this.$scope.mode = mode;
            this.$scope.current.plan = curPlan.plan;
            //this.$scope.currentSubroute = curRoute.subroutes[0];
            if (curPlan.plan.subPlans.length > 1) {
                this.setAdvancedEditingMode();
            }
        }    

        private reloadPlan() {
            //this.$scope.actionGroups = [];
            this.$scope.current = new model.PlanBuilderState();
            this.loadPlan();
        }
    
        private PaneConfigureAction_SetSolutionMode() {
            if (this.$scope.solutionName) {
                return this.createNewSolution(this.$scope.solutionName);
            } else {
                this.loadPlan("solution");
            }
        }



        private PaneConfigureAction_ExecutePlan() {
            var self = this;

            ++self._longRunningActionsCounter;

            this.PlanService.runAndProcessClientAction(this.$scope.current.plan.id)
                .finally(() => {
                    if (this._longRunningActionsCounter > 0) {
                        --this._longRunningActionsCounter;
                    }
                });
        }

        private PaneConfigureAction_ShowAdvisoryMessage(eventArgs: pca.ShowAdvisoryMessagesEventArgs) {
            for (var i = 0; i < this.$scope.processedSubPlans.length; ++i) {
                var subPlan = this.$scope.processedSubPlans[i];
                if (!subPlan.actionGroups) {
                    continue;
                }
                for (var j = 0; j < subPlan.actionGroups.length; ++j) {
                    var actionGroup = subPlan.actionGroups[j];
                    if (!actionGroup.envelopes) {
                        continue;
                    }
                    for (var k = 0; k < actionGroup.envelopes.length; ++k) {
                        var envelope = actionGroup.envelopes[k];
                        if (envelope.activity.id === eventArgs.id) {
                            envelope.activity.showAdvisoryPopup = true;
                            envelope.activity.advisoryMessages = eventArgs.advisories;
                        }
                    }
                }
            }
        }
    }
    app.controller('PlanController', PlanController);
}