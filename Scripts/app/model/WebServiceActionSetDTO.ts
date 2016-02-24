module dockyard.model {
	export class WebServiceActionSetDTO {
		webServiceIconPath: string;
        activities: Array<ActivityTemplate>;
        webServiceName: string;

        constructor(webServiceIconPath: string, webServiceName: string, activities: Array<ActivityTemplate>) {
			this.webServiceIconPath = webServiceIconPath;
            this.activities = activities;
            this.webServiceName = webServiceName;
		}
	}
}