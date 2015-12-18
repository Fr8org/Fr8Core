module dockyard.model {
    export class ActivityTemplate {
        id: number;
        name: string;
        label: string;
        version: string;
        description: string;
        defaultEndPoint: string;
        componentActivities: string;
        category: string;
        type: string;
        minPaneWidth: number;

        constructor(id: number, name: string,
            version: string, description: string, componentActivities: string, category: string, label?:string, minPaneWidth?:number, type?:string) {

            this.id = id;
            this.name = name;
            this.label = label;
            this.version = version;
            this.description = description;
            this.componentActivities = componentActivities;
            this.category = category;
            this.type = type;
            //this.parentPluginRegistration = parentPluginRegistration;  the client shouldn't know anything about plugins
        }

        clone(): ActivityTemplate {
            var result = new ActivityTemplate(
                this.id,
                this.name,
                this.label,
                this.version,
                this.description,
                this.componentActivities,
                this.category,
                this.minPaneWidth,
                this.type
            // this.parentPluginRegistration
                );

            return result;
        }
    }
}