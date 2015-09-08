module dockyard.model {
    export class ActionTemplate {
        id: number;
        name: string;
        version: string;
        defaultEndPoint: string;

        constructor(id: number, name: string,
            version: string) {

            this.id = id;
            this.name = name;
            this.version = version;
            //this.parentPluginRegistration = parentPluginRegistration;  the client shouldn't know anything about plugins
        }

        clone(): ActionTemplate {
            var result = new ActionTemplate(
                this.id,
                this.name,
                this.version
               // this.parentPluginRegistration
            );

            return result;
        }
    }
}