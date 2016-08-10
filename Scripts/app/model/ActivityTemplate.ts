module dockyard.model {

    export class ActivityTemplateSummary {
        name: string;
        version: string;
        terminalName: string;
        terminalVersion: string;
    }

    export class ActivityTemplate {
        id: string;
        name: string;
        label: string;
        version: string;
        defaultEndPoint: string;
        // TODO: FR-4943, remove this.
        // category: string;
        type: string;
        tags: string;
        minPaneWidth: number;
        terminal: TerminalDTO;
        needsAuthentication: boolean;
        // TODO: FR-4943, remove this.
        // webService: WebServiceDTO; 
        categories: Array<ActivityCategoryDTO>;
        showDocumentation: ActivityResponseDTO;
        description: string;
        constructor(
            id: string,
            name: string,
            version: string,
            // TODO: FR-4943, remove this.
            // category: string,
            label?: string,
            minPaneWidth?: number,
            type?: string,
            // TODO: FR-4943, remove this.
            // webService?: WebServiceDTO,
            categories?: Array<ActivityCategoryDTO>,
            showDocumentation?: ActivityResponseDTO,
            description?: string) {

            this.id = id;
            this.name = name;
            this.label = label;
            this.version = version;
            // TODO: FR-4943, remove this.
            // this.category = category;
            this.type = type;
            // TODO: FR-4943, remove this.
            // this.webService = webService;
            this.categories = categories;
            this.showDocumentation = showDocumentation;
            this.description = description;
            //this.parentPluginRegistration = parentPluginRegistration;  the client shouldn't know anything about plugins
        }

        clone(): ActivityTemplate {
            var result = new ActivityTemplate(
                this.id,
                this.name,
                this.label,
                this.version,
                // TODO: FR-4943, remove this.
                // this.category,
                this.minPaneWidth,
                this.type,
                // TODO: FR-4943, remove this.
                // this.webService,
                this.categories,
                this.showDocumentation,
                this.description
            // this.parentPluginRegistration
                );

            return result;
        }
    }
}