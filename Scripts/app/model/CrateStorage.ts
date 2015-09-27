module dockyard.model {
    export class CrateStorage {
        crates: Array<Crate>;
        crateDTO: Array<Crate>; // purely for easier deserialization on backend
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