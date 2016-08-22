module dockyard.model {

    export class ValidationResult {
        public errorMessage: string;
        public controlNames : string[];
    }

    export class ValidationResults {
        public validationErrors: ValidationResult[];
    }

} 