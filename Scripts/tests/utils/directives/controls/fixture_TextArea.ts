module dockyard.tests.utils.fixtures {

    export class TextAreaDefinitionDTO {

        public static sampleField = <model.TextAreaControlDefinitionDTO> {
            type: 'textArea',
            fieldLabel: 'Label',
            name: 'duration1',
            events: [],
            isReadOnly: false,
            value: 'Text value'
        }

        public static readOnlyField = <model.TextAreaControlDefinitionDTO> {
            type: 'textArea',
            fieldLabel: 'Label1',
            name: 'duration2',
            events: [],
            isReadOnly: true,
            value: 'Text value 1'
        }

    }

}