/// <reference path="../_all.ts" />

module dockyard.controllers.NotifierController {
    'use strict';

    import notificationType = dockyard.directives.NotificationType;
    import designHeaderEvents = dockyard.Fr8Events.DesignerHeader;

    export class Fr8InternalEvent {
        data: any;
        type: notificationType;
    }
    export interface INotifierControllerScope extends ng.IScope {
        eventList: Array<Fr8InternalEvent>;
        planIsRunning: Boolean;
    }

    class NotifierController {
        // $inject annotation.
        // It provides $injector with information about dependencies to be injected into constructor
        // it is better to have it close to the constructor, because the parameters must match in count and type.
        // See http://docs.angularjs.org/guide/di
        public static $inject = [
            'UserService',
            'PusherNotifierService',
            'ngToast',
            '$mdSidenav',
            '$scope'
        ];

        constructor(
            private UserService: services.IUserService,
            private PusherNotifierService: services.IPusherNotifierService,
            private ngToast: any,
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

                PusherNotifierService.bindEventToChannel(channel, notificationType[notificationType.GenericSuccess], (data: any) => {
                    var event = new Fr8InternalEvent();
                    event.type = notificationType.GenericSuccess;
                    event.data = data;
                    this.$scope.eventList.splice(0,0,event);
                });

                PusherNotifierService.bindEventToChannel(channel, notificationType[notificationType.GenericFailure], (data: any) => {
                    var event = new Fr8InternalEvent();
                    event.type = notificationType.GenericFailure;
                    event.data = data;
                    this.$scope.eventList.splice(0, 0, event);
                });

                PusherNotifierService.bindEventToChannel(channel, notificationType[notificationType.GenericInfo], (data: any) => {
                    var event = new Fr8InternalEvent();
                    event.type = notificationType.GenericInfo;
                    event.data = data;
                    this.$scope.eventList.splice(0, 0, event);
                });

                PusherNotifierService.bindEventToChannel(channel, notificationType[notificationType.TerminalEvent], (data: any) => {
                    var event = new Fr8InternalEvent();
                    event.type = notificationType.TerminalEvent;
                    event.data = data;
                    this.$scope.eventList.splice(0, 0, event);
                });
            });
        }
    }
    app.controller('NotifierController', NotifierController);
}