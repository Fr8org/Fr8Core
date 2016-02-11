module dockyard.model {
	export class WebServiceActionSetDTO {
		webServiceIconPath: string;
        actions: Array<ActivityTemplate>;
        webServiceName: string;

		constructor(webServiceIconPath: string, webServiceName: string, actions: Array<ActivityTemplate>) {
			this.webServiceIconPath = webServiceIconPath;
            this.actions = actions;
            this.webServiceName = webServiceName;
		}
	}
}