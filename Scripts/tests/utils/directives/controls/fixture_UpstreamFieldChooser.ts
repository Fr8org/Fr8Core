module dockyard.tests.utils.fixtures {
    export class UpstreamFieldChooser {
        public static incomingFields = <any[]>[
            {
                availability:2,
                isRequired: false,
                key: 'CurrentRecipientEmail',
                label:'DocuSign Envelope Fields'
            }
            ,
            {
                availability: 2,
                isRequired: false,
                key: 'CurrentRecipientUserName',
                label: 'DocuSign Envelope Fields'
            }
        ]
    }
}