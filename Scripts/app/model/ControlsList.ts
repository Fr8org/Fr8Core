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
        errorMessage : string;
        events: Array<ControlEvent>;
        value: string;
    }

    export class ControlEvent {
        name: string;
        handler: string;
    }

    export class CheckBox extends ControlDefinitionDTO {
        selected: boolean;
    }

    export class Button extends ControlDefinitionDTO {
        checked: boolean;

        constructor(label: string) {
            super();
            this.type = 'Button';
            (<any>this).label = label;
        }
    }

    export class TextBox extends ControlDefinitionDTO {
        required: boolean;        
    }

    export class File extends ControlDefinitionDTO {

    }

    export class RadioButtonOption extends ControlDefinitionDTO implements ISupportsNestedFields {
        selected: boolean;
        controls: Array<ControlDefinitionDTO>;
    }
    
    export class RadioButtonGroup extends ControlDefinitionDTO {
        groupName: string;
        radios: Array<RadioButtonOption>;
    }

    export class TextBlock extends ControlDefinitionDTO {
        class: string;

        constructor(value: string, _class: string) {
            super();
            this.type = 'TextBlock';
            this.value = value;
            this.class = _class;
        }
    }

    export class FieldDTO {
        public key: string;
        public value: string;
        public availability: AvailabilityType;
        public tags: string;
        public sourceCrateLabel: string;
        public sourceCrateManifest: {
            Id: string;
            Type: string;
        };
    }

    export enum AvailabilityType {
        Configuration = 1,
        RunTime = 2,
        Always = 3
    }

    export class DropDownListItem extends FieldDTO {
        selected: boolean;
    }

    export class FieldSource {
        public manifestType: string;
        public label: string;
        public filterByTag: string;
    }

    export class DropDownList extends ControlDefinitionDTO {
        listItems: Array<DropDownListItem>;
        source: FieldSource;
        selectedKey: string;
    }

    export class TextSource extends DropDownList {
        initialLabel: string;
        valueSource: string;
        textValue: string;
    }

    export class TextBlockField extends ControlDefinitionDTO {
        public class: string;
    }

    export class TextArea extends ControlDefinitionDTO {
        isReadOnly:boolean;
    }

    export class MappingPane extends ControlDefinitionDTO {
    }

    export class RoutingControlGroup extends ControlDefinitionDTO {
        sourceField: string;
        routes: Array<Route>;
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

    export class Duration extends ControlDefinitionDTO {
        days: number;
        hours: number;
        minutes: number;
    }

    export class UpstreamDataChooser extends ControlDefinitionDTO {
        selectedManifest: string;
        selectedLabel: string;
        selectedFieldType: string;
    }
}