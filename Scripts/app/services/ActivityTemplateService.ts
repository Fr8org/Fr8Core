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
                    url: '/api/activity_templates',
                    isArray: true
                }
            })
    ]);

    export interface IActivityTemplateHelperService {
        getActivityTemplate: (activity: model.ActivityDTO) => interfaces.IActivityTemplateVM;
        toSummary: (activityTemplate: interfaces.IActivityTemplateVM) => model.ActivityTemplateSummary;
        equals: (activityTemplateSummary: model.ActivityTemplateSummary, activityTemplate: interfaces.IActivityTemplateVM) => boolean;
    }

    class ActivityTemplateHelperService implements IActivityTemplateHelperService {

        private activityTemplateCache: Array<interfaces.IActivityTemplateVM> = [];

        constructor(private ActivityTemplates: Array<interfaces.IActivityCategoryDTO>) {
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
        

        //private loadActivities() {

        //    this.ActivityTemplateService.getAvailableActivities().$promise.then((data: Array<interfaces.IActivityCategoryDTO>) => {
        //        var list = [];
        //        for (var i = 0; i < data.length; i++) {
        //            list.push(data[i].activities);
        //        }
        //        this.activityTemplateCache = list;
        //        for (var i = 0; i < this.waiters.length; i++) {
        //            this.waiters[i].resolve(this.activityTemplateCache);
        //        }
        //    }, () => {
        //        for (var i = 0; i < this.waiters.length; i++) {
        //            this.waiters[i].reject();
        //        }
        //    });
        //}

        

        //public getActivities(): ng.IPromise<Array<interfaces.IActivityTemplateVM>> {
        //    var deferred = this.$q.defer<Array<interfaces.IActivityTemplateVM>>();

        //    if (this.activityTemplateCache === null) {
        //        this.waiters.push(deferred);
        //        //this is initial call - we should load templates
        //        if(this.waiters.length === 1){
        //            this.loadActivities();
        //        }
        //    } else {
        //        deferred.resolve(this.activityTemplateCache);
        //    }
        //    return deferred.promise;
        //}

    }

    app.factory('ActivityTemplateHelperService', ['ActivityTemplates', (ActivityTemplates: Array<interfaces.IActivityCategoryDTO>): IActivityTemplateHelperService => new ActivityTemplateHelperService(ActivityTemplates)]);
}