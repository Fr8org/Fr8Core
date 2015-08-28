module dockyard.model {
    export class ConfigurationSettings {
        fields: Array<ConfigurationField>
    }

    export class ConfigurationField {
        type: string;
        fieldLabel: string;
        name: string;
    }

    export class CheckboxField extends ConfigurationField {
        checked: boolean;
        public render(element: ng.IAugmentedJQuery) {
            var prefix: string = "pca__chk__",
                placeholder = element.find("div.field-placeholder"),
                field = angular.element(
                    "<div class='col-sm-offset-1'>"
                    + "<div class='checkbox checkbox-primary'>"
                    + "   <input type='checkbox' id='" + prefix + this.name + "'"
                    + "     name='" + prefix + this.name + "'>"
                    + "   <label for='" + prefix + this.name + "'>" + this.fieldLabel + "</label>"
                    + "</div>"
                    + "</div>");
            var wrapper = angular.element("<div class='form-group'></div>");
            placeholder.append(wrapper.append(field));
        }
    }

    export class TextField extends ConfigurationField {
        value: string;
        required: boolean;
        public render(element: ng.IAugmentedJQuery) {
            var prefix: string = "pca__txt__",
                placeholder = element.find("div.field-placeholder"),
                field = angular.element(
                    "<label class='control-label'>" + this.fieldLabel + "</label>"
                    + "<input type='textbox' id='" + prefix + this.name + "' "
                    + " class='form-control form-control-focus' name='" + prefix + this.name + "'>");
            var wrapper = angular.element("<div class='form-group'></div>");
            placeholder.append(wrapper.append(field));
        }
    }
}