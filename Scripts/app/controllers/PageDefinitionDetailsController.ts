/// <reference path="../_all.ts" />

module dockyard.controllers {
    "use strict";

    export interface IPageDefinitionDetailsScope extends ng.IScope {
        pd: interfaces.IPageDefinitionVM;
        submit: (isValid: boolean) => void;
        cancel: () => void;
    }

    class PageDefinitionDetailsController {
        // $inject annotation.
        // It provides $injector with information about dependencies to be injected into constructor
        // it is better to have it close to the constructor, because the parameters must match in count and type.
        // See http://docs.angularjs.org/guide/di
        public static $inject = [
            "$scope",
            "$state",
            "PageDefinitionService"
        ];

        constructor(
            private $scope: IPageDefinitionDetailsScope,
            private $state: ng.ui.IStateService,
            private PageDefinitionService: services.IPageDefinitionService) {
       
            PageDefinitionService.getDetails({ id: 0 }).$promise.then(data => {
                $scope.pd = data;
            });

            //Save button
            //$scope.submit = function (isValid) {
            //    if (isValid) {
            //        var result = PageDefinitionService.up($scope.user);
            //        result.$promise.then(() => { $state.go('accounts'); });
            //    }
            //};

            $scope.cancel = function () {
                $state.go('pageDefinitions');
            };
        }
    }

    app.controller("PageDefinitionDetailsController", PageDefinitionDetailsController);
}