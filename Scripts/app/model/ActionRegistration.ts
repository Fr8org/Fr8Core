module dockyard.model {
    export class ActionRegistration {
        id: number;
        actionType: string;
        version: string;
        parentPluginRegistration: string;

        constructor(id: number, actionType: string,
            version: string, parentPluginRegistration: string) {

            this.id = id;
            this.actionType = actionType;
            this.version = version;
            this.parentPluginRegistration = parentPluginRegistration;
        }

        clone(): ActionRegistration {
            var result = new ActionRegistration(
                this.id,
                this.actionType,
                this.version,
                this.parentPluginRegistration
            );

            return result;
        }
    }
}