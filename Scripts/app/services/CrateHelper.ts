module dockyard.services {

    export class CrateHelper {

        constructor() {
        }

        public throwError(errorText: string) {
            alert(errorText);
            throw errorText;
        }

        public hasCrateOfManifestType(crateStorage: model.CrateStorage, manifestType: string): boolean {
            if (!crateStorage || !crateStorage.crates) {
                return false;
            }

            for (var i = 0; i < crateStorage.crates.length; ++i) {
                if (crateStorage.crates[i].manifestType == manifestType) {
                    return true;
                }
            }

            return false;
        }

        public hasControlListCrate(crateStorage: model.CrateStorage): boolean {
            return this.hasCrateOfManifestType(crateStorage, 'Standard Configuration Controls');
        }

        // Find single Crate by ManifestType in CrateStorage.
        public findByManifestType(crateStorage: model.CrateStorage, manifestType: string): model.Crate {
            // Check that CrateStorage is not empty.
            if (!crateStorage || !crateStorage.crates) {
                this.throwError('CrateStorage is empty.');
                return;
            }

            // First we put all found crates to "foundCrates" array.
            // We want to return only a single Crate, so we validate foundCrates.length later.
            var foundCrates: Array<model.Crate> = [];

            for (var i = 0; i < crateStorage.crates.length; ++i) {
                if (crateStorage.crates[i].manifestType == manifestType) {
                    foundCrates.push(crateStorage.crates[i]);
                    break;
                }
            }

            // Validate foundCrates.length that only single Crate was found.
            if (foundCrates.length == 0 || foundCrates.length > 1) {
                this.throwError('Invalid foundCrates.length = ' + foundCrates.length.toString());
                return;
            }

            // Return single Crate.
            return foundCrates[0];
        }

        // Find single Crate by ManifestType and Label in CrateStorage.
        public findByManifestTypeAndLabel(crateStorage: model.CrateStorage, manifestType: string, label: string): model.Crate {
            // Check that CrateStorage is not empty.
            if (!crateStorage || !crateStorage.crates) {
                this.throwError('CrateStorage is empty.');
                return;
            }

            // First we put all found crates to "foundCrates" array.
            // We want to return only a single Crate, so we validate foundCrates.length later.
            var foundCrates: Array<model.Crate> = [];

            for (var i = 0; i < crateStorage.crates.length; ++i) {
                if (crateStorage.crates[i].manifestType == manifestType && crateStorage.crates[i].label == label) {
                    foundCrates.push(crateStorage.crates[i]);
                    break;
                }
            }

            // Validate foundCrates.length that only single Crate was found.
            if (foundCrates.length == 0) {
                return null;
            }
            if (foundCrates.length > 1) {
                this.throwError('Invalid foundCrates.length = ' + foundCrates.length.toString() +' in function findByManifestTypeAndLabel');
                return;
            }

            // Return single Crate.
            return foundCrates[0];
        }

        // Since all configurationControl information is stored in Action.ConfigurationControls field,
        // we should merge it back to action's CrateStorage before sending that data to server.
        public mergeControlListCrate(controlList: model.ControlsList, crateStorage: model.CrateStorage) {
            // Validate that ControlList is not empty.
            if (!controlList || !controlList.fields
                || !crateStorage || !crateStorage.crates) {

                return;
            }

            // Find single crate with manifestType == 'Standard Configuration Controls'.
            var controlListCrate = this.findByManifestType(
                crateStorage, 'Standard Configuration Controls');

            // Overwrite contents of that crate with actual data in controlList.fields.
            controlListCrate.contents = angular.toJson(controlList.fields);
        }

        public createControlListFromCrateStorage(crateStorage: model.CrateStorage): model.ControlsList {
            var crate = this.findByManifestType(
                crateStorage, 'Standard Configuration Controls'
                );

            var controlsList = new model.ControlsList();
            controlsList.fields = angular.fromJson(crate.contents);

            //now we should look for crates with manifestType Standart Design Time Fields
            //to set or override our DropdownListBox items
            //TODO remove this logic to seperate function
            for (var i = 0; i < controlsList.fields.length; i++) {
                if (controlsList.fields[i].type == 'dropdownlistField') {
                    var dropdownListField = <model.DropDownListBoxField> controlsList.fields[i];
                    if (!dropdownListField.source) {
                        continue;
                    }
                    var stdfCrate = this.findByManifestTypeAndLabel(
                        crateStorage, dropdownListField.source.manifestType, dropdownListField.source.label
                        );
                    if (stdfCrate == null) {
                        continue;
                    }

                    var listItems = <Array<model.DropDownListItem>> angular.fromJson(stdfCrate.contents);
                    dropdownListField.listItems = listItems;
                }
            }

            return controlsList;
        }
    }
}

app.service('CrateHelper', dockyard.services.CrateHelper); 