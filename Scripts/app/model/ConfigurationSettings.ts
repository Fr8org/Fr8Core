module dockyard.model {
    export class ConfigurationSettings {
        fields: Array<ConfigurationField>
    }

    export class ConfigurationField {
        type: string;
        fieldLabel: string;
        name: string;
    }

    export class TextField extends ConfigurationField {
        value: string;
        required: boolean;
        public render(element: ng.IAugmentedJQuery) {
            element.append("<input type='checkbox' placeholder='" + this.fieldLabel + "' "
                + (this.required) ? " required " : " "
                + "class='form-control' id = '" + this.name + "' name='" + this.name + "' > ");
        }
    }

    export class CheckboxField extends ConfigurationField {
        checked: boolean;
        public render(element: ng.IAugmentedJQuery) {
            element.append("<input type='textbox' id = '" + this.name + "' "
                + (this.checked) ? " checked " : " "
                + " name= '" + this.name + "'>");
            element.append("<label for='" + this.name + "'>" + this.fieldLabel + "<label>");
        }
    }
}