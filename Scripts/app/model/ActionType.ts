//this used to be called ActionType. I renamed it to ActionTemplate only to find that we already have 
//an ActionTemplate. We shouldn't need both, and the ActionTemplate doesn't have anything
//to do with configurationSettings (for now, at least)

//module dockyard.model {
//    export class ActionTemplate {
//        public actionRegistrationId: number;
//        public name: string;
//        public configurationSettings: any;

//        constructor(id: number, name: string, configurationSettings: any) {
//            this.actionRegistrationId = id;
//            this.name = name;
//            this.configurationSettings = configurationSettings;
//        }
//    }
//}