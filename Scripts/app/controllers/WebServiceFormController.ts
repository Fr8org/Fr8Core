/// <reference path="../_all.ts" />

module dockyard.controllers {
	'use strict';

	export interface IWebServiceFormScope extends ng.IScope {
		webService: interfaces.IWebServiceVM;
		submit: () => void;
		cancel: () => void;
	}

	class WebServiceFormController {

		// $inject annotation.
		// It provides $injector with information about dependencies to be injected into constructor
		// it is better to have it close to the constructor, because the parameters must match in count and type.
		// See http://docs.angularjs.org/guide/di
		public static $inject = [
			'$scope',
			'WebServiceService',
			'$modalInstance'
		];

		constructor(
			private $scope: IWebServiceFormScope,
			private WebServiceService: services.IWebServiceService,
			private $modalInstance: any) {

			$scope.submit = <() => void> angular.bind(this, this.sumbitForm);
			$scope.cancel = <() => void> angular.bind(this, this.cancelForm);
		}

		private sumbitForm() {
			this.WebServiceService.save(this.$scope.webService).$promise.then(webService => {
				this.$modalInstance.close(webService);
			});
		}

		private cancelForm() {
			this.$modalInstance.dismiss('cancel');
		}
	}

	app.controller('WebServiceFormController', WebServiceFormController);
}