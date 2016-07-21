module dockyard.model {

    export class AdvisoryMessage {
        public name: string;
        public content: string;
    }

    export class AdvisoryMessages {
        public advisories: AdvisoryMessage[];
    }
} 