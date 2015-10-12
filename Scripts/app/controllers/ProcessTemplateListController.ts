/// <reference path="../_all.ts" />

/*
    Detail (view/add/edit) controller
*/
module dockyard.controllers {
    'use strict';

    export interface IProcessTemplateListScope extends ng.IScope {
        executeProcessTemplate: (processTemplate: interfaces.IProcessTemplateVM) => void;
        goToProcessTemplatePage: (processTemplate: interfaces.IProcessTemplateVM) => void;
        deleteProcessTemplate: (processTemplate: interfaces.IProcessTemplateVM) => void;
        dtOptionsBuilder: any;
        dtColumnDefs: any;
        /*
        activeProcessTemplates: Array<interfaces.IProcessTemplateVM>;
        inActiveProcessTemplates: Array<interfaces.IProcessTemplateVM>;
        */
        processTemplates: Array<interfaces.IProcessTemplateVM>;
    }

    /*
        List controller
    */
    class ProcessTemplateListController {
        // $inject annotation.
        // It provides $injector with information about dependencies to be injected into constructor
        // it is better to have it close to the constructor, because the parameters must match in count and type.
        // See http://docs.angularjs.org/guide/di
        public static $inject = [
            '$scope',
            'ProcessTemplateService',
            '$modal',
            'DTOptionsBuilder',
            'DTColumnDefBuilder',
            '$state'
        ];

        constructor(
            private $scope: IProcessTemplateListScope,
            private ProcessTemplateService: services.IProcessTemplateService,
            private $modal,
            private DTOptionsBuilder,
            private DTColumnDefBuilder,
            private $state) {

            $scope.$on('$viewContentLoaded', () => {
                // initialize core components
                Metronic.initAjax();
            });

            //Load Process Templates view model
           
            $scope.processTemplates = ProcessTemplateService.query();
            $scope.dtOptionsBuilder = DTOptionsBuilder.newOptions().withPaginationType('full_numbers').withDisplayLength(10);   
            $scope.dtColumnDefs = this.getColumnDefs(); 
            /*
            $scope.activeProcessTemplates = ProcessTemplateService.getbystatus({ id: null, status: 2 });   
            $scope.inActiveProcessTemplates = ProcessTemplateService.getbystatus({ id: null, status: 1 });
            */
            $scope.executeProcessTemplate = <(processTemplate: interfaces.IProcessTemplateVM) => void> angular.bind(this, this.executeProcessTemplate);
            $scope.goToProcessTemplatePage = <(processTemplate: interfaces.IProcessTemplateVM) => void> angular.bind(this, this.goToProcessTemplatePage);
            $scope.deleteProcessTemplate = <(processTemplate: interfaces.IProcessTemplateVM) => void> angular.bind(this, this.deleteProcessTemplate);
        }

        private getColumnDefs() {
            return [
                this.DTColumnDefBuilder.newColumnDef(0),
                this.DTColumnDefBuilder.newColumnDef(1),
                this.DTColumnDefBuilder.newColumnDef(2).notSortable(),
                this.DTColumnDefBuilder.newColumnDef(3).notSortable()
            ];
        }

        private executeProcessTemplate(processTemplateId, $event) {
            this.ProcessTemplateService.execute({ id: processTemplateId });
        }

        private goToProcessTemplatePage(processTemplateId) {
            this.$state.go('processBuilder', { id: processTemplateId });
        }

        private deleteProcessTemplate(processTemplateId) {
            //to save closure of our controller
            this.$modal.open({
                animation: true,
                templateUrl: 'modalDeleteConfirmation',
                controller: 'ProcessTemplateListController__DeleteConfirmation',

            }).result.then(() => {
                //Deletion confirmed
                this.ProcessTemplateService.delete({ id: processTemplateId }).$promise.then(function () {
                    //now loop through our existing templates and remove from local memory
                    for (var i = 0; i < this.$scope.processTemplates.length; i++) {
                        if (this.$scope.processTemplates[i].id === processTemplateId) {
                            this.$scope.processTemplates.splice(i, 1);
                            break;
                        }
                    }
                });
            });
        }
    }
    app.controller('ProcessTemplateListController', ProcessTemplateListController);

    /*
        A simple controller for Delete confirmation dialog.
        Note: here goes a simple (not really a TypeScript) way to define a controller. 
        Not as a class but as a lambda function.
    */
    app.controller('ProcessTemplateListController__DeleteConfirmation', ($scope: any, $modalInstance: any): void => {
        $scope.ok = () => {
            $modalInstance.close();
        };

        $scope.cancel = () => {
            $modalInstance.dismiss('cancel');
        };
    });
}