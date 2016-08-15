/// <reference path="../_all.ts" />

module dockyard.controllers {    
    'use strict';

    export interface IAdminToolsScope extends ng.IScope {
        user: interfaces.IUserDTO;
        checkPages: ()=>void;

        submit: (isValid: boolean) => void;
        cancel: () => void;
    }

    class AdminToolsController {
        // $inject annotation.
        // It provides $injector with information about dependencies to be injected into constructor
        // it is better to have it close to the constructor, because the parameters must match in count and type.
        // See http://docs.angularjs.org/guide/di
        public static $inject = [
            '$scope',
            '$state',
            '$http',
            'UINotificationService'
        ];

        constructor(
            private $scope: IAdminToolsScope,
            private $state: ng.ui.IStateService,
            private $http: ng.IHttpService,
            private uiNotifications: interfaces.IUINotificationService
            ) {

            //Save button
            $scope.checkPages = function () {
                

                let success = (response) => {
                    alert(response.data);

                };
                let fail = (data) => {
                    uiNotifications.notify(data,dockyard.enums.UINotificationStatus.Error,null);
                };

                $http.get("/api/page_generation/generate_pages").then(success,fail);
            };

        }
    }

    app.controller('AdminToolsController', AdminToolsController);
}