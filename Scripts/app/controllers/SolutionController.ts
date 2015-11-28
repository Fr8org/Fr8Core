/// <reference path="../_all.ts" />

module dockyard.controllers {
    'use strict';
    import pca = dockyard.directives.paneConfigureAction;

    export interface ISolutionScope extends ng.IScope {
        action: model.ActionDTO;
        route: model.RouteDTO;
        onSolutionSelected: (solution: interfaces.IActivityCategoryDTO) => void;
        processing: boolean;
    }

    export class SolutionController {

        public static $inject = [
            '$scope',
            'ActionService',
            '$state',
            '$stateParams',
            'RouteService'
        ];
        constructor(
            private $scope: ISolutionScope,
            private ActionService: services.IActionService,
            private $state: ng.ui.IStateService,
            private $stateParams: ng.ui.IStateParamsService,
            private RouteService: services.IRouteService) {

            $scope.$on(pca.MessageType[pca.MessageType.PaneConfigureAction_ChildActionsDetected], () => {
                this.onChildActionsDetected();
            });

            $scope.action = new model.ActionDTO('0', '0', false);

            var solutionNameOrId = $stateParams["solutionName"];
            if (!solutionNameOrId) {
                alert('No solution name is specified');
                return;
            }

            // If an positive integer is supplied, consider the value actionId
            // Otherwise consider the value solution name
            if (/[0-9]+$/.test(solutionNameOrId))
                this.editExistingSolution(<string>solutionNameOrId, $scope);
            else
                this.createNewSolution(solutionNameOrId, $scope);
        }

        private editExistingSolution(actionId: string, $scope: ISolutionScope) {
            if ($scope.action != null && $scope.action.id === actionId) {
                // don't need to load the action 
                $scope.action.childrenActions = [];
                $scope.$broadcast(pca.MessageType[pca.MessageType.PaneConfigureAction_RenderConfiguration]);
            }
            else {
                $scope.processing = true;
                var actionDeffered = this.ActionService.get({ id: actionId });
                actionDeffered.$promise.then((action) => {
                    $scope.action = action;
                    $scope.action.childrenActions = [];
                    var routeDeffered = this.RouteService.getByAction({ id: action.id });
                    routeDeffered.$promise.then(route => {
                        $scope.route = route;
                        $scope.$broadcast(pca.MessageType[pca.MessageType.PaneConfigureAction_RenderConfiguration]);
                    }).finally(() => {
                        $scope.processing = false;
                    });
                }).finally(() => {
                    $scope.processing = false;
                });
            }
        }

        private createNewSolution(solutionName: string, $scope: ISolutionScope) {
            $scope.processing = true;
            var route = this.ActionService.createSolution({
                solutionName: solutionName
            });
            route.$promise.then((result) => {
                console.log(result);
                $scope.route = result;
                $scope.action = result.subroutes[0].actions[0]; // get the only first action
                $scope.$broadcast(pca.MessageType[pca.MessageType.PaneConfigureAction_RenderConfiguration]);
            })
                .finally(() => {
                    $scope.processing = false;
                });

        }

        private onChildActionsDetected() {
            debugger;
            this.$state.transitionTo('routeBuilder', { id: this.$scope.route.id });
        }

    }

    app.controller('SolutionController', SolutionController);
} 