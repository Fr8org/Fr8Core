
module dockyard.services {
	export interface IWebServiceService extends ng.resource.IResourceClass<interfaces.IWebServiceVM> { }

	app.factory('WebServiceService', ['$resource', ($resource: ng.resource.IResourceService): IWebServiceService =>
		<IWebServiceService> $resource('api/webservices/:id', { id: '@id' }, { })
	]);
}