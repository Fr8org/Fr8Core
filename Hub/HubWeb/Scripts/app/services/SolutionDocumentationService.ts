
module dockyard.services {
    export interface ISolutionDocumentationService extends ng.resource.IResourceClass<interfaces.ISolutionDocumentationVM> {
        getSolutionDocumentationList: (terminalName: { terminalName:string }) => Array<string>;
        getDocumentationResponseDTO: (action: interfaces.IActivityDTO) => ng.resource.IResource<model.DocumentationResponseDTO>;
    }

    app.factory("SolutionDocumentationService", ["$resource", ($resource: ng.resource.IResourceService): ISolutionDocumentationService =>
        <ISolutionDocumentationService>$resource("/api/activities", { terminalName: '@terminalName' },
            {
                'getSolutionDocumentationList': {
                    method: "GET",
                    isArray: true,
                    url: "/api/activities/getDocusignsolutionlist"
                },
                'getDocumentationResponseDTO': {
                    method: "POST",
                    isArray: false,
                    url: "/api/documentation/activity"
                }
            })
    ]);
}