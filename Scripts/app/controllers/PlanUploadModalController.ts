/// <reference path="../_all.ts" />

module dockyard.controllers {
    'use strict';

    export interface IPlanUploadModalScope extends ng.IScope {
        planName: string;
        planFile: any;
        fileSelected: boolean;
        fileName: string;
        uploadFlag: boolean;

        loadPlan: (file:any)=> void;
        cancel: () => void;
        fileChanged: (input: any)=>void;
    }


    class PlanUploadModalController {
    // $inject annotation.
    // It provides $injector with information about dependencies to be injected into constructor
    // it is better to have it close to the constructor, because the parameters must match in count and type.
    // See http://docs.angularjs.org/guide/di
    public static $inject = [
        '$scope',
        '$modalInstance',
        'Upload',
        '$state'
    ];

    constructor(
        private $scope: IPlanUploadModalScope,
        private $modalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private Upload,
        private $state: ng.ui.IStateService
    ) {

        $scope.fileChanged = () => {
            if ($scope.planFile) {
                $scope.fileSelected = true;
                $scope.fileName = $scope.planFile.name;
            }
        }
        
        $scope.loadPlan = (file) => {

            $scope.uploadFlag = true;
            Upload.upload({
                url: '/api/plans/upload?planName=' + $scope.planName,
                file: file 
            }).then((response) => {
                
                if ($state.current.name === "planList") {
                    $state.reload();
                } else {
                    $state.go("planList");    
                }
                    $scope.uploadFlag = false;
                    $modalInstance.dismiss();
                }, (error) => {

                    $modalInstance.dismiss();
                    $scope.uploadFlag = false;  
            });
            
        };
       
        $scope.cancel = () => { $modalInstance.dismiss(); }

       }
    }

    app.controller('PlanUploadModalController', PlanUploadModalController);
}