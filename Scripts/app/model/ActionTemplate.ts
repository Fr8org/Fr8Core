module dockyard.model {
    export class ActionTemplate {
        id: number;
        actionType: string;
        version: string;
        parentPluginRegistration: string;

        constructor(id: number, actionType: string,
            version: string) {

            this.id = id;
            this.actionType = actionType;
            this.version = version;
            //this.parentPluginRegistration = parentPluginRegistration;  the client shouldn't know anything about plugins
        }

        clone(): ActionTemplate {
            var result = new ActionTemplate(
                this.id,
                this.actionType,
                this.version
               // this.parentPluginRegistration
            );

            return result;
        }
    }
}