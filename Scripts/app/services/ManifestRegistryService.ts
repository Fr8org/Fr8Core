
module dockyard.services {
    export interface IManifestRegistryService extends ng.resource.IResourceClass<interfaces.IManifestRegistryVM> {
        checkVersionAndName: (versionAndName: string, userAccountId: string) => boolean
    }

    app.factory("ManifestRegistryService", ["$resource", ($resource: ng.resource.IResourceService): IManifestRegistryService =>
        <IManifestRegistryService>$resource("/api/manifestregistry?id=:id", { id: "@id" }, {
            query: {
                method: 'GET',
                params: { userAccountId: "" },
                isArray: true
            },
            checkVersionAndName: {
                method: 'GET',
                params: {
                    versionAndName: "",
                    userAccountId: ""
                },
                isArray: false,
                url: "/api/manifestregistry/checkVersionAndName"
            }
         })
     ]);
}