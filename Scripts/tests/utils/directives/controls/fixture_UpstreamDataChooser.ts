module dockyard.tests.utils.fixtures {

    export class UpstreamDataChooser {

        public static sampleField = <model.UpstreamDataChooser> {
            type: 'UpstreamDataChooser',
            name: 'upstreamDataChooser1',
            events: [],
            selectedManifest: null,
            selectedLabel: null,
            selectedFieldType: null,
            value: 'HolderEmail'
        }

        public static fieldWithValues = <model.UpstreamDataChooser> {
            type: 'UpstreamDataChooser',
            name: 'upstreamDataChooser2',
            events: [],
            selectedManifest: 'Standard Design-Time Fields',
            selectedLabel: 'Available Templates',
            selectedFieldType: 'Date'
        }

        public static sampleAction = <model.ActivityDTO> {
            crateStorage: {
                crates: [
                    <model.Crate> {
                        "id": "b4852fd7-6ea3-46a2-9d52-4489546b613d",
                        "label": "Configuration_Controls",
                        "contents": {
                            "Controls": [
                                {
                                    "selectedManifest": null,
                                    "selectedLabel": null,
                                    "selectedFieldType": null,
                                    "name": null,
                                    "required": false,
                                    "value": null,
                                    "label": null,
                                    "type": "UpstreamDataChooser",
                                    "selected": false,
                                    "events": null,
                                    "source": null
                                }
                            ]
                        },
                        "parentCrateId": null,
                        "manifestType": "Standard UI Controls",
                        "manifestId": "6",
                        "manufacturer": null,
                        "createTime": "0001-01-01T00:00:00"
                    },
                    <model.Crate> {
                        "id": "0e69ac7e-61e1-45c4-a7dc-4b4977aa41ed",
                        "label": "Upstream Manifest Type List",
                        "contents": {
                            "Fields": [
                                {
                                    "key": "3",
                                    "value": "Standard Design-Time Fields",
                                    "tags": null
                                },
                                {
                                    "key": "999",
                                    "value": "Test Manifest",
                                    "tags": null
                                }
                            ]
                        },
                        "parentCrateId": null,
                        "manifestType": "Standard Design-Time Fields",
                        "manifestId": "3",
                        "manufacturer": null,
                        "createTime": "0001-01-01T00:00:00"
                    },
                    <model.Crate> {
                        "id": "2071b642-9a85-4e7d-9cdc-532d24b5b5ad",
                        "label": "Upstream Crate Label List",
                        "contents": {
                            "Fields": [
                                {
                                    "key": null,
                                    "value": "Configuration_Controls",
                                    "tags": null
                                },
                                {
                                    "key": null,
                                    "value": "Available Templates",
                                    "tags": null
                                },
                                {
                                    "key": null,
                                    "value": "DocuSign Event Fields",
                                    "tags": null
                                },
                                {
                                    "key": null,
                                    "value": "Standard Event Subscriptions",
                                    "tags": null
                                }
                            ]
                        },
                        "parentCrateId": null,
                        "manifestType": "Standard Design-Time Fields",
                        "manifestId": "3",
                        "manufacturer": null,
                        "createTime": "0001-01-01T00:00:00"
                    },
                    {
                        "id": "e47c7d1b-3c1f-468f-b84a-0ff3067e1397",
                        "label": "Upstream Terminal-Provided Fields",
                        "contents": {
                            "Fields": [
                                {
                                    "key": "RecipientEmail",
                                    "value": null,
                                    "tags": "EmailAddress",
                                    "sourceCrateLabel": "Label1",
                                    "sourceCrateManifest": {
                                        "Id": "3",
                                        "Type": "Standard Design-Time Fields"
                                    }
                                },
                                {
                                    "key": "DocumentName",
                                    "value": null,
                                    "tags": null,
                                    "sourceCrateLabel": "Label1",
                                    "sourceCrateManifest": {
                                        "Id": "3",
                                        "Type": "Standard Design-Time Fields"
                                    }
                                },
                                    {
                                    "key": "CompletedDate",
                                    "value": null,
                                    "tags": "Date",
                                    "sourceCrateLabel": "Label2",
                                    "sourceCrateManifest": {
                                        "Id": "3",
                                        "Type": "Standard Design-Time Fields"
                                    }
                                },
                                {
                                    "key": "HolderEmail",
                                    "value": null,
                                    "tags": "EmailAddress",
                                    "sourceCrateLabel": "Label2",
                                    "sourceCrateManifest": {
                                        "Id": "3",
                                        "Type": "Standard Design-Time Fields"
                                    }
                                },
                                {
                                    "key": "Subject",
                                    "value": null,
                                    "tags": null,
                                    "sourceCrateLabel": "Label2",
                                    "sourceCrateManifest": {
                                        "Id": "3",
                                        "Type": "Standard Design-Time Fields"
                                    }
                                }
                            ]
                        },
                        "parentCrateId": null,
                        "manifestType": "Standard Design-Time Fields",
                        "manifestId": 3,
                        "manufacturer": null,
                        "createTime": "0001-01-01T00:00:00"
                    }
                ]
            },
        }

        public static actionWithoutListCrates = <model.ActivityDTO> {
            crateStorage: {
                crates: [
                    <model.Crate> {
                        "id": "b4852fd7-6ea3-46a2-9d52-4489546b613d",
                        "label": "Configuration_Controls",
                        "contents": {
                            "Controls": [
                                {
                                    "selectedManifest": null,
                                    "selectedLabel": null,
                                    "selectedFieldType": null,
                                    "name": null,
                                    "required": false,
                                    "value": null,
                                    "label": null,
                                    "type": "UpstreamDataChooser",
                                    "selected": false,
                                    "events": null,
                                    "source": null
                                }
                            ]
                        },
                        "parentCrateId": null,
                        "manifestType": "Standard UI Controls",
                        "manifestId": "6",
                        "manufacturer": null,
                        "createTime": "0001-01-01T00:00:00"
                    }
                ]
            }
        }

        public static initialSettingsField = <model.UpstreamDataChooser> {
            type: 'UpstreamDataChooser',
            name: 'upstreamDataChooser1',
            events: [],
            selectedManifest: '',
            selectedLabel: null,
            selectedFieldType: null
        }

    }

}