/// <reference path="../_all.ts" />

module dockyard.controllers {
    'use strict';

    export interface ISolutionScope extends ng.IScope {
        activityCategories: ng.resource.IResource<interfaces.IActivityCategoryDTO[]>;
        onSolutionSelected: (solution: interfaces.IActivityCategoryDTO) => void;
    }

    export class SolutionController {

        public static $inject = [
            '$scope',
            'SolutionService',
            '$state',
            '$stateParams'
        ];
        constructor(
            private $scope: ISolutionScope,
            private SolutionService: services.ISolutionService,
            private $state: ng.ui.IStateService,
            private $stateParams: ng.ui.IStateParamsService) {

            $scope.onSolutionSelected = <(solution: interfaces.IActivityCategoryDTO) => void>
                angular.bind(this, this.onSolutionSelected);

            var solutionName = $stateParams["solutionName"];
            var route = SolutionService.create({
                solutionName: solutionName
            });
            route.$promise.then((result) => console.log(result));            
        }

        private onSolutionSelected(solution: interfaces.IActivityTemplateVM) {
        }
    }

    app.controller('SolutionController', SolutionController);
} 