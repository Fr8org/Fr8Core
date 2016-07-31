module dockyard.model {
    export class TerminalDTO {
        id : number;
        name: string;
        label: string;
        endpoint: string;
        description: string;
        version: string;
        terminalStatus: number;
        authenticationType: number;
        
        constructor(id: number, name: string, url: string, description: string) {
            this.id = id;
            this.name = name;
            this.endpoint = url;
            this.description = description;
            //this.version = "1";
        }
	}
}