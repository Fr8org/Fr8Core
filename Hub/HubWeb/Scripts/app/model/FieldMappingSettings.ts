module dockyard.model {
    export class FieldMappingSettings {
        public fields: Array<FieldMapping>;
    }

    export class FieldMapping {
        public name: string;
        public value: string;

        constructor(name: string, value: string) {
            this.name = name;
            this.value = value;
        }
    }
}
