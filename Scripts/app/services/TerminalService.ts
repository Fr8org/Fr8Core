
module dockyard.services {
    export interface ITerminalService extends ng.resource.IResourceClass<interfaces.ITerminalVM> {
        getActions: (params: Array<number>) => Array<model.TerminalActionSetDTO>;
    }

    app.factory("TerminalService", ["$resource", ($resource: ng.resource.IResourceService): ITerminalService =>
        <ITerminalService>$resource("/api/terminals?id=:id", { id: "@id" }, {
            getActions: {
                method: "POST",
                isArray: true,
                url: "/api/terminals/actions"
            }
        })
    ]);
}