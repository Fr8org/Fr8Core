module dockyard.model {

    export class Field {
        public key: string;
        public name: string;

        constructor(key: string, name: string) {
            this.key = key;
            this.name = name;
        }

        clone(): Field {
            return new Field(this.key, this.name);
        }
    }

} 