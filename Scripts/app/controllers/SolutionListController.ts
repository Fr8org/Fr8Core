/// <reference path="../_all.ts" />

module dockyard.controllers {
    'use strict';

    export interface ISelectActionScope extends ng.IScope {
        activityCategories: ng.resource.IResource<interfaces.IActivityCategoryDTO[]>;
        onSolutionSelected: (solution: interfaces.IActivityCategoryDTO) => void;
    }

    export class SolutionListController {

        public static $inject = [
            '$scope',
            'ActivityTemplateService',
            '$state'
        ];
        constructor(
            private $scope: ISelectActionScope,
            private ActivityTemplateService: services.IActivityTemplateService,
            private ActionService: services.IActionService,
            private $state: ng.ui.IStateService) {

            $scope.onSolutionSelected = <(solution: interfaces.IActivityCategoryDTO) => void>
                angular.bind(this, this.onSolutionSelected);

            $scope.activityCategories = ActivityTemplateService.getSolutions();
        }

        private onSolutionSelected(solution: interfaces.IActivityTemplateVM) {
            this.$state.go('configureSolution', { id: solution.id });
        }
    }

    app.controller('SolutionListController', SolutionListController);
}