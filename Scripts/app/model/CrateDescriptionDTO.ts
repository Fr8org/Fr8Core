module dockyard.model {
    export class CrateDescriptionDTO {
		manifestId: number;
		manifestType: string;
		label: string;

        constructor(manifestId: number, manifestType: string, label: string) {
            this.manifestId = manifestId;
            this.manifestType = manifestType;
            this.label = label;
		}
	}
}