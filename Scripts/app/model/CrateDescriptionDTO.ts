module dockyard.model {
    export class CrateDescriptionDTO {
		manifestId: number;
		manifestType: string;
        label: string;
        producedBy: string;
        selected: boolean;
        fields: FieldDTO[];
	}
}