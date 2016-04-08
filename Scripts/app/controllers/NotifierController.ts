/// <reference path="../_all.ts" />

module dockyard.controllers.NotifierController {
    'use strict';
    
    export class Fr8InternalEvent {
        data: any;
        type: string;
    }
    export interface INotifierControllerScope extends ng.IScope {
        eventList: Array<Fr8InternalEvent>;
    }
    export const successEvent = 'fr8pusher_generic_success';
    export const failureEvent = 'fr8pusher_generic_failure';
    export const executionEvent = 'fr8pusher_activity_execution_info';
    export const terminalEvent = 'fr8pusher_terminal_event';


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

            UserService.getCurrentUser().$promise.then(data => {
                $scope.eventList = [];
                
                PusherNotifierService.bindEventToChannel('fr8pusher_' + data.emailAddress, successEvent, (data: any) => {
                    this.$mdSidenav('right')
                        .toggle()
                        .then(function () {
                        });
                    var event = new Fr8InternalEvent();
                    event.type = successEvent;
                    event.data = data;
                    this.$scope.eventList.splice(0,0,event);
                    //ngToast.create(data);

                });

                PusherNotifierService.bindEventToChannel('fr8pusher_' + data.emailAddress, terminalEvent, (data: any) => {
                    var event = new Fr8InternalEvent();
                    event.type = terminalEvent;
                    event.data = data;
                    this.$scope.eventList.splice(0, 0, event);
                });

                PusherNotifierService.bindEventToChannel('fr8pusher_' + data.emailAddress, executionEvent, (data: any) => {
                    this.$mdSidenav('right')
                        .toggle()
                        .then(function () {
                        });
                    //var contentTemplate = "<label class='toast-activity-info'>Executing Activity: " + data.ActivityName + "</label><label class='toast-activity-info'>For Plan: " + data.PlanName + "</label> <label class='toast-activity-info'>Container: " + data.ContainerId +"</label>";
                    var event = new Fr8InternalEvent();
                    event.type = executionEvent;
                    event.data = data;
                    this.$scope.eventList.splice(0, 0, event);
                    //ngToast.create({
                    //    className : "success",
                    //    content : contentTemplate
                    //});
                });

                PusherNotifierService.bindEventToChannel('fr8pusher_' + data.emailAddress, failureEvent, (data: any) => {
                    this.$mdSidenav('right')
                        .toggle()   
                        .then(function () {
                        });
                    var event = new Fr8InternalEvent();
                    event.type = failureEvent;
                    event.data = data;
                    this.$scope.eventList.splice(0, 0, event);
                    //ngToast.danger(data);
                });
            });
        }
    }

    app.controller('NotifierController', NotifierController);
   
}