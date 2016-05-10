
module dockyard.services {
    export interface IManifestRegistryService extends ng.resource.IResourceClass<interfaces.IManifestRegistryVM> {
        checkVersionAndName: (version: { version: string }, name: { name: string }) => any;
        getDescriptionWithLastVersion: (name: { name: string }) => any;
    }

    app.factory("ManifestRegistryService", ["$resource", ($resource: ng.resource.IResourceService): IManifestRegistryService =>
        <IManifestRegistryService>$resource("/api/manifestregistry?id=:id", { id: "@id" }, {
            checkVersionAndName: {
                method: 'GET',
                params: {
                    version: "@version",
                    name: "@name"
                },
                isArray: false,
                url: "/api/manifestregistry/checkVersionAndName"
            },
            getDescriptionWithLastVersion: {
                method: 'GET',
                params: {
                    name: "@name"
                },
                isArray: false,
                url: "/api/manifestregistry/getDescriptionWithLastVersion"
            }
         })
     ]);
}