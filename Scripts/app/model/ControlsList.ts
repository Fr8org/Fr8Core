module dockyard.model {
    export class ControlsList {
        fields: Array<ConfigurationField>
    }

    export class ConfigurationField {
        type: string;
        fieldLabel: string;
        name: string;
        events: Array<FieldEvent>;
    }

    export class FieldEvent {
        name: string;
        handler: string;
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

    export class FieldDTO {
        public Key: string;
        public Value: string;
    }

    export class DropDownListItem extends FieldDTO {
        
    }

    export class FieldSource {
        public manifestType: string;
        public label: string;
    }

    export class DropDownListBoxField extends ConfigurationField {
        listItems: Array<DropDownListItem>;
        value: string;
        source: FieldSource;
    }

    export class TextBlockField extends ConfigurationField {
        public value: string;
        public class: string;
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