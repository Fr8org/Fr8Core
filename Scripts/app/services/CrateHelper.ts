module dockyard.services {

    export class CrateHelper {
        private filterByTag: (list: model.DropDownListItem[], filterByTag: string) => model.DropDownListItem[]
        private $filter: ng.IFilterService;

        constructor($filter) {
            this.$filter = $filter;
            this.filterByTag = $filter('FilterByTag');
        }

        public throwError(errorText: string) {
            throw new Error(errorText);
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
            return this.hasCrateOfManifestType(crateStorage, 'Standard UI Controls');
        }

        public findByLabel(crateStorage: model.CrateStorage, label: string): model.Crate {
            // Check that CrateStorage is not empty.
            if (!crateStorage || !crateStorage.crates) {
                this.throwError('CrateStorage is empty.');
                return;
            }

            // First we put all found crates to "foundCrates" array.
            // We want to return only a single Crate, so we validate foundCrates.length later.
            var foundCrates: Array<model.Crate> = [];

            for (var i = 0; i < crateStorage.crates.length; ++i) {
                if (crateStorage.crates[i].label == label) {
                    foundCrates.push(crateStorage.crates[i]);
                    break;
                }
            }

            // Validate foundCrates.length that only single Crate was found.
            if (foundCrates.length == 0) {
                this.throwError('Invalid foundCrates.length = ' + foundCrates.length.toString());
                return;
            }

            // Return single Crate.
            return foundCrates[0];
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
            if (foundCrates.length == 0 ) {
                this.throwError('Invalid foundCrates.length = ' + foundCrates.length.toString());
                return;
            }

            // Return single Crate.
            return foundCrates[0];
        }

        // Find single Crate by ManifestType and Label in CrateStorage.
        public findByManifestTypeAndLabel(crateStorage: model.CrateStorage, manifestType: string, label: string): model.Crate {
            // Check that CrateStorage is not empty.
            if (!crateStorage || !crateStorage.crates || crateStorage.crates.length === 0) {
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

            // remove AuthUnsuccessfulLabel from fields before sending data to server
            var fieldsToSyncWithCrate = controlList.fields.slice();
            for (var i = 0; i < fieldsToSyncWithCrate.length; ++i) {
                if (fieldsToSyncWithCrate[i].name === 'AuthUnsuccessfulLabel') {
                    fieldsToSyncWithCrate.splice(i, 1);
                    break;
                }
            }

            // Find single crate with manifestType == 'Standard UI Controls'.
            var controlListCrate = this.findByManifestType(
                crateStorage, 'Standard UI Controls');

            // Overwrite contents of that crate with actual data in controlList.fields.
            controlListCrate.contents = { Controls: fieldsToSyncWithCrate };
        }

        private populateListItemsFromDataSource(fields: Array<model.ControlDefinitionDTO>, crateStorage: model.CrateStorage) {
            //now we should look for crates with manifestType Standard Design Time Fields
            //to set or override our DropdownListBox items
            for (var i = 0; i < fields.length; i++) {
                if (fields[i].type == 'DropDownList' || fields[i].type == 'TextSource') {
                    var dropdownListField = <model.DropDownList> fields[i];
                    if (!dropdownListField.source) {
                        continue;
                    }

                    var stdfCrate = this.findByManifestTypeAndLabel(
                        crateStorage, dropdownListField.source.manifestType, dropdownListField.source.label
                        );
                    if (stdfCrate == null) {
                        continue;
                    }

                    var listItems = <any> stdfCrate.contents;
                    dropdownListField.listItems = this.filterByTag(listItems.Fields, dropdownListField.source.filterByTag);
                }

                // Handle nested fields
                let field: any = fields[i];
                if (field.controls) {
                    this.populateListItemsFromDataSource((<model.ISupportsNestedFields>field).controls, crateStorage);
                }
                // If we encountered radiobuttonGroup, we need to check every individual option if it has any nested fields
                if (field.radios) {
                    this.populateListItemsFromDataSource((<model.RadioButtonGroup>field).radios, crateStorage);
                }
            }
        }

        private resetClickedFlag(fields: Array<any>) {
            for (var field of fields) {
                if (field.clicked) 
                    field.clicked = false;
            }
        }

        public createControlListFromCrateStorage(crateStorage: model.CrateStorage): model.ControlsList {
            var crate = this.findByManifestType(
                crateStorage, 'Standard UI Controls'
                );

            var controlsList = new model.ControlsList();
            controlsList.fields = (<any>crate.contents).Controls;
            this.resetClickedFlag(controlsList.fields); // Unset 'clicked' flag on buttons and other coontrols on which it exists
            this.populateListItemsFromDataSource(controlsList.fields, crateStorage);
            return controlsList;
        }

        public getAvailableFieldTypes(crateStorage: model.CrateStorage): string[]{
            try {
                var fieldsCrate = this.findByLabel(crateStorage, 'Upstream Terminal-Provided Fields');
            } catch (e) {
                return [];
            }

            var fields = <string[]>(<any>fieldsCrate.contents).Fields;
            var result = [];
            <string[]>(<any>fieldsCrate.contents).Fields.forEach((field) => {
                if (field.tags && result.indexOf(field.tags) === -1) result.push(field.tags);
            });

            return result;
        }

        //get all validation messages that are inside the Error Message Response Crate
        public getValidationErrorMessagesFromCrate(crateStorage: model.CrateStorage, label: string): string {
            "use strict"; // Check that CrateStorage is not empty.
            if (!crateStorage || !crateStorage.crates || crateStorage.crates.length === 0) {
                this.throwError('CrateStorage is empty.');
                return '';
            }

            var result = '';
            var errorCrate = this.findByManifestType(crateStorage, label);
            if (errorCrate != null) {
                let content: any = errorCrate.contents;

                if (Object.prototype.toString.call(content) === '[object Array]') {
                    content.forEach((field) => {
                        //result = 'Problem with action: ' + field.CurrentAction + ' <br/>';
                        result += field.Message + ' <br/>';
                    });
                } else {
                    result += content.Message;
                }
        
                //remove from the crate storage the error overview crate
                for (var i = crateStorage.crates .length - 1; i >= 0; i--) {
                    if (crateStorage.crates[i].id === errorCrate.id) crateStorage.crates.splice(i, 1);
                }
            }

            return result;
        }
    }
}

app.service('CrateHelper', ['$filter', dockyard.services.CrateHelper]); 
