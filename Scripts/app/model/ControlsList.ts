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
        label: string;
        name: string;
        errorMessage : string;
        events: Array<ControlEvent>;
        value: string;
        isFocused: boolean;
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

        constructor() {
            super();
            this.type = "TextBox";
        }
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

        constructor(key: string, value: string) {
            super();
            this.key = key;
            this.value = value;
            
        }
    }

    export class FieldSource {
        public manifestType: string;
        public label: string;
        public filterByTag: string;
        public requestUpstream: boolean;
    }

    export class DropDownList extends ControlDefinitionDTO {
        listItems: Array<DropDownListItem>;
        source: FieldSource;
        selectedKey: string;

        constructor() {
            super();
            this.type = "DropDownList";
        }
    }

    export class FilterConditionDTO {
        field: string;
        operator: string;
        value: string;
    }

    export class ControlMetaDescriptionDTO {
        public controls: Array<ControlDefinitionDTO> = [];
        public type: string;
        public description: string;
        
        constructor(type: string, description: string) {
            this.type = type;
            this.description = description;
        }
    }
    
    export class TextBoxMetaDescriptionDTO extends ControlMetaDescriptionDTO
    {
        constructor() {
            super("TextBoxMetaDescriptionDTO", "TextBox");
            var tb = new model.TextBox();
            tb.label = "Label :";
            this.controls.push(tb);
        }
    }
    
    export class TextBlockMetaDescriptionDTO extends ControlMetaDescriptionDTO
    {
        constructor() {
            super("TextBlockMetaDescriptionDTO", "TextBlock");
            var tb = new model.TextBox();
            tb.label = "Label :";
            this.controls.push(tb);
        }
    }
    
    export class FilePickerMetaDescriptionDTO extends ControlMetaDescriptionDTO
    {
        static fileExtensions: Array<string> = ["xlsx"];
        constructor() {
            super("FilePickerMetaDescriptionDTO", "File Picker");
            var tb = new model.TextBox();
            tb.label = "Label :";
            this.controls.push(tb);

            var listItems: Array<DropDownListItem> = [];
            for (var i = 0; i < FilePickerMetaDescriptionDTO.fileExtensions.length; i++) {
                var extensionValue = FilePickerMetaDescriptionDTO.fileExtensions[i];
                listItems.push(new DropDownListItem(extensionValue, extensionValue));
            }
            var allowedExtensions = new model.DropDownList();
            allowedExtensions.listItems = listItems;
            allowedExtensions.label = "File Type:";
            this.controls.push(allowedExtensions);
        }

    }
    
    

    export class ControlContainer extends ControlDefinitionDTO {
        metaDescriptions: Array<ControlMetaDescriptionDTO>;
    }

    export class ContainerTransitionField {
        conditions: Array<FilterConditionDTO>;
        transition: number;
        targetNodeId: string;

        constructor() {
            this.conditions = [];
            this.transition = 0;
            this.targetNodeId = null;
        }
    }

    export class ContainerTransition extends ControlDefinitionDTO {
        transitions: Array<ContainerTransitionField>;
    }

    export class CrateDetails {
        manifestType: model.DropDownList;
        label: model.DropDownList;
    }

    export class UpstreamCrateChooser extends ControlDefinitionDTO {
        selectedCrates: Array<CrateDetails>;
        multiSelection: boolean;
    }

    export class CrateChooser extends ControlDefinitionDTO {
        crateDescriptions: Array<CrateDescriptionDTO>;
        singleManifestOnly: boolean;
        requestUpstream: boolean;
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
        plans: Array<Plan>;
    }

    export class Plan extends ControlDefinitionDTO {
        measurementValue: string;
        selection: string;
        previousActionList: PlanActionList;
        previousActionSelectedId: string;
        availableProcessNode: string;
    }

    export class PlanActionList extends ControlDefinitionDTO {
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
        innerLabel: string;
    }

    export class UpstreamDataChooser extends ControlDefinitionDTO {
        selectedManifest: string;
        selectedLabel: string;
        selectedFieldType: string;
    }
    
    export class SourceableCriteria extends DropDownList{
        fieldList: Array<DropDownListItem>;
        comparatorList: Array<DropDownListItem>;
        valueSource: string;
        textValue: string;
    }
}