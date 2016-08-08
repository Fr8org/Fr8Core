
module dockyard.tests.utils.fixtures {

    export class FieldDTO {

        public static newPlan = <interfaces.IPlanVM>{
            name: 'Test',
            description: 'Description',
            planState: model.PlanState.Inactive
        };

        public static filePickerField: model.File = {
            type: 'FilePicker',
            fieldLabel: 'FilePicker Test',
            label: 'FilePicker Test',
            name: 'FilePickerTest',
            events: [],
            value: null,
            errorMessage: null,
            isFocused: false,
            isHidden:false
        };

        public static textField: model.TextBox = {
            required: true,
            type: 'TextBox',
            fieldLabel: 'test',
            name: 'test',
            events: [],
            errorMessage: null,
            isFocused: false,
            value: 'test',
            label: 'test',
            isHidden: false
        };

        public static textBlock: model.TextBlock = new model.TextBlock('<p>teststs</p>', 'well well-lg');

        public static dropDownListBox: model.DropDownList = {
            listItems: [
                { key: 'test1', data: null, fieldType: null, selected: false, value: 'value1', tags: null, availability: model.AvailabilityType.Configuration, sourceCrateLabel: null, sourceCrateManifest: null, sourceActivityId: null },
                { key: 'test2', data: null, fieldType: null, selected: false, value: 'value2', tags: null, availability: model.AvailabilityType.Configuration, sourceCrateLabel: null, sourceCrateManifest: null, sourceActivityId: null },
                { key: 'test3', data: null, fieldType: null, selected: false, value: 'value3', tags: null, availability: model.AvailabilityType.Configuration, sourceCrateLabel: null, sourceCrateManifest: null, sourceActivityId: null }
            ],
            source: {
                manifestType: 'testManifest',
                label: 'testLabel',
                filterByTag: null,
                requestUpstream: false,
                availabilityType: model.AvailabilityType.NotSet,
            },
            type: 'DropDownList',
            fieldLabel: 'DropDownList Test',
            label: 'DropDownList Test',
            name: 'DropDownList',
            isFocused: false,
            events: [],
            value: 'value3',
            errorMessage: null,
            selectedKey: 'test3',
            isHidden: false,
            hasRefreshButton: false,
            selectedItem: null
        };

        public static radioButtonGroupField: model.RadioButtonGroup = {
            groupName: 'SMSNumber_Group',
            radios: [
                {
                    selected: false,
                    name: 'SMSNumberOption',
                    value: 'SMS Number',
                    type: "RadioButtonGroup",
                    fieldLabel: null,
                    label: null,
                    events: null,
                    errorMessage: null,
                    isFocused: false,
                    isHidden: false,
                    controls: [
                        {
                            name: 'SMS_Number',
                            value: null,
                            fieldLabel: null,
                            label: null,
                            type: "TextBox",
                            events: null,
                            errorMessage: null,
                            isFocused: false,
                            isHidden: false
                        }
                    ]
                },
                {
                    selected: false,
                    name: 'SMSNumberOption',
                    value: 'A value from Upstream Crate',
                    type: "RadioButtonGroup",
                    fieldLabel: null,
                    label: null,
                    events: null,
                    errorMessage: null,
                    isFocused: false,
                    isHidden: false,
                    controls: [
                        {
                            name: 'SMS_Number2',
                            value: null,
                            fieldLabel: null,
                            label: null,
                            type: "TextBox",
                            events: null,
                            isFocused: false,
                            errorMessage: null,
                            isHidden: false
                        }
                    ]
                }
            ],
            name: '',
            value: null,
            fieldLabel: "For the SMS Number use:",
            label: "For the SMS Number use:",
            type: "RadioButtonGroup",
            errorMessage: null,
            isFocused: false,
            isHidden: false,
            events: null
        };


        public static designTimeField = {
            Key: 'test2',
            Value: 'value'
        };

        public static fieldList = {
            value: JSON.stringify([FieldDTO.designTimeField]),
            field: 'test2'
        };


        public static textSource: model.TextSource = {
            type: "TextSource",
            events: [],
            initialLabel: 'test label',
            label: null,
            value: null,
            textValue: null,
            errorMessage: null,
            isFocused: false,
            isHidden: false,
            isCollapsed: false,
            hasRefreshButton: false,
            groupLabelText: null,
            source: {
                manifestType: 'testManifest',
                label: 'testLabel',
                filterByTag: null,
                requestUpstream: false,
                availabilityType: model.AvailabilityType.NotSet
            },
            valueSource: '',
            listItems: [
                {
                    key: 'test1',
                    selected: false,
                    value: 'value1',
                    tags: null,
                    availability: model.AvailabilityType.Configuration,
                    sourceCrateLabel: null,
                    sourceCrateManifest: null,
                    sourceActivityId: null,
                    fieldType: null,
                    data: null
                },
                {
                    key: 'test2',
                    selected: false,
                    value: 'value2',
                    tags: null,
                    availability: model.AvailabilityType.Configuration,
                    sourceCrateLabel: null,
                    sourceCrateManifest: null,
                    sourceActivityId: null,
                    fieldType: null,
                    data: null
                },
                {
                    key: 'test3',
                    selected: false,
                    value: 'value3',
                    tags: null,
                    availability: model.AvailabilityType.Configuration,
                    sourceCrateLabel: null,
                    sourceCrateManifest: null,
                    sourceActivityId: null,
                    fieldType: null,
                    data: null
                }],
            name: 'test name',
            fieldLabel: 'test label',
            selectedKey: null,
            selectedItem: null
        };
    }
} 