module dockyard.services {

    export interface IOrganizationSettingsService extends ng.resource.IResourceClass<interfaces.IOrganizationSettingsVM> {
        add: (organizationSettings: model.OrganizationDTO) => interfaces.IOrganizationSettingsVM;
        update: (organizationSettings: model.OrganizationDTO) => interfaces.IOrganizationSettingsVM;
    }

    app.factory('OrganizationSettingsService', ['$resource', ($resource: ng.resource.IResourceService): IOrganizationSettingsService =>
        <IOrganizationSettingsService>$resource('/api/organizationsettings', null,
            {
                'add': {
                    method: 'POST'
                },
                'update': {
                    method: 'PUT'
                },
                'get': {
                    method: 'GET'
                }   
            })
    ]);
} 