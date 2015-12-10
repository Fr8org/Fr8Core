/// <reference path="../_all.ts" />

module dockyard.controllers {
	'use strict';

	export interface IRouteActionsDialogScope extends ng.IScope {
		webServiceActionList: Array<model.WebServiceActionSetDTO>;
		actionCategories: any;
		toggleFilter: () => void;
		selectAction: (activityType: interfaces.IActivityTemplateVM) => void;
		close: () => void;
	}

	class RouteActionsDialogController {

		public static $inject = [
			'$scope',
			'WebServiceService',
			'$modalInstance'
		];

		constructor(
			private $scope: IRouteActionsDialogScope,
			private WebServiceService: services.IWebServiceService,
			private $modalInstance: any) {

			$scope.toggleFilter = <() => void> angular.bind(this, this.toggleFilter);
			$scope.selectAction = <() => void> angular.bind(this, this.selectAction);
			$scope.close = <() => void> angular.bind(this, this.close);

			$scope.actionCategories = [
				{ id: 1, name: "Monitors", description: "Learn when something happen", checked: true },
				{ id: 2, name: "Receivers", description: "In-process Crates from a web service", checked: true },
				{ id: 3, name: "Processors", description: "Carry out work on a Container", checked: true },
                { id: 4, name: "Forwarders", description: "Send Crates to a web service", checked: true }
			];

			this.toggleFilter();
		}

		private toggleFilter() {
			var categories = [];

			angular.forEach(this.$scope.actionCategories, (value) => {
				if (value.checked) {
					categories.push(value.id);
				}
			});

			this.$scope.webServiceActionList = this.WebServiceService.getActions(categories);
		}

		private selectAction(activityType: interfaces.IActivityTemplateVM) {
			this.$modalInstance.close(activityType);
		}

		private close() {
			this.$modalInstance.dismiss('cancel');
		}
	}

	app.controller('RouteActionsDialogController', RouteActionsDialogController);
}