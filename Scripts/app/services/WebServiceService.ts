
module dockyard.services {
	export interface IWebServiceService extends ng.resource.IResourceClass<interfaces.IWebServiceVM> {
        getActivities: (params: Array<number>) => Array<model.WebServiceActionSetDTO>
	}

	app.factory("WebServiceService", ["$resource", ($resource: ng.resource.IResourceService): IWebServiceService =>
        <IWebServiceService>$resource("/api/webservices?id=:id", { id: "@id" }, {
            getActivities: {
				method: "POST",
				isArray: true,
                url: "/api/webservices/activities"
			}
		})
	]);
}