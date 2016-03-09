
module dockyard.services {
    export interface ITerminalService extends ng.resource.IResourceClass<interfaces.ITerminalVM> {
        getActivities: (params: Array<number>) => Array<model.TerminalActionSetDTO>;
    }

    app.factory("TerminalService", ["$resource", ($resource: ng.resource.IResourceService): ITerminalService =>
        <ITerminalService>$resource("/api/terminals?id=:id", { id: "@id" }, {
            getActivities: {
                method: "POST",
                isArray: true,
                url: "/api/terminals/activities"
            }
        })
    ]);
}