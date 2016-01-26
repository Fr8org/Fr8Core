/// <reference path="../_all.ts" />

module dockyard.controllers {
	'use strict';

	class PayloadFormController {

		public static $inject = [
			'$scope',
			'RouteService',
			'routeId',
			'$modalInstance'
		];

		constructor(
			private $scope: any,
			private RouteService: services.IRouteService,
			private routeId: any,
			private $modalInstance: any) {

			$scope.submit = <() => void> angular.bind(this, this.submit);
			$scope.close = <() => void> angular.bind(this, this.close);

			$scope.payload = "";
		}

		private submit(form) {
            this.RouteService
                .runAndProcessClientAction(this.routeId)
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