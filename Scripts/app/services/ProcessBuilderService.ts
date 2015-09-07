/// <reference path="../_all.ts" />

/*
    The service enables operations with Process Templates
*/
module dockyard.services {
    export interface IProcessTemplateService extends ng.resource.IResourceClass<interfaces.IProcessTemplateVM> { }
    export interface IActionService extends ng.resource.IResourceClass<interfaces.IActionVM> {
        getConfigurationStore: (actionTemplateId: { id: number }) => ng.resource.IResource<interfaces.IConfigurationStoreVM>;
        getFieldDataSources: (params: Object, data: interfaces.IActionVM) => interfaces.IDataSourceListVM;
    }
    export interface IDocuSignTemplateService extends ng.resource.IResourceClass<interfaces.IDocuSignTemplateVM> { }
    export interface IDocuSignTriggerService extends ng.resource.IResourceClass<interfaces.IDocuSignExternalEventVM> { }
    export interface ICriteriaService extends ng.resource.IResourceClass<interfaces.ICriteriaVM> { }

    app.factory('ProcessTemplateService', ['$resource', 'urlPrefix', ($resource: ng.resource.IResourceService, urlPrefix: string): IProcessTemplateService =>
        <IProcessTemplateService> $resource(urlPrefix + '/processTemplate/:id', { id: '@id' })
    ]);

    app.factory('DocuSignTemplateService', ['$resource', 'urlPrefix', ($resource: ng.resource.IResourceService, urlPrefix: string): IDocuSignTemplateService =>
        <IDocuSignTemplateService> $resource(urlPrefix + '/docusigntemplate')
    ]);

    app.factory('DocuSignTriggerService', ['$resource', 'urlPrefix', ($resource: ng.resource.IResourceService, urlPrefix: string): IDocuSignTriggerService =>
        <IDocuSignTriggerService> $resource(urlPrefix + '/processtemplate/trigg ersettings')
    ]);

    app.factory('CriteriaService', ['$resource', 'urlPrefix', ($resource: ng.resource.IResourceService, urlPrefix: string): IDocuSignTriggerService =>
        <IDocuSignTriggerService> $resource(urlPrefix + '/processtemplate/trigg ersettings')
    ]);

    app.factory('ActionService', ['$resource', 'urlPrefix', ($resource: ng.resource.IResourceService, urlPrefix: string): IActionService =>
        <IActionService> $resource('/actions/:id',
            {
                id: '@id'
            },
            {
                'save': {
                    method: 'POST',
                    isArray: true,
                    url: '/actions/save'
                },
                //'get': {
                //    transformResponse: function (data) {
                //        //Map a proto-action object to an actual ActionDesignDTO instance so that we can access methods 
                //        var dataObject: interfaces.IActionDesignDTO = angular.fromJson(data);
                //        return model.ActionDesignDTO.create(dataObject);
                //    }
                //},
                'delete': { method: 'DELETE' },
                'getConfigurationStore': {
                    method: 'POST',
                    url: '/actions/configuration',
                    params: { curActionDesignDTO: model.ActionDesignDTO } //pass ActionDesignDTO as parameter
                },

                'getFieldDataSources': {
                    method: 'POST',
                    isArray: true,
                    url: '/actions/field_data_sources'
                },
                'params': {
                    id: 'id'
                }
            })
    ]);
}