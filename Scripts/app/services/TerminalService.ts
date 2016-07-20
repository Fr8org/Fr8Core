
module dockyard.services {
    export interface ITerminalService extends ng.resource.IResourceClass<interfaces.ITerminalVM> {
        getRegistrations: () => Array<model.TerminalRegistrationDTO>;
        getAll: () => Array<model.TerminalDTO>;
        register: (terminal: model.TerminalRegistrationDTO) => ng.IPromise<any>;
    }

    app.factory("TerminalService", ["$resource", ($resource: ng.resource.IResourceService): ITerminalService =>
        <ITerminalService>$resource("/api/terminals?id=:id", { id: "@id" }, {
            getRegistrations: {
                method: "GET",
                isArray: true,
                url: "/api/terminals/registrations"
            },
            getAll: {
                method: "GET",
                isArray: true,
                url: "/api/terminals/all"
            },
            register: {
                method: "POST",
                url: "/api/terminals"
            }
        })
    ]);
}