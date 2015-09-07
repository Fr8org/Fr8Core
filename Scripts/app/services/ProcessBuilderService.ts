/// <reference path="../_all.ts" />

/*
    The service enables operations with Process Templates
*/
module dockyard.services {
    export interface IProcessTemplateService extends ng.resource.IResourceClass<interfaces.IProcessTemplateVM> { }
    export interface IActionService extends ng.resource.IResourceClass<interfaces.IActionVM> {
        getConfigurationSettings: (actionTemplateId: { id: number }) => interfaces.IConfigurationSettingsVM;
        getFieldDataSources: (params: Object, data: interfaces.IActionVM) => interfaces.IDataSourceListVM;
    }
    export interface IDocuSignTemplateService extends ng.resource.IResourceClass<interfaces.IDocuSignTemplateVM> { }
    export interface IDocuSignTriggerService extends ng.resource.IResourceClass<interfaces.IDocuSignExternalEventVM> { }
    interface __IProcessNodeTemplateService extends ng.resource.IResourceClass<interfaces.IProcessNodeTemplateVM> {
        add: (curProcessNodeTemplate: model.ProcessNodeTemplateDTO) => interfaces.IProcessNodeTemplateVM;
        update: (curProcessNodeTemplate: model.ProcessNodeTemplateDTO) => interfaces.IProcessNodeTemplateVM;
    }
    interface __ICriteriaService extends ng.resource.IResourceClass<interfaces.ICriteriaVM> {
        update: (curCriteria: model.CriteriaDTO) => interfaces.ICriteriaVM;
        byProcessNodeTemplate: (id: { id: number }) => interfaces.ICriteriaVM;
    }
    export interface ICriteriaWrapperService {
        load: (id: number) => ng.IPromise<model.ProcessNodeTemplateDTO>;
        add: (curProcessNodeTemplate: model.ProcessNodeTemplateDTO) => ng.IPromise<model.ProcessNodeTemplateDTO>;
        update: (curProcessNodeTemplate: model.ProcessNodeTemplateDTO) => ng.IPromise<model.ProcessNodeTemplateDTO>;
        addOrUpdate(curProcessNodeTemplate: model.ProcessNodeTemplateDTO): {
            actionType: ActionTypeEnum,
            promise: ng.IPromise<model.ProcessNodeTemplateDTO>
        }
    }

    app.factory('ProcessTemplateService', ['$resource', 'urlPrefix', ($resource: ng.resource.IResourceService): IProcessTemplateService =>
        <IProcessTemplateService> $resource('api/processTemplate/:id', { id: '@id' })
    ]);

    app.factory('DocuSignTemplateService', ['$resource', 'urlPrefix', ($resource: ng.resource.IResourceService): IDocuSignTemplateService =>
        <IDocuSignTemplateService> $resource('api/docusigntemplate')
    ]);

    app.factory('DocuSignTriggerService', ['$resource', ($resource: ng.resource.IResourceService): IDocuSignTriggerService =>
        <IDocuSignTriggerService> $resource('/api/processtemplate/triggersettings')
    ]);

    app.factory('ActionService', ['$resource', ($resource: ng.resource.IResourceService): IActionService =>
        <IActionService> $resource('/api/actions/:id',
            {
                id: '@id'
            },
            {
                'save': {
                    method: 'POST',
                    isArray: true,
                    url: '/api/actions/save'
                },
                //'get': {
                //    transformResponse: function (data) {
                //        //Map a proto-action object to an actual ActionDesignDTO instance so that we can access methods 
                //        var dataObject: interfaces.IActionDesignDTO = angular.fromJson(data);
                //        return model.ActionDesignDTO.create(dataObject);
                //    }
                //},
                'delete': { method: 'DELETE' },
                'getConfigurationSettings': {
                    method: 'GET',
                    url: '/api/actions/configuration/:id'
                },

                'getFieldDataSources': {
                    method: 'POST',
                    isArray: true,
                    url: '/api/actions/field_data_sources'
                },
                'params': {
                    id: 'id'
                }
            })
    ]);

    app.factory('CriteriaService', ['$resource', 'urlPrefix', ($resource: ng.resource.IResourceService): __ICriteriaService =>
        <__ICriteriaService> $resource('/api/criteria', null,
            {
                'update': {
                    method: 'PUT'
                },
                'save': {
                    method: 'PUT'
                },
                'byProcessNodeTemplate': {
                    method: 'GET',
                    url: '/api/criteria/byProcessNodeTemplate'
                }
            })
    ]);

    app.factory('ProcessNodeTemplateService', ['$resource', 'urlPrefix', ($resource: ng.resource.IResourceService): __IProcessNodeTemplateService =>
        <__IProcessNodeTemplateService> $resource('/api/processnodetemplate', null,
            {
                'add': {
                    method: 'POST'
                },
                'update': {
                    method: 'PUT'
                }
            })
    ]);

    class CriteriaWrapperService implements ICriteriaWrapperService {
        public constructor(
            private CriteriaService: __ICriteriaService,
            private ProcessNodeTemplateService: __IProcessNodeTemplateService,
            private $q: ng.IQService) {
        }

        public add(curProcessNodeTemplate: model.ProcessNodeTemplateDTO): ng.IPromise<model.ProcessNodeTemplateDTO> {
            var deferred = this.$q.defer<interfaces.IProcessNodeTemplateVM>();

            // Save ProcessNodeTemplate object to server.
            // Server automatically creates empty criteria node.
            var addDeferred = this.ProcessNodeTemplateService.add(curProcessNodeTemplate);

            addDeferred.$promise
                .then((addResult: interfaces.IProcessNodeTemplateVM) => {
                    curProcessNodeTemplate.isTempId = false;
                    curProcessNodeTemplate.id = addResult.id;    
                    // Fetch criteria object from server by ProcessNodeTemplate global ID.
                    return this.CriteriaService.byProcessNodeTemplate({ id: addResult.id }).$promise;
                })
                .then((getResult: interfaces.ICriteriaVM) => {
                    curProcessNodeTemplate.criteria.id = getResult.id;
                    curProcessNodeTemplate.criteria.isTempId = false;
                    // Update criteria object on server.
                    return this.CriteriaService.save({ id: getResult.id }).$promise;
                })
                .then((updateResult: interfaces.ICriteriaVM) => {
                    deferred.resolve(addDeferred);
                })
                .catch((reason: any) => {
                    deferred.reject(reason);
                });

            return deferred.promise;
        }

        public update(curProcessNodeTemplate: model.ProcessNodeTemplateDTO): ng.IPromise<model.ProcessNodeTemplateDTO> {
            var deferred = this.$q.defer<interfaces.IProcessNodeTemplateVM>();

            // Save ProcessNodeTemplate object to server.
            // Server automatically creates empty criteria node.
            var updateDeferred = this.ProcessNodeTemplateService.update(curProcessNodeTemplate);

            updateDeferred.$promise
                .then((updateResult: interfaces.IProcessNodeTemplateVM) => {   
                    // Call REST api to update Criteria entity on server.
                    return this.CriteriaService.update(curProcessNodeTemplate.criteria).$promise;
                })
                .then((updateResult: interfaces.ICriteriaVM) => {
                    deferred.resolve(updateDeferred);
                })
                .catch((reason: any) => {
                    deferred.reject(reason);
                });

            return deferred.promise;
        }

        public load(id: number): ng.IPromise<model.ProcessNodeTemplateDTO> {
            var deferred = this.$q.defer<interfaces.IProcessNodeTemplateVM>();
            debugger;

            // Save ProcessNodeTemplate object to server.
            // Server automatically creates empty criteria node.
            var getPntDeferred = this.ProcessNodeTemplateService.get({ id: id }),
                getCriteriaDeferred = this.CriteriaService.byProcessNodeTemplate({ id: id });

            this.$q.all([getPntDeferred.$promise, getCriteriaDeferred.$promise])
                .then(() => {
                    var criteria = new model.CriteriaDTO(
                        getCriteriaDeferred.id,
                        false,
                        getPntDeferred.id,
                        getCriteriaDeferred.executionType
                        );

                    angular.forEach(getCriteriaDeferred.conditions, function (it: model.Condition) {
                        criteria.conditions.push(it);
                    });

                    getPntDeferred.criteria = criteria;
                    deferred.resolve(getPntDeferred);
                });

            return deferred.promise;
        }

        public addOrUpdate(curProcessNodeTemplate: model.ProcessNodeTemplateDTO):
            {
                actionType: ActionTypeEnum,
                promise: ng.IPromise<model.ProcessNodeTemplateDTO>
            }
        {
            // Don't save anything if there is no criteria selected, 
            // just return a null- valued resolved promise
            if (!curProcessNodeTemplate) {
                var deferred = this.$q.defer < model.ProcessNodeTemplateDTO>();
                deferred.resolve(null);
                return {
                    actionType: ActionTypeEnum.None,
                    promise: deferred.promise
                };
            }

            if (curProcessNodeTemplate.isTempId) {
                // In case of newly created object (i.e. isTempId === true).
                return {
                    actionType: ActionTypeEnum.Add,
                    promise: this.add(curProcessNodeTemplate)
                };
            }
            else {
                return {
                    actionType: ActionTypeEnum.Update,
                    promise: this.update(curProcessNodeTemplate)
                };
            }
        }
    }

    export enum ActionTypeEnum {
        None = 0,
        Add = 1,
        Update = 2
    }

    app.factory('CriteriaServiceWrapper', ['CriteriaService', 'ProcessNodeTemplateService', '$q', (CriteriaService, ProcessNodeTemplateService, $q) => {
        return new CriteriaService(CriteriaService, ProcessNodeTemplateService, $q)
    }]);
}