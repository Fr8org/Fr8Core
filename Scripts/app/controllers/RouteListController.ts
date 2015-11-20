/// <reference path="../_all.ts" />

/*
    Detail (view/add/edit) controller
*/
module dockyard.controllers {
    'use strict';

    export interface IRouteListScope extends ng.IScope {
        executeProcessTemplate: (processTemplate: interfaces.IRouteVM) => void;
        goToProcessTemplatePage: (processTemplate: interfaces.IRouteVM) => void;
        goToProcessTemplateDetailsPage: (processTemplate: interfaces.IRouteVM) => void;
        deleteProcessTemplate: (processTemplate: interfaces.IRouteVM) => void;
        activateProcessTemplate: (processTemplate: interfaces.IRouteVM) => void;
        deactivateProcessTemplate: (processTemplate: interfaces.IRouteVM) => void;
        dtOptionsBuilder: any;
        dtColumnDefs: any;
        activeProcessTemplates: Array<interfaces.IRouteVM>;
        inActiveProcessTemplates: Array<interfaces.IRouteVM>;
    }

    /*
        List controller
    */
    class RouteListController {
        // $inject annotation.
        // It provides $injector with information about dependencies to be injected into constructor
        // it is better to have it close to the constructor, because the parameters must match in count and type.
        // See http://docs.angularjs.org/guide/di
        public static $inject = [
            '$scope',
            'RouteService',
            '$modal',
            'DTOptionsBuilder',
            'DTColumnDefBuilder',
            '$state'
        ];

        constructor(
            private $scope: IRouteListScope,
            private ProcessTemplateService: services.IRouteService,
            private $modal,
            private DTOptionsBuilder,
            private DTColumnDefBuilder,
            private $state: ng.ui.IStateService) {

            $scope.$on('$viewContentLoaded', () => {
                // initialize core components
                Metronic.initAjax();
            });

            //Load Process Templates view model
            $scope.dtOptionsBuilder = DTOptionsBuilder.newOptions().withPaginationType('full_numbers').withDisplayLength(10);   
            $scope.dtColumnDefs = this.getColumnDefs(); 
            $scope.activeProcessTemplates = ProcessTemplateService.getbystatus({ id: null, status: 2 });   
            $scope.inActiveProcessTemplates = ProcessTemplateService.getbystatus({ id: null, status: 1 });
            $scope.executeProcessTemplate = <(processTemplate: interfaces.IRouteVM) => void> angular.bind(this, this.executeProcessTemplate);
            $scope.goToProcessTemplatePage = <(processTemplate: interfaces.IRouteVM) => void> angular.bind(this, this.goToProcessTemplatePage);
            $scope.goToProcessTemplateDetailsPage = <(processTemplate: interfaces.IRouteVM) => void>angular.bind(this, this.goToProcessTemplateDetailsPage);
            $scope.deleteProcessTemplate = <(processTemplate: interfaces.IRouteVM) => void> angular.bind(this, this.deleteProcessTemplate);
            $scope.activateProcessTemplate = <(processTemplate: interfaces.IRouteVM) => void> angular.bind(this, this.activateProcessTemplate);
            $scope.deactivateProcessTemplate = <(processTemplate: interfaces.IRouteVM) => void> angular.bind(this, this.deactivateProcessTemplate);
        }

        private getColumnDefs() {
            return [
                this.DTColumnDefBuilder.newColumnDef(0),
                this.DTColumnDefBuilder.newColumnDef(1),
                this.DTColumnDefBuilder.newColumnDef(2).notSortable(),
                this.DTColumnDefBuilder.newColumnDef(3).notSortable()
            ];
        }

        private activateProcessTemplate(processTemplate) {
            this.ProcessTemplateService.activate(processTemplate);
            location.reload();
        };
        private deactivateProcessTemplate(processTemplate) {
            this.ProcessTemplateService.deactivate(processTemplate);
            location.reload();
        };
        private executeProcessTemplate(processTemplateId, $event) {
			if ($event.ctrlKey) {
				this.$modal.open({
					animation: true,
					templateUrl: '/AngularTemplate/_AddPayloadModal',
					controller: 'PayloadFormController', resolve: { processTemplateId: processTemplateId  }
				});
			}
			else {
				this.ProcessTemplateService.execute({ id: processTemplateId }, null, null, null);
			}
        }

        private goToProcessTemplatePage(processTemplateId) {
            this.$state.go('processBuilder', { id: processTemplateId });
        }

        private goToProcessTemplateDetailsPage(processTemplateId) {
            this.$state.go('routeDetails', { id: processTemplateId });
        }

        private deleteProcessTemplate(processTemplateId: number, isActive: boolean) {
            //to save closure of our controller
            var self = this;
            this.$modal.open({
                animation: true,
                templateUrl: 'modalDeleteConfirmation',
                controller: 'RouteListController__DeleteConfirmation',

            }).result.then(() => {
                //Deletion confirmed
                this.ProcessTemplateService.delete({ id: processTemplateId }).$promise.then(() => {
                    var procTemplates = isActive ? self.$scope.activeProcessTemplates : self.$scope.inActiveProcessTemplates;
                    //now loop through our existing templates and remove from local memory
                    for (var i = 0; i < procTemplates.length; i++) {
                        if (procTemplates[i].id === processTemplateId) {
                            procTemplates.splice(i, 1);
                            break;
                        }
                    }
                });
            });
        }
    }
    app.controller('RouteListController', RouteListController);

    /*
        A simple controller for Delete confirmation dialog.
        Note: here goes a simple (not really a TypeScript) way to define a controller. 
        Not as a class but as a lambda function.
    */
    app.controller('RouteListController__DeleteConfirmation', ($scope: any, $modalInstance: any): void => {
        $scope.ok = () => {
            $modalInstance.close();
        };

        $scope.cancel = () => {
            $modalInstance.dismiss('cancel');
        };
    });
}