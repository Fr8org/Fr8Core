module dockyard.model {
	export class TerminalActionSetDTO {
		terminalUrl: string;
		activities: Array<ActivityTemplate>;

        constructor(terminalUrl: string, actions: Array<ActivityTemplate>) {
            this.terminalUrl = terminalUrl;
			this.activities = actions;
		}
	}
}