/// <reference path="../_all.ts" />

module dockyard.controllers {
	'use strict';

	class PayloadFormController {

		public static $inject = [
			'$scope',
			'PlanService',
			'planId',
			'$modalInstance'
		];

		constructor(
			private $scope: any,
			private PlanService: services.IPlanService,
			private planId: any,
			private $modalInstance: any) {

			$scope.submit = <() => void> angular.bind(this, this.submit);
			$scope.close = <() => void> angular.bind(this, this.close);

			$scope.payload = "";
		}

		private submit(form) {
            this.PlanService
                .runAndProcessClientAction(this.planId)
                .then((successResponse) => {
                    this.$modalInstance.close();
                })
				.catch((errorResponse) => {
					form.payload.$error.invalid = true;
				});
		}

		private close() {
			this.$modalInstance.dismiss('cancel');
		}
	}

	app.controller('PayloadFormController', PayloadFormController);
}