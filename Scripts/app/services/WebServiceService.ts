
module dockyard.services {
	export interface IWebServiceService extends ng.resource.IResourceClass<interfaces.IWebServiceVM> {
		getActions: (params: Array<number>) => Array<model.WebServiceActionSetDTO>
	}

	app.factory("WebServiceService", ["$resource", ($resource: ng.resource.IResourceService): IWebServiceService =>
        <IWebServiceService>$resource("/api/webservices?id=:id", { id: "@id" }, {
			getActions: {
				method: "POST",
				isArray: true,
                url: "/api/webservices/actions"
			}
		})
	]);
}