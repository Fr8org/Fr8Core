/// <reference path="../_all.ts" />

module dockyard.controllers {
	'use strict';

	class PayloadFormController {

		public static $inject = [
			'$scope',
			'RouteService',
			'processTemplateId',
			'$modalInstance'
		];

		constructor(
			private $scope: any,
			private ProcessTemplateService: services.IRouteService,
			private processTemplateId: any,
			private $modalInstance: any) {

			$scope.submit = <() => void> angular.bind(this, this.submit);
			$scope.close = <() => void> angular.bind(this, this.close);

			$scope.payload = "";
			$scope.error = "";
		}

		private submit(form) {
			this.ProcessTemplateService
				.execute({ id: this.processTemplateId }, { payload: form.payload.$modelValue },
				(successResponse) => {
					this.$modalInstance.close();
				},
				(errorResponse) => {
					form.payload.$error.invalid = true;
					this.$scope.error = errorResponse.data.message;
				});
		}

		private close() {
			this.$modalInstance.dismiss('cancel');
		}
	}

	app.controller('PayloadFormController', PayloadFormController);
}