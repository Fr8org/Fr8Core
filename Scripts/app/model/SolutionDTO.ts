module dockyard.model {
	export class SolutionDTO {
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

    export class SolutionNameValue {
        solutionName: string;
        solutionFriendlyName: string;

        constructor(solutionName: string, solutionFriendlyName: string, terminal: string, body: string) {
            this.solutionName = solutionName;
            this.solutionFriendlyName = solutionFriendlyName;
        }
    }
}