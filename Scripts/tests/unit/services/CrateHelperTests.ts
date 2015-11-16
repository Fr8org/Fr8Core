/// <reference path="../../../app/_all.ts" />
/// <reference path="../../../typings/angularjs/angular-mocks.d.ts" />
 

module dockyard.tests.controller {

    import CrateHelper = dockyard.services.CrateHelper;
    import fx = utils.fixtures;


    describe('CrateHelper', () => {

        var ch, crateStorage, emptyStorage, duplicateCrateStorage, controlList, fields;
        beforeEach(() => {
            ch = new CrateHelper();
            crateStorage = $.extend(true, {}, fx.CrateHelper.sampleStorage);
            emptyStorage = $.extend(true, {}, fx.CrateHelper.emptyStorage);
            duplicateCrateStorage = $.extend(true, {}, fx.CrateHelper.duplicateStorage);
            controlList = $.extend(true, {}, fx.CrateHelper.controlsList);
            fields = $.extend(true, [], fx.CrateHelper.fields);
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
                var copy = $.extend(true, {}, crateStorage);
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
                var copy = $.extend(true, {}, flatFieldList);
                flatFieldList.forEach((field, index) => {
                    if (field.type !== 'DropDownList' && field.type !== 'TextSource' && !field.radios && !field.controls) {
                        expect(field).toEqual(copy[index]);
                    }
                });
            });

        });

    });

}