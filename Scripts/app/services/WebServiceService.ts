
module dockyard.services {
	export interface IWebServiceService extends ng.resource.IResourceClass<interfaces.IWebServiceVM> {
		getActions: (params: Array<number>) => Array<model.WebServiceActionSetDTO>
	}

	app.factory("WebServiceService", ["$resource", ($resource: ng.resource.IResourceService): IWebServiceService =>
		<IWebServiceService> $resource("webservices/:id", { id: "@id" }, {
			getActions: {
				method: "POST",
				isArray: true,
				url: "webservices/actions"
			}
		})
	]);
}