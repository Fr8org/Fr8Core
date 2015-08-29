module dockyard.model {
    export class ActionTemplate {
        public actionTemplateId: number;
        public name: string;
        public configurationSettings: any;

        constructor(id: number, name: string, configurationSettings: any) {
            this.actionTemplateId = id;
            this.name = name;
            this.configurationSettings = configurationSettings;
        }
    }
}