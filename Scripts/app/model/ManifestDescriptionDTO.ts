module dockyard.model {
    export class ManifestDescriptionDTO {
        id: number;
        name: string;
        version: string;
        sampleJSON: string;
        description: string;
        registeredBy: string;
        
        constructor(id: number, name: string, version: string, sampleJSON: string, description: string, registeredBy: string) {
            this.id = id;
            this.name = name;
            this.version = version;
            this.sampleJSON = sampleJSON;
            this.description = description;
            this.registeredBy = registeredBy;
        }
    }
}