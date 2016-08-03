/// <reference path="../_all.ts" />

module dockyard.controllers.NotifierController {
    'use strict';

    export class Fr8InternalEvent {
        data: any;
        type: dockyard.enums.NotificationType;
    }

    export interface INotifierControllerScope extends ng.IScope {
        eventList: Array<Fr8InternalEvent>;
        planIsRunning: Boolean;
    }

    import designHeaderEvents = dockyard.Fr8Events.DesignerHeader;

    class NotifierController {
        // $inject annotation.
        // It provides $injector with information about dependencies to be injected into constructor
        // it is better to have it close to the constructor, because the parameters must match in count and type.
        // See http://docs.angularjs.org/guide/di
        public static $inject = [
            'UserService',
            'PusherNotifierService',
            'UINotificationService',
            '$mdSidenav',
            '$scope',
            '$stateParams'
        ];

        constructor(
            private UserService: services.IUserService,
            private PusherNotifierService: services.IPusherNotifierService,
            private uiNotificationService: services.IUINotificationService,
            private $mdSidenav: any,
            private $scope: INotifierControllerScope,
            private $stateParams: ng.ui.IStateParamsService) {

            $scope.planIsRunning = false; // Used for linear-progress-bar control
            var user = null;
            var isScopeDestroyed = false;

            this.$scope.$on(<any>designHeaderEvents.PLAN_EXECUTION_STARTED,
                (event: ng.IAngularEvent) => {
                    $scope.planIsRunning = true;
                });

            this.$scope.$on(<any>designHeaderEvents.PLAN_EXECUTION_STOPPED,
                (event: ng.IAngularEvent) => {
                    $scope.planIsRunning = false;
                });

            this.$scope.$on('$destroy', (event: ng.IAngularEvent) => {
                isScopeDestroyed = true;
                if (user !== null) {
                    PusherNotifierService.removeAllEvents(user.id);
                }
            });

            UserService.getCurrentUser().$promise.then(data => {
                // Destroyed scope's channel binding can be called late which we prevent it in here
                if (isScopeDestroyed) {
                    return;
                }

                $scope.eventList = [];
                var channel: string = data.id;
                user = data;

                // Generic Success
                PusherNotifierService.bindEventToChannel(channel, dockyard.enums.NotificationType[dockyard.enums.NotificationType.GenericSuccess], (data: any) => {
                    this.sendNotification(data);
                });

                // Generic Failure
                PusherNotifierService.bindEventToChannel(channel, dockyard.enums.NotificationType[dockyard.enums.NotificationType.GenericFailure], (data: any) => {
                    this.sendNotification(data);
                });

                // Generic Info
                PusherNotifierService.bindEventToChannel(channel, dockyard.enums.NotificationType[dockyard.enums.NotificationType.GenericInfo], (data: any) => {
                    this.sendNotification(data);
                });

                // Terminal Event
                PusherNotifierService.bindEventToChannel(channel, dockyard.enums.NotificationType[dockyard.enums.NotificationType.TerminalEvent], (data: any) => {
                    this.sendNotification(data);
                });

                // Execution Stopped
                PusherNotifierService.bindEventToChannel(channel, dockyard.enums.NotificationType[dockyard.enums.NotificationType.ExecutionStopped], (data: any) => {
                    this.sendNotification(data);
                });
            });
        }

        // Determines notifications are (toast message or activity stream)
        sendNotification(data: any): void {
            if (this.$stateParams['viewMode'] == "kiosk") {
                // All notifications are implemented as Alert messages in toast.
                // When we implement sub-notification types for TerminalEvent, we can reevaluate here and pass a parameter for it
                this.uiNotificationService.notify(data.Message, dockyard.enums.UINotificationStatus.Alert, null);
            } else {
                var event = new Fr8InternalEvent();
                event.type = data.NotificationType;
                event.data = data;
                this.$scope.eventList.splice(0, 0, event);
            }
        }
    }

    app.controller('NotifierController', NotifierController);
}