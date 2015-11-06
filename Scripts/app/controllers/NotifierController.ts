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
                
                PusherNotifierService.bindEventToChannel('fr8pusher_' + data.emailAddress, 'fr8pusher_container_executed', (data: any) => {
                    ngToast.create(data);
                });

                PusherNotifierService.bindEventToChannel('fr8pusher_' + data.emailAddress, 'fr8pusher_container_failed', (data: any) => {
                    ngToast.create(data);
                });
            });
        }
    }

    app.controller('NotifierController', NotifierController);
}