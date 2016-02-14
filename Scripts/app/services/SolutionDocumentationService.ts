
module dockyard.services {
    export interface ISolutionDocumentationService extends ng.resource.IResourceClass<interfaces.ISolutionDocumentationVM> {
        getSolutionDocumentationList: (terminalName: { terminalName:string }) => Array<string>
        getSolutionDTO: (action: interfaces.IActivityDTO) => ng.resource.IResource<model.SolutionDTO>
    }

    app.factory("SolutionDocumentationService", ["$resource", ($resource: ng.resource.IResourceService): ISolutionDocumentationService =>
        <ISolutionDocumentationService>$resource("/api/actions", { terminalName: '@terminalName' },
            {
                'getSolutionDocumentationList': {
                    method: "GET",
                    isArray: true,
                    url: "/api/actions/getDocusignsolutionlist"
                },
                'getSolutionDTO': {
                    method: "POST",
                    isArray: false,
                    url: "/api/actions/documentation",
                }
            })
    ]);
}