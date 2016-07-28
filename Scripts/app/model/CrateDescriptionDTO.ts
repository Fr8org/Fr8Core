module dockyard.model {
    export class CrateDescriptionDTO {
		manifestId: number;
        manifestType: string;
        availability: AvailabilityType;
        sourceActivityId: string;
        label: string;
        producedBy: string;
        selected: boolean;
        fields: FieldDTO[];
	}
}