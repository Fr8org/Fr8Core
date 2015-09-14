module dockyard.model {
    export class CrateStorage {
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

    export class FileField extends ConfigurationField {
        value: string;
    }

    export class RadioField extends ConfigurationField {
        value: string;
        selected: boolean;
    }

    export class RadioButtonGroupField extends ConfigurationField {
        groupName: string;
        radios: Array<RadioField>;
    }

    export class RoutingControlGroup extends ConfigurationField {
        sourceField: string;
        routes: Array<Route>
    }

    export class Route extends ConfigurationField {
        measurementValue: string;
        selection: string;
        previousActionList: RouteActionList;
        previousActionSelectedId: string;
        availableProcessNode: string;
    }

    export class RouteActionList extends ConfigurationField {
        choices: Array<Choice>;
        selectionId: string;
    }

    export class Choice extends ConfigurationField {
        Label: string;
        Id: string;
    }
}