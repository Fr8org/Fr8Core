/// <reference path="../_all.ts" />

module dockyard.controllers {
    'use strict';

    export interface IPlanTemplatesModalScope extends ng.IScope {
        plan: interfaces.IPlanFullDTO;
        href: string;

        publish: ()=> void;
        cancel: () => void;
    }


    class PlanTemplateModalController {
    // $inject annotation.
    // It provides $injector with information about dependencies to be injected into constructor
    // it is better to have it close to the constructor, because the parameters must match in count and type.
    // See http://docs.angularjs.org/guide/di
    public static $inject = [
        '$scope',
        '$modalInstance',
        '$filter',
        'PlanService',
        'plan'
    ];

    constructor(
        private $scope: IPlanTemplatesModalScope,
        private $modalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private $filter:ng.IFilterService,
        private PlanService: services.IPlanService,
        private plan: interfaces.IPlanFullDTO
    ) {
        
        $scope.plan = plan;

        $scope.publish = () => { };
        $scope.cancel = () => { $modalInstance.dismiss(); }

        debugger;

        //if network olatency to high plan will be empty!
        
        let json = $filter('json')($scope.plan);
        let data = new Blob([json]);
        $scope.href = URL.createObjectURL(data);

       }
    }

    app.controller('PlanTemplateModalController', PlanTemplateModalController);
}