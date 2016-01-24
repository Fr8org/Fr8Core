module dockyard.services {
    export class AuthService {
        private _pendingActionIds: any;
        private _authDialogDisplayed: boolean;

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

        public clear() {
            this._pendingActionIds = {};
        }

        public enqueue(actionId: string) {
            if (!(actionId in this._pendingActionIds)) {
                this._pendingActionIds[actionId] = actionId;
            }
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
                angular.forEach(actionIds, function (it) {
                    self.$rootScope.$broadcast(
                        dockyard.directives.paneConfigureAction.MessageType[dockyard.directives.paneConfigureAction.MessageType.PaneConfigureAction_Reconfigure],
                        new dockyard.directives.paneConfigureAction.ActionReconfigureEventArgs(<interfaces.IActivityDTO>({ id: it }))
                    );
                });
            })
            .catch((result) => {
                angular.forEach(actionIds, function (it) {
                    self.$rootScope.$broadcast(
                        dockyard.directives.paneConfigureAction.MessageType[dockyard.directives.paneConfigureAction.MessageType.PaneConfigureAction_AuthFailure],
                        new dockyard.directives.paneConfigureAction.ActionAuthFailureEventArgs(it)
                    );
                });
            })
            .finally(function () {
                self._authDialogDisplayed = false;
                self.clear();
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