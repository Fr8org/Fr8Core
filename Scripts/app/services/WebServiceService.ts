
module dockyard.services {
    export interface IWebServiceService extends ng.resource.IResourceClass<interfaces.IWebServiceVM> {
        getActivities: (id: any) => Array<model.WebServiceActionSetDTO>
    }

    app.factory("WebServiceService", ["$resource", ($resource: ng.resource.IResourceService): IWebServiceService =>
        <IWebServiceService>$resource("/api/webservices/:id",
            {
                id: '@id'

            },
            {
                getActivities:
                {
                    method: "GET",
                    isArray: true,
                    url: "/api/webservices/",
                    params: {
                        id: "@id"
                    }
                }
            })
    ]);
}