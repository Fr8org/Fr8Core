module dockyard.model {
    export class ActivityTemplate {
        id: number;
        name: string;
        version: string;
        defaultEndPoint: string;
        componentActivities: string;
        category: string;

        constructor(id: number, name: string,
            version: string, componentActivities: string, category: string) {

            this.id = id;
            this.name = name;
            this.version = version;
            this.componentActivities = componentActivities;
            this.category = category;
            //this.parentPluginRegistration = parentPluginRegistration;  the client shouldn't know anything about plugins
        }

        clone(): ActivityTemplate {
            var result = new ActivityTemplate(
                this.id,
                this.name,
                this.version,
                this.componentActivities,
                this.category
            // this.parentPluginRegistration
                );

            return result;
        }
    }

    export class IsAuthenticatedDTO {
        authenticated: boolean;
    }
}