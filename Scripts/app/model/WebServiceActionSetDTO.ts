module dockyard.model {
	export class WebServiceActionSetDTO {
		webServiceIconPath: string;
		actions: Array<ActivityTemplate>;

		constructor(webServiceIconPath: string, actions: Array<ActivityTemplate>) {
			this.webServiceIconPath = webServiceIconPath;
			this.actions = actions;
		}
	}
}