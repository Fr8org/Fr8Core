module dockyard.model {
    export class ActivityTemplate {
        id: number;
        name: string;
        label: string;
        version: string;
        defaultEndPoint: string;
        componentActivities: string;
        category: string;
        minPaneWidth: number;

        constructor(id: number, name: string,
            version: string, componentActivities: string, category: string, label?:string, minPaneWidth?:number) {

            this.id = id;
            this.name = name;
            this.label = label;
            this.version = version;
            this.componentActivities = componentActivities;
            this.category = category;
            //this.parentPluginRegistration = parentPluginRegistration;  the client shouldn't know anything about plugins
        }

        clone(): ActivityTemplate {
            var result = new ActivityTemplate(
                this.id,
                this.name,
                this.label,
                this.version,
                this.componentActivities,
                this.category,
                this.minPaneWidth
            // this.parentPluginRegistration
                );

            return result;
        }
    }
}