/// <reference path="../_all.ts" />

module dockyard.controllers {    
    'use strict';

    export interface IAdminToolsScope extends ng.IScope {
        user: interfaces.IUserDTO;
        generatePages: () => void;
        generatePagesInProgress: boolean;
    }

    class AdminToolsController {
        // $inject annotation.
        // It provides $injector with information about dependencies to be injected into constructor
        // it is better to have it close to the constructor, because the parameters must match in count and type.
        // See http://docs.angularjs.org/guide/di
        public static $inject = [
            '$scope',
            '$http',
            'UINotificationService'
        ];

        constructor(
            private $scope: IAdminToolsScope,
            private $http: ng.IHttpService,
            private uiNotifications: interfaces.IUINotificationService
            ) {
            $scope.generatePagesInProgress = false;
            $scope.generatePages = function () {
                var url = '/api/plan_templates/generatepages';
                let success = (response) => {
                    $scope.generatePagesInProgress = false;
                    uiNotifications.notifyToast(response.data, dockyard.enums.UINotificationStatus.Success, null);
                };
                let fail = (data) => {
                    $scope.generatePagesInProgress = false;
                    uiNotifications.notifyToast(data, dockyard.enums.UINotificationStatus.Error, null);
                };

                $scope.generatePagesInProgress = true;
                $http.post(url, null).then(success, fail);
            }
        }
    }

    app.controller('AdminToolsController', AdminToolsController);
}