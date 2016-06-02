/// <reference path="../_all.ts" />

module dockyard.controllers.NotifierController {
    'use strict';
    
    export class Fr8InternalEvent {
        data: any;
        type: string;
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

                var channel: string = data.emailAddress;

                PusherNotifierService.bindEventToChannel(channel, dockyard.services.pusherNotifierSuccessEvent, (data: any) => {
                    var event = new Fr8InternalEvent();
                    event.data = data;
                    event.type = dockyard.services.pusherNotifierSuccessEvent;
                    this.$scope.eventList.splice(0,0,event);
                });

                PusherNotifierService.bindEventToChannel(channel, dockyard.services.pusherNotifierTerminalEvent, (data: any) => {
                    var event = new Fr8InternalEvent();
                    event.type = dockyard.services.pusherNotifierTerminalEvent;
                    event.data = data;
                    this.$scope.eventList.splice(0, 0, event);
                });

                PusherNotifierService.bindEventToChannel(channel, dockyard.services.pusherNotifierExecutionEvent, (data: any) => {
                    var event = new Fr8InternalEvent();
                    event.type = dockyard.services.pusherNotifierExecutionEvent;
                    event.data = data;
                    this.$scope.eventList.splice(0, 0, event);
                });

                PusherNotifierService.bindEventToChannel(channel, dockyard.services.pusherNotifierFailureEvent, (data: any) => {
                    var event = new Fr8InternalEvent();
                    event.type = dockyard.services.pusherNotifierFailureEvent;
                    event.data = data;
                    this.$scope.eventList.splice(0, 0, event);
                });
            });
        }
    }
    app.controller('NotifierController', NotifierController);
}