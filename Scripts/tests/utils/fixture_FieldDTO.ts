
module dockyard.tests.utils.fixtures {

    export class FieldDTO {

        public static newRoute = <interfaces.IRouteVM> {
            name: 'Test',
            description: 'Description',
            routeState: 1
        };

        public static filePickerField: model.File = {
            type: 'FilePicker',
            fieldLabel: 'FilePicker Test',
            name: 'FilePickerTest',
            events: [],
            value: null
        };

        public static textField: model.TextBox = {
            required: true,
            type: 'TextBox',
            fieldLabel: 'test',
            name: 'test',
            events: [],
            value: 'test'
        };

        public static textBlock: model.TextBlock = new model.TextBlock('<span>teststs</span>', 'well well-lg');

        public static dropDownListBox: model.DropDownList = {
            listItems: [{ key: 'test1', value: 'value1', availability: model.AvailabilityType.Configuration }, { key: 'test2', value: 'value2', availability: model.AvailabilityType.Configuration }, { key: 'test3', value: 'value3', availability: model.AvailabilityType.Configuration  }],
            source: {
                manifestType: 'testManifest',
                label: 'testLabel'
            },
            type: 'DropDownList',
            fieldLabel: 'DropDownList Test',
            name: 'DropDownList',
            events: [],
            value: 'value3',
            selectedKey: 'test3'
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
                    events: null,
                    controls: [
                        {
                            name: 'SMS_Number',
                            value: null,
                            fieldLabel: null,
                            type: "TextBox",
                            events: null
                        }
                    ]
                },
                {
                    selected: false,
                    name: 'SMSNumberOption',
                    value: 'A value from Upstream Crate',
                    type: "RadioButtonGroup",
                    fieldLabel: null,
                    events: null,
                    controls: [
                        {
                            name: 'SMS_Number2',
                            value: null,
                            fieldLabel: null,
                            type: "TextBox",
                            events: null
                        }
                    ]
                }
            ],
            name: '',
            value: null,
            fieldLabel: "For the SMS Number use:",
            type: "RadioButtonGroup",
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
            value: null,
            source: {
                manifestType: 'testManifest',
                label: 'testLabel'
            },
            valueSource: 'test',
            listItems: [{ key: 'test1', value: 'value1', availability: model.AvailabilityType.Configuration }, { key: 'test2', value: 'value2', availability: model.AvailabilityType.Configuration }, { key: 'test3', value: 'value3', availability: model.AvailabilityType.Configuration }],
            name: 'test name',
            fieldLabel: 'test label',
            selectedKey: null
        };
    }
} 