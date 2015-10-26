module dockyard.model {
    export class ControlsList {
        fields: Array<ControlDefinitionDTO>
    }

    export interface ISupportsNestedFields {
        controls: Array<ControlDefinitionDTO>;
    }

    export class ControlDefinitionDTO {
        type: string;
        fieldLabel: string;
        name: string;
        events: Array<ControlEvent>;
        value: string;
    }

    export class ControlEvent {
        name: string;
        handler: string;
    }

    export class CheckBoxControlDefinitionDTO extends ControlDefinitionDTO {
        checked: boolean;
    }

    export class TextBoxControlDefinitionDTO extends ControlDefinitionDTO {
        required: boolean;        
    }

    export class FileControlDefinitionDTO extends ControlDefinitionDTO {

    }

    export class RadioButtonOption extends ControlDefinitionDTO implements ISupportsNestedFields {
        selected: boolean;
        controls: Array<ControlDefinitionDTO>;
    }

    export class RadioButtonGroupControlDefinitionDTO extends ControlDefinitionDTO {
        groupName: string;
        radios: Array<RadioButtonOption>;
    }

    export class TextBlock extends ControlDefinitionDTO {
        class: string;

        constructor(type: string, value: string, _class: string) {
            super();
            this.type = type;
            this.value = value;
            this.class = _class;
        }
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

    export class DropDownListControlDefinitionDTO extends ControlDefinitionDTO {
        listItems: Array<DropDownListItem>;
        source: FieldSource;
    }

    export class TextBlockField extends ControlDefinitionDTO {
        public class: string;
    }

    export class TextAreaControlDefinitionDTO extends ControlDefinitionDTO {
    }

    export class MappingPaneControlDefinitionDTO extends ControlDefinitionDTO {
    }

    export class RoutingControlGroup extends ControlDefinitionDTO {
        sourceField: string;
        routes: Array<Route>
    }

    export class Route extends ControlDefinitionDTO {
        measurementValue: string;
        selection: string;
        previousActionList: RouteActionList;
        previousActionSelectedId: string;
        availableProcessNode: string;
    }

    export class RouteActionList extends ControlDefinitionDTO {
        choices: Array<Choice>;
        selectionId: string;
    }

    export class Choice extends ControlDefinitionDTO {
        Label: string;
        Id: string;
    }
}