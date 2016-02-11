module dockyard.model {

    export class Condition implements interfaces.ICondition {
        public field: string;
        public operator: string;
        public value: string;
        public valueError: string;

        constructor(field: string, operator: string, value: string) {
            this.field = field;
            this.operator = operator;
            this.value = value;
        }

        clone(): Condition {
            var result = new Condition(this.field, this.operator, this.value);
            result.valueError = this.valueError;

            return result;
        }
    }

} 
