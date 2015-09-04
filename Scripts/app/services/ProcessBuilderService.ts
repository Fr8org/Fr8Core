/// <reference path="../_all.ts" />

/*
    The service enables operations with Process Templates
*/
module dockyard.services {
    export interface IProcessTemplateService extends ng.resource.IResourceClass<interfaces.IProcessTemplateVM> { }
    export interface IActionService extends ng.resource.IResourceClass<interfaces.IActionVM> {
        getConfigurationSettings: (actionTemplateId: { id: number }) => ng.resource.IResource<interfaces.IConfigurationSettingsVM>;
        getFieldDataSources: (params: Object, data: interfaces.IActionVM) => interfaces.IDataSourceListVM;
    }
    export interface IDocuSignTemplateService extends ng.resource.IResourceClass<interfaces.IDocuSignTemplateVM> { }
    export interface IDocuSignTriggerService extends ng.resource.IResourceClass<interfaces.IDocuSignExternalEventVM> { }

    app.factory('ProcessTemplateService', ['$resource', 'urlPrefix', ($resource: ng.resource.IResourceService, urlPrefix: string): IProcessTemplateService =>
        <IProcessTemplateService> $resource(urlPrefix + '/processTemplate/:id', { id: '@id' })
    ]);

    app.factory('DocuSignTemplateService', ['$resource', 'urlPrefix', ($resource: ng.resource.IResourceService, urlPrefix: string): IDocuSignTemplateService =>
        <IDocuSignTemplateService> $resource(urlPrefix + '/docusigntemplate')
    ]);

    app.factory('DocuSignTriggerService', ['$resource', 'urlPrefix', ($resource: ng.resource.IResourceService, urlPrefix: string): IDocuSignTriggerService =>
        <IDocuSignTriggerService> $resource(urlPrefix + '/processtemplate/triggersettings')
    ]);

    app.factory('ActionService', ['$resource', 'urlPrefix', ($resource: ng.resource.IResourceService, urlPrefix: string): IActionService =>
        <IActionService> $resource(urlPrefix + '/Action/:id',
            {
                id: '@id'
            },
            {
                'save': {
                    method: 'POST',
                    isArray: true
                },
                'delete': { method: 'DELETE' },
                'getConfigurationSettings': {
                    method: 'GET',
                    url: '/api/Actions/configuration/:id'
                },

                'getFieldDataSources': {
                    method: 'POST',
                    isArray: true,
                    url: '/api/Actions/field_data_sources'
                },
                'params': {
                    id: 'id'
                }
            })
    ]);
}