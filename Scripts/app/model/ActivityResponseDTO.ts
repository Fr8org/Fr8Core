module dockyard.model {
    export class ActivityResponseDTO {
        type: string;
        body: any;
        constructor(
            type: string,
            body: any) {
            this.type = type;
            this.body = body;
        }

        clone(): ActivityResponseDTO {
            var result = new ActivityResponseDTO(
                this.type,
                this.body
            );

            return result;
        }
    }
}