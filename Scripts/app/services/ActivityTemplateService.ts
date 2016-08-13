/// <reference path="../_all.ts" />

module dockyard.services {


    export interface IActivityTemplateService extends ng.resource.IResourceClass<interfaces.IActivityTemplateVM> {
        getAvailableActivities: () => ng.resource.IResource<Array<interfaces.IActivityCategoryDTO>>;
    }

    app.factory('ActivityTemplateService', ['$resource', ($resource: ng.resource.IResourceService): IActivityTemplateService =>
        <IActivityTemplateService>$resource('/api/activity_templates/:id', { id: '@id' },
            {
                'getAvailableActivities': {
                    method: 'GET',
                    url: '/api/activity_templates/',
                    isArray: true
                }
            })
    ]);

    export interface IActivityTemplateHelperService {
        getAvailableActivityTemplatesByCategory: () => ng.IPromise<Array<interfaces.IActivityCategoryDTO>>;
        getAvailableActivityTemplatesInCategories: () => Array<interfaces.IActivityCategoryDTO>;
        getActivityTemplate: (activity: interfaces.IActivityDTO) => interfaces.IActivityTemplateVM;
        toSummary: (activityTemplate: interfaces.IActivityTemplateVM) => model.ActivityTemplateSummary;
        equals: (activityTemplateSummary: model.ActivityTemplateSummary, activityTemplate: interfaces.IActivityTemplateVM) => boolean;
    }

    class ActivityTemplateHelperService implements IActivityTemplateHelperService {

        private activityTemplateCache: Array<interfaces.IActivityTemplateVM> = [];
        private activityTemplateByCategoryCache: Array<interfaces.IActivityCategoryDTO> = null;

        constructor(private ActivityTemplates: Array<interfaces.IActivityCategoryDTO>, private $q: ng.IQService, private activityTemplateService: IActivityTemplateService) {
            //lineralize data
            for (var i = 0; i < this.ActivityTemplates.length; i++) {
                for (var j = 0; j < this.ActivityTemplates[i].activities.length; j++){
                    this.activityTemplateCache.push(this.ActivityTemplates[i].activities[j]);
                }
            }
        }

        public getActivityTemplate(activity: model.ActivityDTO) {
            for (var i = 0; i < this.activityTemplateCache.length; i++) {
                var at = this.activityTemplateCache[i];
                if (this.equals(activity.activityTemplate, at)) {
                    return at;
                }
            }
            return null;
        }

        public toSummary(activityTemplate: interfaces.IActivityTemplateVM): model.ActivityTemplateSummary {
            var result = new model.ActivityTemplateSummary();
            result.name = activityTemplate.name;
            result.version = activityTemplate.version;
            result.terminalName = activityTemplate.terminal.name;
            result.terminalVersion = activityTemplate.terminal.version;
            return result;
        }

        public equals(activityTemplateSummary: model.ActivityTemplateSummary, activityTemplate: interfaces.IActivityTemplateVM): boolean {
            if (activityTemplate.name === activityTemplateSummary.name
                && activityTemplate.version === activityTemplateSummary.version
                && activityTemplate.terminal.name === activityTemplateSummary.terminalName
                && activityTemplate.terminal.version === activityTemplateSummary.terminalVersion) {
                return true;
            }
            return false;
        }

        public getAvailableActivityTemplatesInCategories() {
            return this.ActivityTemplates;
        }

        public getAvailableActivityTemplatesByCategory() {
            var deferred = this.$q.defer<Array<interfaces.IActivityCategoryDTO>>();
            if (this.activityTemplateByCategoryCache != null) {
                deferred.resolve(this.activityTemplateByCategoryCache);
            } else {
                this.activityTemplateService.getAvailableActivities().$promise.then((data) => {
                    this.activityTemplateByCategoryCache = data;
                    deferred.resolve(data);
                }, (err) => {
                    deferred.reject(err);
                });
            }
            return deferred.promise;
        }

    }

    app.factory('ActivityTemplateHelperService', ['ActivityTemplates', '$q', 'ActivityTemplateService', (ActivityTemplates: Array<interfaces.IActivityCategoryDTO>, $q: ng.IQService, ActivityTemplateService: IActivityTemplateService): IActivityTemplateHelperService => new ActivityTemplateHelperService(ActivityTemplates, $q, ActivityTemplateService)]);
}