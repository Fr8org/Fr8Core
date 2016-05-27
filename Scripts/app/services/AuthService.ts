module dockyard.services {
    export class AuthService {
        private _pendingActivities: { [key: string]: model.ActivityDTO };
        private _authDialogDisplayed: boolean;
        private _currentPlan: interfaces.IPlanVM;

        constructor(
            private $rootScope: ng.IScope,
            private $interval: ng.IIntervalService,
            private $modal,
            private ConfigureTrackerService: services.ConfigureTrackerService
            ) {

            var self = this;

            self._pendingActivities = {};
            self._authDialogDisplayed = false;
            self.$interval(function () {
                self.intervalHandler();
            }, 1000);
        }

        private intervalHandler() {
            if (!this.ConfigureTrackerService.hasPendingConfigureAuthCalls()) {
                var activities: Array<model.ActivityDTO> = [];
                var key;
                for (key in this._pendingActivities) {
                    if (!this._pendingActivities.hasOwnProperty(key)) {
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
        }

        public clear() {
            this._pendingActivities = {};
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

            var activity = subPlan.activities[0];
            if (!activity || !activity.activityTemplate) {
                return false;
            }

            if (activity.activityTemplate.category === 'Solution'
                // Second clause to force new algorithm work only for specific activities.
                && activity.activityTemplate.tags === 'UsesReconfigureList') {

                return true;
            }

            return false;
        }

        public startAuthentication(activities: Array<model.ActivityDTO>) {
            var self = this;

            var modalScope = <controllers.IAuthenticationDialogScope>self.$rootScope.$new(true);
            modalScope.activities = activities;

            self._authDialogDisplayed = true;

            self.$modal.open({
                animation: true,
                templateUrl: '/AngularTemplate/AuthenticationDialog',
                controller: 'AuthenticationDialogController',
                scope: modalScope
            })
            .result
            .then(() => {
                if (!this.isSolutionBasedPlan()) {
                    angular.forEach(activities, it => {
                        self.$rootScope.$broadcast(
                            dockyard.directives.paneConfigureAction.MessageType[dockyard.directives.paneConfigureAction.MessageType.PaneConfigureAction_AuthCompleted],
                            new dockyard.directives.paneConfigureAction.AuthenticationCompletedEventArgs(<interfaces.IActivityDTO>({ id: it.id }))
                        );
                    });
                }
                else {
                    self.$rootScope.$broadcast(
                        dockyard.directives.paneConfigureAction.MessageType[dockyard.directives.paneConfigureAction.MessageType.PaneConfigureAction_AuthCompleted],
                        new dockyard.directives.paneConfigureAction.AuthenticationCompletedEventArgs(this._currentPlan.subPlans[0].activities[0])
                    );

                    console.log(
                        'AuthService.ts',
                        'Configuring root solution activity with ID = '
                            + this._currentPlan.subPlans[0].activities[0].id
                    );
                }
            })
            .catch((result) => {
                angular.forEach(activities, it => {
                    self.$rootScope.$broadcast(
                        dockyard.directives.paneConfigureAction.MessageType[dockyard.directives.paneConfigureAction.MessageType.PaneConfigureAction_AuthFailure],
                        new dockyard.directives.paneConfigureAction.ActionAuthFailureEventArgs(it.id)
                    );
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
        dockyard.services.AuthService
    ]
);