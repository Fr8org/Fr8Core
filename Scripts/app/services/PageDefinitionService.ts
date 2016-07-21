/// <reference path="../_all.ts" />

module dockyard.services {
    export interface IPageDefinitionService extends ng.resource.IResourceClass<interfaces.IPageDefinitionVM> {
        getAll: () => Array<interfaces.IPageDefinitionVM>,
        getPageDefinition: (id: { id: number; }) => any;
    }

    app.factory("PageDefinitionService", ["$resource", ($resource: ng.resource.IResourceService): IPageDefinitionService =>
        <IPageDefinitionService>$resource("/api/pagedefinitions", null, {
            getAll: {
                method: "GET",
                isArray: true,
                url: "/api/pagedefinitions"
            },
            getPageDefinition: {
                method: "GET",
                isArray: false,
                url: "/api/pagedefinitions?id=:id",
                params: {
                    id: "@id"
                }
            }
        })
    ]);
}