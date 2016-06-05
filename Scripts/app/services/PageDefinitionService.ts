/// <reference path="../_all.ts" />

module dockyard.services {
    export interface IPageDefinitionService extends ng.resource.IResourceClass<interfaces.IPageDefinitionVM> {
        getAll: () => Array<interfaces.IPageDefinitionVM>,
        getPageDefinition: (title: { title: string; }) => any;
    }

    app.factory("PageDefinitionService", ["$resource", ($resource: ng.resource.IResourceService): IPageDefinitionService =>
        <IPageDefinitionService>$resource("/api/pagedefinitions", {}, {
            getAll: {
                method: "GET",
                isArray: true,
                url: "/api/pagedefinitions"
            },
            getPageDefinition: {
                method: "GET",
                isArray: false,
                url: "/api/pagedefinitions?title=:title",
                params: {
                    title: "@title"
                }
            }
        })
    ]);
}