module dockyard.model {
    export class TerminalRegistrationDTO {
        endpoint: string;

        constructor(endpoint: string) {
            this.endpoint = endpoint;
        }
	}
}