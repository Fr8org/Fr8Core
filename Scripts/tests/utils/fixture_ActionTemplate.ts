module dockyard.tests.utils.fixtures {

    export class ActivityTemplate {
        public static activityTemplateDO = <model.ActivityTemplate> {
            id: 1,
            name: "Write to SQL",
            version: "1",
            defaultEndPoint: "AzureSqlServer"
        };
    }
} 