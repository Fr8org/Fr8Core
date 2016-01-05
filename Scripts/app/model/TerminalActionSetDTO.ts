module dockyard.model {
	export class TerminalActionSetDTO {
		terminalUrl: string;
		actions: Array<ActivityTemplate>;

        constructor(terminalUrl: string, actions: Array<ActivityTemplate>) {
            this.terminalUrl = terminalUrl;
			this.actions = actions;
		}
	}
}