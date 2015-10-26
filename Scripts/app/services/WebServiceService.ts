
module dockyard.services {
	export interface IWebServiceService extends ng.resource.IResourceClass<interfaces.IWebServiceVM>, interfaces.IWebServiceVM {
		getAll: () => Array<interfaces.IWebServiceVM>;
	}

	app.factory('WebServiceService', ['$resource', ($resource: ng.resource.IResourceService): IWebServiceService =>
		<IWebServiceService> $resource('api/webservices/:id', { id: '@id' }, {
			getAll: {
				method: 'GET',
				isArray: true,
				url: 'api/webservices'
			}
		})
	]);
}