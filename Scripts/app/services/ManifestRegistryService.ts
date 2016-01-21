
module dockyard.services {
    export interface IManifestRegistryService extends ng.resource.IResourceClass<interfaces.IManifestRegistryVM> {
       addManifestDescription: (param: model.ManifestDescriptionDTO) => Array<model.ManifestDescriptionDTO>
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
                    version: "",
                    name: "",
                    userAccountId: ""
                },
                isArray: false,
                url: "/api/manifestregistry/checkVersionAndName"
            }
         })
     ]);
}