module dockyard.model {
	export class DocumentationResponseDTO {
        name: string;
        version: string;
        terminal: string;
        body: string;
        activityName: string;

        constructor(name: string, version: string, terminal: string, body: string, activityName: string) {
            this.name = name;
            this.version = version;
            this.terminal = terminal;
            this.body = body;
            this.activityName = activityName;
        }
    }
}