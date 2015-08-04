/// <reference path="../_all.ts" />

/*
    Detail (view/add/edit) controller
*/
module dockyard.controllers {
    'use strict';

    class ProcessBuilderController {
        // $inject annotation.
        // It provides $injector with information about dependencies to be injected into constructor
        // it is better to have it close to the constructor, because the parameters must match in count and type.
        // See http://docs.angularjs.org/guide/di

        private pbAddCriteriaClick: () => void;
        private pbCriteriaClick: (criteriaId: number) => void;
        private pbAddActionClick: (criteriaId: number) => void;
        private pbActionClick: (criteriaId: number, actionId: number) => void;

        public static $inject = [
            '$rootScope',
            'StringService'
        ];
        constructor(
            private $rootScope: interfaces.IAppRootScope,
            private StringService: services.IStringService) {

            // BEGIN ProcessBuilder event handlers.

            var criteriaIdSeq = 0;
            var actionIdSeq = 0;

            this.pbAddCriteriaClick = function () {
                this.processBuilder.addCriteria({ id: ++criteriaIdSeq })
            };

            this.pbCriteriaClick = function (criteriaId) {
                this.processBuilder.removeCriteria(criteriaId);
            };

            this.pbAddActionClick = function (criteriaId) {
                this.processBuilder.addAction(criteriaId, { id: ++actionIdSeq });
            };

            this.pbActionClick = function (criteriaId, actionId) {
                this.processBuilder.removeAction(criteriaId, actionId);
            };

            // END ProcessBuilder event handlers.
        }
    }
    app.controller('ProcessBuilderController', ProcessBuilderController);
} 