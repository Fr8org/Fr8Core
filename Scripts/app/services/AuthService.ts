module dockyard.services {
    export class AuthService {
        private _pendingActionIds: any;
        private _authDialogDisplayed: boolean;
        private _currentPlan: interfaces.IRouteVM;

        constructor(
            private $rootScope: ng.IScope,
            private $interval: ng.IIntervalService,
            private $modal,
            private ConfigureTrackerService: services.ConfigureTrackerService
            ) {

            var self = this;

            self._pendingActionIds = {};
            self._authDialogDisplayed = false;
            self.$interval(function () {
                self.intervalHandler();
            }, 1000);
        }

        private intervalHandler() {
            if (!this.ConfigureTrackerService.hasPendingConfigureAuthCalls()) {
                var actionIds: Array<string> = [];
                var key;
                for (key in this._pendingActionIds) {
                    if (!this._pendingActionIds.hasOwnProperty(key)) {
                        continue;
                    }

                    actionIds.push(key);
                }

                if (actionIds.length > 0 && !this._authDialogDisplayed) {
                    this.startAuthentication(actionIds);
                }
            }
        }

        public setCurrentPlan(plan: interfaces.IRouteVM) {
            this._currentPlan = plan;
        }

        public clear() {
            this._pendingActionIds = {};
        }

        public enqueue(actionId: string) {
            if (!(actionId in this._pendingActionIds)) {
                this._pendingActionIds[actionId] = actionId;
            }
        }

        public isSolutionBasedPlan() {
            if (!this._currentPlan.subroutes) {
                return false;
            }

            var subroute = this._currentPlan.subroutes[0];
            if (!subroute || !subroute.activities) {
                return false;
            }

            var activity = subroute.activities[0];
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

        public startAuthentication(actionIds: Array<string>) {
            var self = this;

            var modalScope = <any>self.$rootScope.$new(true);
            modalScope.actionIds = actionIds;

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
                    angular.forEach(actionIds, it => {
                        self.$rootScope.$broadcast(
                            dockyard.directives.paneConfigureAction.MessageType[dockyard.directives.paneConfigureAction.MessageType.PaneConfigureAction_AuthCompleted],
                            new dockyard.directives.paneConfigureAction.AuthenticationCompletedEventArgs(<interfaces.IActivityDTO>({ id: it }))
                        );
                    });
                }
                else {
                    self.$rootScope.$broadcast(
                        dockyard.directives.paneConfigureAction.MessageType[dockyard.directives.paneConfigureAction.MessageType.PaneConfigureAction_AuthCompleted],
                        new dockyard.directives.paneConfigureAction.AuthenticationCompletedEventArgs(this._currentPlan.subroutes[0].activities[0])
                    );

                    console.log(
                        'AuthService.ts',
                        'Configuring root solution activity with ID = '
                            + this._currentPlan.subroutes[0].activities[0].id
                    );
                }
            })
            .catch((result) => {
                angular.forEach(actionIds, it => {
                    self.$rootScope.$broadcast(
                        dockyard.directives.paneConfigureAction.MessageType[dockyard.directives.paneConfigureAction.MessageType.PaneConfigureAction_AuthFailure],
                        new dockyard.directives.paneConfigureAction.ActionAuthFailureEventArgs(it)
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