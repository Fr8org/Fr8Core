/// <reference path="../_all.ts" />

module dockyard.controllers {	
	'use strict';

	export interface IWebServiceListScope extends ng.IScope {
		webServices: Array<interfaces.IWebServiceVM>;
		showAddWebServiceModal: () => void;
	}

	class WebServiceListController {

		// $inject annotation.
		// It provides $injector with information about dependencies to be injected into constructor
		// it is better to have it close to the constructor, because the parameters must match in count and type.
		// See http://docs.angularjs.org/guide/di
		public static $inject = [
			'$scope',
			'WebServiceService',
			'$modal'
		];

		constructor(
			private $scope: IWebServiceListScope,
			private WebServiceService: services.IWebServiceService,
			private $modal: any) {

			$scope.showAddWebServiceModal = <() => void> angular.bind(this, this.showAddWebServiceModal);

			WebServiceService.query().$promise.then(function (data) {
				$scope.webServices = data;
			});
		}

		private showAddWebServiceModal() {
			var me = this;

			this.$modal.open({
				animation: true,
				templateUrl: 'webServiceFormModal',
				controller: 'WebServiceFormController'
			})
			.result.then(function (webService) {
				me.$scope.webServices.push(webService);
			});
		}
	}

	app.controller('WebServiceListController', WebServiceListController);
}