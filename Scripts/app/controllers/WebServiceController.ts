/// <reference path="../_all.ts" />

module dockyard.controllers {	
	'use strict';

	export interface IWebServiceScope extends ng.IScope {
		webServices: Array<interfaces.IWebServiceVM>;
	}

	class WebServiceController {

		// $inject annotation.
		// It provides $injector with information about dependencies to be injected into constructor
		// it is better to have it close to the constructor, because the parameters must match in count and type.
		// See http://docs.angularjs.org/guide/di
		public static $inject = [
			'$scope',
			'WebServiceService',
			'$state'
		];

		constructor(
			private $scope: IWebServiceScope,
			private WebServiceService: services.IWebServiceService) {

			//$scope.webServices = WebServiceService.getAll();

			WebServiceService.getAll().$promise.then(function (data) {
				$scope.webServices = data;
			});

			//$scope.webServices = this.wsMock();
		}

		private wsMock() {
			return [
				new model.WebServiceDTO(1, "AWS", "/Content/icons/aws-icon-64x64.png"),
				new model.WebServiceDTO(2, "Slack", "/Content/icons/slack-icon-64x64.png"),
				new model.WebServiceDTO(3, "Docusign", "/Content/icons/docusign-icon-64x64.png")
			];
		}
	}

	app.controller('WebServiceController', WebServiceController);
}