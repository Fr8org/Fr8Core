module dockyard.model {
    export class TerminalDTO {
        internalId : number;
        name: string;
        label: string;
        endpoint: string;
        prodUrl: string;
        devUrl: string;
        description: string;
        version: string;
        terminalStatus: number;
        isFr8OwnTerminal: boolean;
        participationState: enums.ParticipationState;
        authenticationType: number;
        roles: Array<string>;
        
        constructor(internalId: number, name: string, url: string, description: string, roles: Array<string>) {
            this.internalId = internalId;
            this.name = name;
            this.endpoint = url;
            this.description = description;
            this.roles = roles;
            //this.version = "1";
        }
	}
}