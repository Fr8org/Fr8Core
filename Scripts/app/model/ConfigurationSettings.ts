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
    }

    export class TextField extends ConfigurationField {
        value: string;
        required: boolean;
    }

    export class RoutingControlGroup extends ConfigurationField {
        sourceField: string;
        routes : Array<Route>
    }

    export class Route extends ConfigurationField {
        measurementValue: string;
        selection: string;
        previousActionList: string;
        previousActionSelectedId: string;
        availableProcessNode: string;
    }
}