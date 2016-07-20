module dockyard.services {

    export interface IOrganizationService extends ng.resource.IResourceClass<interfaces.IOrganizationVM> {
        add: (organization: model.OrganizationDTO) => interfaces.IOrganizationVM;
        update: (organization: model.OrganizationDTO) => interfaces.IOrganizationVM;
        getOrganization: (params: { organizationId: number }) => interfaces.IOrganizationVM;
    }

    app.factory('OrganizationService', ['$resource', ($resource: ng.resource.IResourceService): IOrganizationService =>
        <IOrganizationService>$resource('/api/organizations', null,
            {
                'add': {
                    method: 'POST'
                },
                'update': {
                    method: 'PUT'
                },
                'getOrganization': {
                    method: 'GET',
                    url: '/api/organizations?id=:id',
                    params: {
                        id: '@id'
                    }
                }
            })
    ]);
} 