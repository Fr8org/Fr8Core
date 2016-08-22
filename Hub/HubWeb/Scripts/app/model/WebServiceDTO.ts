module dockyard.model {
	export class WebServiceDTO {
		id: number;
		name: string;
		iconPath: string;

		constructor(id: number, name: string, iconPath: string) {
			this.id = id;
			this.name = name;
			this.iconPath = iconPath;
		}
	}
}