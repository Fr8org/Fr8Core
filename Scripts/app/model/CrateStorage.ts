module dockyard.model {
    export class CrateStorage {
        crates: Array<Crate>
    }

    export class Crate {
        id: string;
        label: string;
        contents: string;
        parentCrateId: string;
        manifestType: string;
        manifestId: string;
        manufacturer: string;
    }
    
}