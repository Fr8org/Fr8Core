
module dockyard.services {
    export interface IManifestRegistryService extends ng.resource.IResourceClass<interfaces.IManifestRegistryVM> {
        checkVersionAndName: (data:{version: string , name: string} ) => any;
        getDescriptionWithLastVersion: (data: { name: string }) => any;
    }

    app.factory("ManifestRegistryService", ["$resource", ($resource: ng.resource.IResourceService): IManifestRegistryService =>
        <IManifestRegistryService>$resource("/api/manifest_registry?id=:id", { id: "@id" }, {
            checkVersionAndName: {
                method: 'POST',
                //params: {
                //    version: "@version",
                //    name: "@name"
                //},
                isArray: false,
                headers: { 'Content-Type': 'application/json' },
                url: "/api/manifest_registry/query"
                //url: "/api/manifestregistry/checkVersionAndName"
            },
            getDescriptionWithLastVersion: {
                method: 'POST',
                //params: {
                //    name: "@name"
                //},
                isArray: false,
                headers: { 'Content-Type': 'application/json' },
                url: "/api/manifest_registry/query"
                //url: "/api/manifestregistry/getDescriptionWithLastVersion"
            }
         })
     ]);
}