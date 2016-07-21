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
            '$scope'
        ];

        constructor(
            private UserService: services.IUserService,
            private PusherNotifierService: services.IPusherNotifierService,
            private uiNotificationService: services.IUINotificationService,
            private $mdSidenav: any,
            private $scope: INotifierControllerScope) {

            // liner-progress-bar controll
            $scope.planIsRunning = false;

            this.$scope.$on(<any>designHeaderEvents.PLAN_EXECUTION_STARTED,
                (event: ng.IAngularEvent) => {
                    $scope.planIsRunning = true;
                });

            this.$scope.$on(<any>designHeaderEvents.PLAN_EXECUTION_STOPPED,
                (event: ng.IAngularEvent) => {
                    $scope.planIsRunning = false;
                });

            UserService.getCurrentUser().$promise.then(data => {
                $scope.eventList = [];

                var channel: string = data.id;

                // ActivityStream
                PusherNotifierService.bindEventToChannel(channel, dockyard.enums.NotificationArea[dockyard.enums.NotificationArea.ActivityStream], (data: any) => {
                    var event = new Fr8InternalEvent();
                    event.type = data.NotificationType;
                    event.data = data;
                    this.$scope.eventList.splice(0, 0, event);
                });

                // Toast Messages
                PusherNotifierService.bindEventToChannel(channel, dockyard.enums.NotificationArea[dockyard.enums.NotificationArea.Toast], (data: any) => {
                    switch (data.NotificationType) {
                        case dockyard.enums.NotificationType.GenericSuccess:
                            uiNotificationService.notify(data.Message, dockyard.enums.UINotificationStatus.Success, null);
                            break;
                        case dockyard.enums.NotificationType.GenericFailure:
                            uiNotificationService.notify(data.Message, dockyard.enums.UINotificationStatus.Error, null);
                            break;
                        case dockyard.enums.NotificationType.GenericInfo:
                            uiNotificationService.notify(data.Message, dockyard.enums.UINotificationStatus.Info, null);
                            break;
                        default:
                            uiNotificationService.notify(data.Message, dockyard.enums.UINotificationStatus.Alert, null);
                            break;
                    }
                });
            });
        }
    }
    app.controller('NotifierController', NotifierController);
}