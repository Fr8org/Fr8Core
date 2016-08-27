module dockyard.services {
    export class AuthService {
        private _pendingActivities: { [key: string]: model.ActivityDTO };
        private _canceledActivities: { [key: string]: boolean };
        private _authDialogDisplayed: boolean;
        private _currentPlan: interfaces.IPlanVM;

        constructor(
            private $rootScope: ng.IScope,
            private $interval: ng.IIntervalService,
            private $modal,
            private ConfigureTrackerService: services.ConfigureTrackerService,
            private ActivityTemplateHelperService: services.IActivityTemplateHelperService,
            private ActivityService: services.IActivityService) {

            var self = this;

            self._pendingActivities = {};
            self._authDialogDisplayed = false;
            self._canceledActivities = {};

            self.$interval(function () {
                self.intervalHandler();
            }, 1000);
        }

        private intervalHandler() {
            if (!this.ConfigureTrackerService.hasPendingConfigureAuthCalls()) {
                var activities: Array<model.ActivityDTO> = [];
                var key;
                for (key in this._pendingActivities) {
                    if (!this._pendingActivities.hasOwnProperty(key) || this._canceledActivities[key]) {
                        continue;
                    }

                    activities.push(this._pendingActivities[key]);
                }

                if (activities.length > 0 && !this._authDialogDisplayed) {
                    this.startAuthentication(activities);
                }
            }
        }

        public setCurrentPlan(plan: interfaces.IPlanVM) {
            this._currentPlan = plan;
            this._canceledActivities = {};
        }

        public clear() {
            this._pendingActivities = {};
        }

        public enableAuthentication(activityId: string) {
            if (this._canceledActivities[activityId]) {
                delete this._canceledActivities[activityId];
            }
        }

        public isAuthenticationCanceled(activityId: string) {
            return !!this._canceledActivities[activityId];
        }

        public enqueue(activity: model.ActivityDTO) {
            if (!(activity.id in this._pendingActivities)) {
                this._pendingActivities[activity.id] = activity;
            }
        }

        public isSolutionBasedPlan() {
            if (!this._currentPlan.subPlans) {
                return false;
            }

            var subPlan = this._currentPlan.subPlans[0];
            if (!subPlan || !subPlan.activities) {
                return false;
            }

            var activity = <model.ActivityDTO>subPlan.activities[0];
            if (!activity || !activity.activityTemplate) {
                return false;
            }
            var at = this.ActivityTemplateHelperService.getActivityTemplate(activity);
            if (at.categories.some((value, index, arr) => { return value.name && value.name.toLowerCase() === 'Solution'; })
                // Second clause to force new algorithm work only for specific activities.
                && at.tags === 'UsesReconfigureList') {

                return true;
            }

            return false;
        }

        public startAuthentication(activities: Array<model.ActivityDTO>) {
            var self = this;

            var modalScope = <controllers.IAuthenticationDialogScope>self.$rootScope.$new(true);
            var planActivityByTerminal = {};
            //Trying to find other activities of the same terminal belong to current plan
            this.ActivityService.getAllActivities(this._currentPlan).forEach(activity => {
                var terminalName = activity.activityTemplate.terminalName;
                if (!planActivityByTerminal.hasOwnProperty(terminalName)) {
                    planActivityByTerminal[terminalName] = [];
                }
                (<any>activity).authorizeIsRequested = false;
                planActivityByTerminal[terminalName].push(activity);
            });
            var resultActivities = [];
            activities.forEach(activity => {
                var terminalName = activity.activityTemplate.terminalName;
                if (planActivityByTerminal.hasOwnProperty(terminalName) &&
                    planActivityByTerminal[terminalName] !== undefined) {
                    planActivityByTerminal[terminalName].forEach(x => { resultActivities.push(x); });
                    delete planActivityByTerminal[terminalName];
                }
            });
            activities.forEach(activity => {
                var foundActivity = resultActivities.filter(x => x.id === activity.id)[0];
                if (foundActivity) {
                    foundActivity.authorizeIsRequested = true;
                }
                else if (activities.length === 1 && activity.ordering === 1) {
                    (<any>activity).authorizeIsRequested = true;
                    resultActivities.push(activity);
                }
            });
            activities = resultActivities;
            modalScope.activities = activities;
            self._authDialogDisplayed = true;

            // debugger;
            self.$modal.open({
                animation: true,
                templateUrl: '/AngularTemplate/AuthenticationDialog',
                controller: 'AuthenticationDialogController',
                scope: modalScope
            })
            .result
            .then((result) => {
                if (!this.isSolutionBasedPlan()) {
                    angular.forEach(activities, it => {
                        if (result.indexOf(it.id) === -1) { return; }
                        self.$rootScope.$broadcast(
                            directives.paneConfigureAction.MessageType[directives.paneConfigureAction.MessageType.PaneConfigureAction_AuthCompleted],
                            new directives.paneConfigureAction.AuthenticationCompletedEventArgs(<interfaces.IActivityDTO>({ id: it.id }))
                        );
                    });
                }
                else {
                    self.$rootScope.$broadcast(
                        directives.paneConfigureAction.MessageType[directives.paneConfigureAction.MessageType.PaneConfigureAction_AuthCompleted],
                        new directives.paneConfigureAction.AuthenticationCompletedEventArgs(this._currentPlan.subPlans[0].activities[0])
                    );

                    console.log(
                        'AuthService.ts',
                        'Configuring root solution activity with ID = '
                            + this._currentPlan.subPlans[0].activities[0].id
                    );
                }
            })
            .catch((result) => {
                //If user cancelled authorization we should mark only those activities for which authorization was requested
                angular.forEach(activities, function (a) {
                    if (!self._canceledActivities[a.id] && (<any>a).authorizeIsRequested) {
                        self._canceledActivities[a.id] = true;
                    }
                });

                angular.forEach(activities, it => {
                    if ((<any>it).authorizeIsRequested) {
                        self.$rootScope.$broadcast(
                        directives.paneConfigureAction.MessageType[directives.paneConfigureAction.MessageType.PaneConfigureAction_AuthFailure],
                        new directives.paneConfigureAction.ActionAuthFailureEventArgs(it.id)
                        );
                    }
                });
            })
            .finally(() => {
                this._authDialogDisplayed = false;
                this.clear();
            });
        }
    }
}

app.service(
    'AuthService',
    [
        '$rootScope',
        '$interval',
        '$modal',
        'ConfigureTrackerService',
        'ActivityTemplateHelperService',
        'ActivityService',
        dockyard.services.AuthService
    ]
);