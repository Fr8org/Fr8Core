module dockyard.tests.utils.fixtures {

    export class DurationControlDefinitionDTO {
        
        public static sampleField = <model.DurationControlDefinitionDTO> {
            type: 'duration',
            fieldLabel: 'Label',
            name: 'duration1',
            events: [],
            days: 5,
            hours: 2,
            minutes: 30
        }

    }

}