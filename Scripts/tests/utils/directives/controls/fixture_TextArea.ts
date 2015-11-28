module dockyard.tests.utils.fixtures {

    export class TextArea {

        public static sampleField = <model.TextArea> {
            type: 'textArea',
            fieldLabel: 'Label',
            name: 'duration1',
            events: [],
            isReadOnly: false,
            value: 'Text value'
        }

        public static readOnlyField = <model.TextArea> {
            type: 'textArea',
            fieldLabel: 'Label1',
            name: 'duration2',
            events: [],
            isReadOnly: true,
            value: 'Text value 1'
        }

    }

}