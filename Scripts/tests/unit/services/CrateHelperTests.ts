/// <reference path="../../../app/_all.ts" />
/// <reference path="../../../typings/angularjs/angular-mocks.d.ts" />
 

module dockyard.tests.controller {

    import CrateHelper = dockyard.services.CrateHelper;
    import fx = utils.fixtures;
    import filterByTagFactory = dockyard.filters.filterByTag.factory;


    describe('CrateHelper', () => {

        var ch, crateStorage, emptyStorage, duplicateCrateStorage, controlList, fields;
        beforeEach(() => {
            ch = new CrateHelper(filterByTagFactory);
            crateStorage = $.extend(true, {}, fx.CrateHelper.sampleStorage);
            emptyStorage = $.extend(true, {}, fx.CrateHelper.emptyStorage);
            duplicateCrateStorage = $.extend(true, {}, fx.CrateHelper.duplicateStorage);
            controlList = $.extend(true, {}, fx.CrateHelper.controlsList);
            fields = $.extend(true, [], fx.CrateHelper.fields);
        });

        describe('.findByLabel()', () => {

            it('should return a crate with specified label', () => {
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

        });

        describe('.findByManifestType()', () => {

            it('should return a crate with specified manifest type', () => {
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

        });

        describe('.findByManifestTypeAndLabel()', () => {

            it('should return a crate with specified manifest type and label', () => {
                crateStorage.crates.forEach((crate) => {
                    if (crate.manifestType && crate.label) {
                        var found = ch.findByManifestTypeAndLabel(crateStorage, crate.manifestType, crate.label);
                        expect(crate).toBe(found);
                    }
                });
            });

            it('should throw an exception if the provided storage is empty', () => {
                expect(ch.findByManifestTypeAndLabel.bind(ch, emptyStorage, 'type', 'label')).toThrow();
            });

            // TODO: This is inconsistent with previous two methods, need to check again
            it('should return null if nothing is found', () => {
                expect(ch.findByManifestTypeAndLabel(crateStorage, 'notExistingType', 'notExistingLabel')).toBe(null);
            });

        });

        describe('.mergeControlListCrate()', () => {

            it('should replace the Controls of "Standard UI Controls" type crate in storage by the given control list', () => {
                ch.mergeControlListCrate(controlList, crateStorage);
                var targetCrate = ch.findByManifestType(crateStorage, 'Standard UI Controls');
                expect(targetCrate.contents.Controls).toEqual(controlList.fields);
            });

            it('should not change other crates', () => {
                var copy = $.extend(true, {}, crateStorage);
                ch.mergeControlListCrate(controlList, crateStorage);
                copy.crates.forEach((crate, index) => {
                    if (crate.manifestType !== 'Standard UI Controls') {
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
                    if (field.type === 'DropDownList' && !field.source.filterByTag) {
                        var crate = ch.findByManifestTypeAndLabel(crateStorage, field.source.manifestType, field.source.label);
                        expect(field.listItems.length > 0).toBe(true);
                        expect(field.listItems).toEqual(crate.contents.Fields);
                    }
                });
            });

            it('should set the listItems property of "TextSource" from a correct crate', () => {
                ch.populateListItemsFromDataSource(fields, crateStorage);

                flatFieldList.forEach((field) => {
                    if (field.type === 'TextSource' && !field.source.filterByTag) {
                        var crate = ch.findByManifestTypeAndLabel(crateStorage, field.source.manifestType, field.source.label);
                        expect(field.listItems.length > 0).toBe(true);
                        expect(field.listItems).toEqual(crate.contents.Fields);
                    }
                });
            });

            it('should not change other fields', () => {
                var copy = $.extend(true, {}, flatFieldList);
                flatFieldList.forEach((field, index) => {
                    if (field.type !== 'DropDownList' && field.type !== 'TextSource' && !field.radios && !field.controls) {
                        expect(field).toEqual(copy[index]);
                    }
                });
            });

            it('should filter the list items for DropDownList if filterByTag property is provided', () => {
                ch.populateListItemsFromDataSource(fields, crateStorage);

                flatFieldList.forEach((field) => {
                    if (field.type === 'DropDownList' && field.source.filterByTag) {
                        field.listItems.forEach((item) => {
                            expect(item.tags.indexOf(field.source.filterByTag)).not.toBe(-1);
                        });
                    }
                });
            });

            it('should filter the list items for TextSource if filterByTag property is provided', () => {
                ch.populateListItemsFromDataSource(fields, crateStorage);

                flatFieldList.forEach((field) => {
                    if (field.type === 'TextSource' && field.source.filterByTag) {
                        field.listItems.forEach((item) => {
                            expect(item.tags.indexOf(field.source.filterByTag)).not.toBe(-1);
                        });
                    }
                });
            });

        });

        describe('.hasCrateOfManifestType()', () => {

            it('should return true if there is a crate with specified manifest type', () => {
                expect(ch.hasCrateOfManifestType(crateStorage, 'Standard UI Controls')).toBe(true);
                expect(ch.hasCrateOfManifestType(crateStorage, 'Standard Design-Time Fields2')).toBe(true);
                expect(ch.hasCrateOfManifestType(duplicateCrateStorage, 'Standard Design-Time Fields')).toBe(true);
            });

            it('should return false if there is no such crate', () => {
                expect(ch.hasCrateOfManifestType(crateStorage, 'not existing manifest type')).toBe(false);
            });

            it('should return false if the storage is empty', () => {
                expect(ch.hasCrateOfManifestType(emptyStorage, 'Standard UI Controls')).toBe(false);
            });

        });

        describe('.createControlListFromCrateStorage()', () => {

            it('should return control list from the control configuration', () => {
                var controlList = ch.createControlListFromCrateStorage(crateStorage);
                var crate = ch.findByManifestType(crateStorage, 'Standard UI Controls');

                expect(controlList.fields).toBe(crate.contents.Controls);
            });

            it('should set the list items', () => {
                var flatFieldList = [];

                function addToFlatList(items) {
                    items.forEach((field) => {
                        flatFieldList.push(field);
                        if (field.radios || field.controls) {
                            addToFlatList(field.radios || field.controls);
                        }
                    });
                }

                var controlList = ch.createControlListFromCrateStorage(crateStorage);
                addToFlatList(controlList.fields);

                for (var field of flatFieldList) {
                    if (field.type === 'DropDownList') {
                        var crate = ch.findByManifestTypeAndLabel(crateStorage, field.source.manifestType, field.source.label);
                        expect(field.listItems.length > 0).toBe(true);
                        expect(field.listItems).toEqual(crate.contents.Fields);
                    }
                }
            });

        });

        describe('.getAvailableFieldTypes()', () => {

            it('should return the list of unique tags', () => {
                var list = ch.getAvailableFieldTypes(crateStorage);
                expect(list).toEqual(['EmailAddress', 'Date']);
            });

            it('should return an empty list if nothing is found', () => {
                var list = ch.getAvailableFieldTypes(emptyStorage);
                expect(list).toEqual([]);
            });
        });

    });

}