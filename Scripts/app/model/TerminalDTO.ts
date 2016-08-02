module dockyard.model {
    export class TerminalDTO {
        name: string;
        label: string;
        endpoint: string;
        description: string;
        version: string;
        terminalStatus: number;
        authenticationType: number;
        
        constructor(name: string, url: string, description: string) {
			this.name = name;
            this.endpoint = url;
            this.description = description;
            //this.version = "1";
        }
	}
}