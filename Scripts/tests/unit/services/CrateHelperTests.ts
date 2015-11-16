/// <reference path="../../../app/_all.ts" />
/// <reference path="../../../typings/angularjs/angular-mocks.d.ts" />
 

module dockyard.tests.controller {

    import CrateHelper = dockyard.services.CrateHelper;



    describe('CrateHelper', () => {

        var ch, crateStorage, emptyStorage, duplicateCrateStorage, controlList, fields;
        beforeEach(() => {
            ch = new CrateHelper();

            crateStorage = {
                "crates": [
                    {
                        "id": "435532ca-b294-44c5-be09-f72338b8dd3e",
                        "label": "Configuration_Controls",
                        "contents": {
                            "Controls": [
                                {
                                    "groupName": "SMSNumber_Group",
                                    "radios": [
                                        {
                                            "selected": true,
                                            "name": "SMSNumberOption",
                                            "value": "SMS Number",
                                            "controls": [
                                                {
                                                    "name": "SMS_Number",
                                                    "required": true,
                                                    "value": null,
                                                    "label": null,
                                                    "type": "TextBox",
                                                    "selected": false,
                                                    "events": null,
                                                    "source": null
                                                }
                                            ]
                                        },
                                        {
                                            "selected": true,
                                            "name": "SMSNumberOption",
                                            "value": "A value from Upstream Crate",
                                            "controls": [
                                                {
                                                    "listItems": [

                                                    ],
                                                    "name": "upstream_crate",
                                                    "required": false,
                                                    "value": null,
                                                    "label": null,
                                                    "type": "DropDownList",
                                                    "selected": false,
                                                    "events": [
                                                        {
                                                            "name": "onChange",
                                                            "handler": "requestConfig"
                                                        }
                                                    ],
                                                    "source": {
                                                        "manifestType": "Standard Design-Time Fields",
                                                        "label": "Available Fields"
                                                    }
                                                }
                                            ]
                                        }
                                    ],
                                    "name": null,
                                    "required": false,
                                    "value": null,
                                    "label": "For the SMS Number use:",
                                    "type": "RadioButtonGroup",
                                    "selected": false,
                                    "events": null,
                                    "source": null
                                },
                                {
                                    "name": "SMS_Body",
                                    "required": true,
                                    "value": null,
                                    "label": "SMS Body",
                                    "type": "TextBox",
                                    "selected": false,
                                    "events": null,
                                    "source": null
                                }
                            ]
                        },
                        "parentCrateId": null,
                        "manifestType": "Standard Configuration Controls",
                        "manifestId": 6,
                        "manufacturer": null,
                        "createTime": "0001-01-01T00:00:00"
                    },
                    {
                        "id": "97f09d75-6810-45c3-9bdb-56912e4606c8",
                        "label": "Available Fields",
                        "contents": {
                            "Fields": [
                                {
                                    "key": "+15005550006",
                                    "value": "+15005550006"
                                }
                            ]
                        },
                        "parentCrateId": null,
                        "manifestType": "Standard Design-Time Fields",
                        "manifestId": 3,
                        "manufacturer": null,
                        "createTime": "0001-01-01T00:00:00"
                    },
                    {
                        "id": "97f09d75-6810-45c3-9bdb-56912e4606c8",
                        "label": "Available Fields1",
                        "contents": {
                            "Fields": [
                                {
                                    "key": "Test",
                                    "value": "test"
                                }
                            ]
                        },
                        "parentCrateId": null,
                        "manifestType": "Standard Design-Time Fields1",
                        "manifestId": 5,
                        "manufacturer": null,
                        "createTime": "0001-01-01T00:00:00"
                    },
                    {
                        "id": "97f09d75-6810-45c3-9bdb-56912e4606c8",
                        "label": "Available Fields2",
                        "contents": {
                            "Fields": [
                                {
                                    "key": "Test1",
                                    "value": "test1"
                                }
                            ]
                        },
                        "parentCrateId": null,
                        "manifestType": "Standard Design-Time Fields2",
                        "manifestId": 5,
                        "manufacturer": null,
                        "createTime": "0001-01-01T00:00:00"
                    }
                ]
            };

            emptyStorage = {
                crates: []
            };

            duplicateCrateStorage = {
                crates: [
                    {
                        "id": "97f09d75-6810-45c3-9bdb-56912e4606c8",
                        "label": "Available Fields",
                        "contents": {
                            "Fields": [
                                {
                                    "key": "+15005550006",
                                    "value": "+15005550006"
                                }
                            ]
                        },
                        "parentCrateId": null,
                        "manifestType": "Standard Design-Time Fields",
                        "manifestId": 3,
                        "manufacturer": null,
                        "createTime": "0001-01-01T00:00:00"
                    },
                    {
                        "id": "97f09d75-6810-45c3-9bdb-56912e4606c8",
                        "label": "Available Fields",
                        "contents": {
                            "Fields": [
                                {
                                    "key": "+15005550006",
                                    "value": "+15005550006"
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
            };

            controlList = {
                fields: [
                    {
                        "name": "SMS_Number",
                        "required": true,
                        "value": null,
                        "label": null,
                        "type": "TextBox",
                        "selected": false,
                        "events": null,
                        "source": null
                    },
                    {
                        "name": "SMS_Body",
                        "required": true,
                        "value": null,
                        "label": "SMS Body",
                        "type": "TextBox",
                        "selected": false,
                        "events": null,
                        "source": null
                    }
                ]
            };

            fields = [
                {
                    "groupName": "SMSNumber_Group",
                    "radios": [
                        {
                            "selected": true,
                            "name": "SMSNumberOption",
                            "value": "SMS Number",
                            "controls": [
                                {
                                    "name": "SMS_Number",
                                    "required": true,
                                    "value": null,
                                    "label": null,
                                    "type": "TextBox",
                                    "selected": false,
                                    "events": null,
                                    "source": null
                                }
                            ]
                        },
                        {
                            "selected": true,
                            "name": "SMSNumberOption",
                            "value": "A value from Upstream Crate",
                            "controls": [
                                {
                                    "listItems": [],
                                    "name": "upstream_crate",
                                    "required": false,
                                    "value": null,
                                    "label": null,
                                    "type": "DropDownList",
                                    "selected": false,
                                    "events": [
                                        {
                                            "name": "onChange",
                                            "handler": "requestConfig"
                                        }
                                    ],
                                    "source": {
                                        "manifestType": "Standard Design-Time Fields",
                                        "label": "Available Fields"
                                    }
                                },
                                {
                                    "listItems": [],
                                    "name": "upstream_crate",
                                    "required": false,
                                    "value": null,
                                    "label": null,
                                    "type": "TextSource",
                                    "selected": false,
                                    "events": [],
                                    "source": {
                                        "manifestType": "Standard Design-Time Fields2",
                                        "label": "Available Fields2"
                                    },
                                    "initialLabel": "Initial label",
                                    "valueSource": "Value source"
                                }

                            ]
                        }
                    ],
                    "name": null,
                    "required": false,
                    "value": null,
                    "label": "For the SMS Number use:",
                    "type": "RadioButtonGroup",
                    "selected": false,
                    "events": null,
                    "source": null
                },
                {
                    "name": "SMS_Body",
                    "required": true,
                    "value": null,
                    "label": "SMS Body",
                    "type": "TextBox",
                    "selected": false,
                    "events": null,
                    "source": null
                },
                {
                    "listItems": [],
                    "name": "upstream_crate1",
                    "required": false,
                    "value": null,
                    "label": null,
                    "type": "DropDownList",
                    "selected": false,
                    "events": [],
                    "source": {
                        "manifestType": "Standard Design-Time Fields1",
                        "label": "Available Fields1"
                    }
                }
            ];
        });

        describe('.findByLabel()', () => {

            it('should return a create with specified label', () => {
                crateStorage.crates.forEach((crate) => {
                    if (crate.label) {
                        var found = ch.findByLabel(crateStorage, crate.label);
                        expect(crate).toBe(found);
                    }
                });
            });

            it('should throw an exception if the provided storage is empty', () => {
                expect(ch.findByLabel.bind(ch, emptyStorage, 'label')).toThrow();
            });

            it('should throw an exception if nothing is found', () => {
                expect(ch.findByLabel.bind(ch, crateStorage, 'notExistingLabel')).toThrow();
            });

            xit('should throw an exception if more then one crate is found', () => {
                expect(ch.findByLabel.bind(ch, duplicateCrateStorage, duplicateCrateStorage.crates[0].label)).toThrow(); 
            });

        });

        describe('.findByManifestType()', () => {

            it('should return a create with specified manifest type', () => {
                crateStorage.crates.forEach((crate) => {
                    if (crate.manifestType) {
                        var found = ch.findByManifestType(crateStorage, crate.manifestType);
                        expect(crate).toBe(found);
                    }
                });
            });

            it('should throw an exception if the provided storage is empty', () => {
                expect(ch.findByManifestType.bind(ch, emptyStorage, 'type')).toThrow();
            });

            it('should throw an exception if nothing is found', () => {
                expect(ch.findByManifestType.bind(ch, crateStorage, 'notExistingType')).toThrow();
            });

            xit('should throw an exception if more then one crate is found', () => {
                expect(ch.findByManifestType.bind(ch, duplicateCrateStorage, duplicateCrateStorage.crates[0].manifestType)).toThrow();
            });

        });

        describe('.findByManifestTypeAndLabel()', () => {

            it('should return a create with specified manifest type and label', () => {
                crateStorage.crates.forEach((crate) => {
                    if (crate.manifestType && crate.label) {
                        var found = ch.findByManifestTypeAndLabel(crateStorage, crate.manifestType, crate.label);
                        expect(crate).toBe(found);
                    }
                });
            });

            xit('should throw an exception if the provided storage is empty', () => {
                expect(ch.findByManifestTypeAndLabel.bind(ch, emptyStorage, 'type', 'label')).toThrow();
            });

            // TODO: This is inconsistent with previous two methods, need to check again
            it('should return null if nothing is found', () => {
                expect(ch.findByManifestTypeAndLabel(crateStorage, 'notExistingType', 'notExistingLabel')).toBe(null);
            });

            xit('should throw an exception if more then one crate is found', () => {
                expect(ch.findByManifestTypeAndLabel.bind(ch, duplicateCrateStorage, duplicateCrateStorage.crates[0].manifestType, duplicateCrateStorage.crates[0].label)).toThrow();
            });

        });

        describe('.mergeControlListCrate()', () => {

            it('should replace the Controls of "Standard Configuration Controls" type crate in storage by the given control list', () => {
                ch.mergeControlListCrate(controlList, crateStorage);
                var targetCrate = ch.findByManifestType(crateStorage, 'Standard Configuration Controls');
                expect(targetCrate.contents.Controls).toBe(controlList.fields);
            });

            it('should not change other crates', () => {
                var copy = $.extend({}, crateStorage);
                ch.mergeControlListCrate(controlList, crateStorage);
                copy.crates.forEach((crate, index) => {
                    if (crate.manifestType !== 'Standard Configuration Controls') {
                        expect(crate).toEqual(crateStorage.crates[index]);
                    }
                });
            });

        });

        describe('.populateListItemsFromDataSource()', () => {

            var flatFieldList;
            beforeEach(() => {
                flatFieldList = [];
                addToFlatList(fields);
            });

            function addToFlatList(items) {
                items.forEach((field) => {
                    flatFieldList.push(field);
                    if (field.radios || field.controls) {
                        addToFlatList(field.radios || field.controls);
                    }
                });
            }

            it('should set the listItems property of "DropDownList" from a correct crate', () => {
                ch.populateListItemsFromDataSource(fields, crateStorage);

                flatFieldList.forEach((field) => {
                    if (field.type === 'DropDownList') {
                        var crate = ch.findByManifestTypeAndLabel(crateStorage, field.source.manifestType, field.source.label);
                        expect(field.listItems.length > 0).toBe(true);
                        expect(field.listItems).toEqual(crate.contents.Fields);
                    }
                });
            });

            it('should set the listItems property of "TextSource" from a correct crate', () => {
                ch.populateListItemsFromDataSource(fields, crateStorage);

                flatFieldList.forEach((field) => {
                    if (field.type === 'TextSource') {
                        var crate = ch.findByManifestTypeAndLabel(crateStorage, field.source.manifestType, field.source.label);
                        expect(field.listItems.length > 0).toBe(true);
                        expect(field.listItems).toEqual(crate.contents.Fields);
                    }
                });
            });

            it('should not change other fields', () => {
                var copy = $.extend({}, flatFieldList);
                flatFieldList.forEach((field, index) => {
                    if (field.type !== 'DropDownList' && field.type !== 'TextSource' && !field.radios && !field.controls) {
                        expect(field).toEqual(copy[index]);
                    }
                });
            });

        });

    });

}