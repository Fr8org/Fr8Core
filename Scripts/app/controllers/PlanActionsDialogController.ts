/// <reference path="../_all.ts" />

module dockyard.controllers {
    'use strict';

    export interface IPlanActionsDialogScope extends ng.IScope {
        webServiceActionList: Array<model.WebServiceActionSetDTO>;
        actionCategories: any;
        activeTerminal: any;
        setActive: () => void;
        setActiveTerminal: () => void;
        selectAction: (activityType: interfaces.IActivityTemplateVM) => void;
        //close: () => void;
    }

    class PlanActionsDialogController {

        public static $inject = [
            '$scope',
            'WebServiceService',
            //'$modalInstance'
        ];

        constructor(
            private $scope: IPlanActionsDialogScope,
            private WebServiceService: services.IWebServiceService
            //private $modalInstance: any
        ) {

            $scope.setActive = <() => void>angular.bind(this, this.setActive);
            $scope.setActiveTerminal = <() => void>angular.bind(this, this.setActiveTerminal);
            $scope.selectAction = <() => void>angular.bind(this, this.selectAction);
            //$scope.close = <() => void> angular.bind(this, this.close);

            $scope.actionCategories = [
                { id: 1, name: "Monitor", description: "Learn when something happen", icon: "eye" },
                { id: 2, name: "Get", description: "In-process Crates from a web service", icon: "download" },
                { id: 3, name: "Process", description: "Carry out work on a Container", icon: "recycle" },
                { id: 4, name: "Forward", description: "Send Crates to a web service", icon: "share" }
            ];
            $scope.activeTerminal = 1;
        }
        private setActive(actionCategoryId) {
            console.log(this.WebServiceService.getActivities([actionCategoryId]))
            this.$scope.webServiceActionList = this.WebServiceService.getActivities([actionCategoryId]);
        }

        private setActiveTerminal(index) {
            this.$scope.activeTerminal = index
            console.log(this.$scope.activeTerminal)
        }

        private selectAction(activityType: interfaces.IActivityTemplateVM) {
            //this.$modalInstance.close(activityType);
        }

        private close() {
            //this.$modalInstance.dismiss('cancel');
        }
    }

    app.controller('PlanActionsDialogController', PlanActionsDialogController);
}