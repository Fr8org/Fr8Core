module dockyard.model {
    export class ActivityTemplate {
        id: string;
        name: string;
        label: string;
        version: string;
        defaultEndPoint: string;
        category: string;
        type: string;
        tags: string;
        minPaneWidth: number;
        terminal: TerminalDTO;
        needsAuthentication: boolean;
        webService: WebServiceDTO; 

        constructor(
            id: string,
            name: string,
            version: string,
            category: string,
            label?: string,
            minPaneWidth?: number,
            type?: string,
            webService?: WebServiceDTO) {

            this.id = id;
            this.name = name;
            this.label = label;
            this.version = version;
            this.category = category;
            this.type = type;
            this.webService = webService;
            //this.parentPluginRegistration = parentPluginRegistration;  the client shouldn't know anything about plugins
        }

        clone(): ActivityTemplate {
            var result = new ActivityTemplate(
                this.id,
                this.name,
                this.label,
                this.version,
                this.category,
                this.minPaneWidth,
                this.type,
                this.webService
            // this.parentPluginRegistration
                );

            return result;
        }
    }
}