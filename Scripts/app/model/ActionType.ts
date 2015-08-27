module dockyard.model {
    export class ActionType {
        public id: number;
        public name: string;
        public configurationSettings: any;

        constructor(id: number, name: string, configurationSettings: any) {
            this.id = id;
            this.name = name;
            this.configurationSettings = configurationSettings;
        }
    }
}