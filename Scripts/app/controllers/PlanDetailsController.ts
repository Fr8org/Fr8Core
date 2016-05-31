/// <reference path="../_all.ts" />

module dockyard.controllers {
    'use strict';

    export interface IPlanDetailsScope extends ng.IScope {
        ptvm: interfaces.IPlanFullDTO;
        submit: (isValid: boolean) => void;
        errorMessage: string;
        planBuilder: any,
        id: string,
        href: string,

        download: ($event: Event) => void;
    }

    class PlanDetailsController {
        // $inject annotation.
        // It provides $injector with information about dependencies to be injected into constructor
        // it is better to have it close to the constructor, because the parameters must match in count and type.
        // See http://docs.angularjs.org/guide/di
        public static $inject = [
            '$rootScope',
            '$scope',
            'PlanService',
            '$stateParams',
            "$filter"
        ];

        constructor(
            private $rootScope: interfaces.IAppRootScope,
            private $scope: IPlanDetailsScope,
            private PlanService: services.IPlanService,
            private $stateParams: any,
            private $filter: ng.IFilterService ) {
            
            //Load detailed information
            $scope.id = $stateParams.id;
            if (this.isValidGUID($scope.id)) {
                $scope.ptvm = PlanService.getFull({ id: $stateParams.id });
            }

            $scope.download = ($event: Event) => {

                debugger;

                let json = $filter('json')($scope.ptvm);
                let data = new Blob([json]);
                $scope.href = URL.createObjectURL(data);

                $event.stopPropagation = null;
                $event.preventDefault = ()=>{};
                $event.cancelBubble = false;
                $event.returnValue = true;
            };
        }

        // Regular Expression reference link
        // https://lostechies.com/gabrielschenker/2009/03/10/how-to-add-a-custom-validation-method-to-the-jquery-validator-plug-in/
        private isValidGUID(GUID) {
            var validGuid = /^({|()?[0-9a-fA-F]{8}-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}(}|))?$/;
            var emptyGuid = /^({|()?0{8}-(0{4}-){3}0{12}(}|))?$/;
            return validGuid.test(GUID) && !emptyGuid.test(GUID);
        }
    }

    app.controller('PlanDetailsController', PlanDetailsController);
}