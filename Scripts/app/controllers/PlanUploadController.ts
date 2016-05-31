/// <reference path="../_all.ts" />

module dockyard.controllers {
    'use strict';

    export interface IPlanUploadScope extends ng.IScope {
        showModal: () => void;
    }


    class PlanUploadController {
        // $inject annotation.
        // It provides $injector with information about dependencies to be injected into constructor
        // it is better to have it close to the constructor, because the parameters must match in count and type.
        // See http://docs.angularjs.org/guide/di
        public static $inject = [
            '$scope',
            '$modal',
            'Upload'
        ];

        constructor(
            private $scope: IPlanUploadScope,
            private $modal,
            private Upload
        ) {

            $scope.showModal = () => {
                let modalInstance = this.$modal.open({
                    animation: true,
                    templateUrl: '/AngularTemplate/PlanUploadModal',
                    controller: 'PlanUploadModalController',
                    resolve: {
                        
                    }
                });    
            };

        }
    }

    app.controller('PlanUploadController', PlanUploadController);
}