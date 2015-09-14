/// <reference path="../_all.ts" />

module dockyard.controllers {
    'use strict';

    export interface IProcessTemplateDetailsScope extends ng.IScope {
        ptvm: interfaces.IProcessTemplateVM;
        submit: (isValid: boolean) => void;
        errorMessage: string;
        processBuilder: any
    }

    class ProcessTemplateDetailsController {
        // $inject annotation.
        // It provides $injector with information about dependencies to be injected into constructor
        // it is better to have it close to the constructor, because the parameters must match in count and type.
        // See http://docs.angularjs.org/guide/di
        public static $inject = [
            '$rootScope',
            '$scope',
            'ProcessTemplateService',
            '$stateParams'
        ];

        constructor(
            private $rootScope: interfaces.IAppRootScope,
            private $scope: IProcessTemplateDetailsScope,
            private ProcessTemplateService: services.IProcessTemplateService,
            private $stateParams: any) {
            
            //Load detailed information
            var id: string = $stateParams.id;
            if (/^[0-9]+$/.test(id) && parseInt(id) > 0) {
                $scope.ptvm = ProcessTemplateService.get({ id: $stateParams.id });
            }
        }
    }

    app.controller('ProcessTemplateDetailsController', ProcessTemplateDetailsController);
}