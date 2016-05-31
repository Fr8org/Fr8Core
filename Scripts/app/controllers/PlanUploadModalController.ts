/// <reference path="../_all.ts" />

module dockyard.controllers {
    'use strict';

    export interface IPlanUploadModalScope extends ng.IScope {
        plan: interfaces.IPlanFullDTO;
        href: string;

        loadPlan: ()=> void;
        download: ($event: Event) => void;
        cancel: () => void;
    }


    class PlanUploadModalController {
    // $inject annotation.
    // It provides $injector with information about dependencies to be injected into constructor
    // it is better to have it close to the constructor, because the parameters must match in count and type.
    // See http://docs.angularjs.org/guide/di
    public static $inject = [
        '$scope',
        '$modalInstance',
        '$filter',
        'PlanService'
    ];

    constructor(
        private $scope: IPlanUploadModalScope,
        private $modalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private $filter:ng.IFilterService,
        private PlanService: services.IPlanService
    ) {
        
        //$scope.plan = plan;


        $scope.loadPlan = () => {
            
        };
       
        $scope.cancel = () => { $modalInstance.dismiss(); }

       }
    }

    app.controller('PlanUploadModalController', PlanUploadModalController);
}