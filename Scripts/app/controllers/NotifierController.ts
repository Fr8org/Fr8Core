/// <reference path="../_all.ts" />

module dockyard.controllers {
    'use strict';

    class NotifierController {
        // $inject annotation.
        // It provides $injector with information about dependencies to be injected into constructor
        // it is better to have it close to the constructor, because the parameters must match in count and type.
        // See http://docs.angularjs.org/guide/di
        public static $inject = [
            'UserService',
            'PusherNotifierService',
            'ngToast'
        ];

        constructor(
            private UserService: services.IUserService,
            private PusherNotifierService: services.IPusherNotifierService,
            private ngToast: any) {

            UserService.getCurrentUser().$promise.then(data => {
                
                PusherNotifierService.bindEventToChannel('fr8pusher_' + data.emailAddress, 'fr8pusher_generic_success', (data: any) => {
                    ngToast.create(data);
                });

                PusherNotifierService.bindEventToChannel('fr8pusher_' + data.emailAddress, 'fr8pusher_activity_execution_info', (data: any) => {

                    var contentTemplate = "<label class='toast-activity-info'>Executing Activity: <strong>" + data.ActivityName + "</strong></label><label class='toast-activity-info'>For Plan: <strong>" + data.PlanName + "</strong></label> <label class='toast-activity-info'>Container: <strong>" + data.ContainerId +"</strong></label>";

                    ngToast.create({
                        className : "success",
                        content : contentTemplate
                    });
                });

                PusherNotifierService.bindEventToChannel('fr8pusher_' + data.emailAddress, 'fr8pusher_generic_failure', (data: any) => {
                    ngToast.danger(data);
                });
            });
        }
    }

    app.controller('NotifierController', NotifierController);
}