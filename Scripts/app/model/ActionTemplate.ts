module dockyard.model {
    export class ActionTemplate {
        id: number;
        name: string;
        version: string;
        defaultEndPoint: string;
        componentActivities: string;

        constructor(id: number, name: string,
            version: string, componentActivities: string) {

            this.id = id;
            this.name = name;
            this.version = version;
            this.componentActivities = componentActivities;
            //this.parentPluginRegistration = parentPluginRegistration;  the client shouldn't know anything about plugins
        }

        clone(): ActionTemplate {
            var result = new ActionTemplate(
                this.id,
                this.name,
                this.version,
                this.componentActivities
            // this.parentPluginRegistration
                );

            return result;
        }
    }
}