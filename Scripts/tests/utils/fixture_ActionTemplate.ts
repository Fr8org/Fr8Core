module dockyard.tests.utils.fixtures {

    export class ActionTemplate {
        public static actionTemplateDO = <model.ActionTemplate> {
            id: 1,
            actionType: "Write to SQL",
            version: "1",
            defaultEndPoint: "AzureSqlServer"
        };
    }
} 