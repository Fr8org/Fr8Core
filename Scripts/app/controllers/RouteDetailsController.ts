/// <reference path="../_all.ts" />

module dockyard.controllers {
    'use strict';

    export interface IRouteDetailsScope extends ng.IScope {
        ptvm: interfaces.IRouteVM;
        submit: (isValid: boolean) => void;
        errorMessage: string;
        routeBuilder: any,
        id: string
    }

    class RouteDetailsController {
        // $inject annotation.
        // It provides $injector with information about dependencies to be injected into constructor
        // it is better to have it close to the constructor, because the parameters must match in count and type.
        // See http://docs.angularjs.org/guide/di
        public static $inject = [
            '$rootScope',
            '$scope',
            'RouteService',
            '$stateParams'
        ];

        constructor(
            private $rootScope: interfaces.IAppRootScope,
            private $scope: IRouteDetailsScope,
            private RouteService: services.IRouteService,
            private $stateParams: any) {
            
            //Load detailed information
            $scope.id = $stateParams.id;
            if (this.isValidGUID($scope.id)) {
                $scope.ptvm = RouteService.getFull({ id: $stateParams.id });
            }
        }

        // Regular Expression reference link
        // https://lostechies.com/gabrielschenker/2009/03/10/how-to-add-a-custom-validation-method-to-the-jquery-validator-plug-in/

        private isValidGUID(GUID) {
            var validGuid = /^({|()?[0-9a-fA-F]{8}-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}(}|))?$/;
            var emptyGuid = /^({|()?0{8}-(0{4}-){3}0{12}(}|))?$/;
            return validGuid.test(GUID) && !emptyGuid.test(GUID);
        }
    }

    app.controller('RouteDetailsController', RouteDetailsController);
}