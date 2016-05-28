
module dockyard.services {
    export interface ISolutionDocumentationService extends ng.resource.IResourceClass<interfaces.ISolutionDocumentationVM> {
        getSolutionDocumentationList: (terminalName: { terminalName:string }) => Array<string>;
        getSolutionDTO: (action: interfaces.IActivityDTO) => ng.resource.IResource<model.SolutionDTO>;
    }

    app.factory("SolutionDocumentationService", ["$resource", ($resource: ng.resource.IResourceService): ISolutionDocumentationService =>
        <ISolutionDocumentationService>$resource("/api/activities", { terminalName: '@terminalName' },
            {
                'getSolutionDocumentationList': {
                    method: "GET",
                    isArray: true,
                    url: "/api/activities/getDocusignsolutionlist"
                },
                'getSolutionDTO': {
                    method: "POST",
                    isArray: false,
                    url: "/api/documentation/activity"
                }
            })
    ]);
}