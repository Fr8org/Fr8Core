/// <reference path="../_all.ts" />

module dockyard.controllers {
    'use strict';

    export interface IPlanTemplatesModalScope extends ng.IScope {

    }


    class PlanListController {
    // $inject annotation.
    // It provides $injector with information about dependencies to be injected into constructor
    // it is better to have it close to the constructor, because the parameters must match in count and type.
    // See http://docs.angularjs.org/guide/di
    public static $inject = [
        '$scope'
    ];

    constructor(
        private $scope: IPlanTemplatesModalScope
    ) {}

    }
}