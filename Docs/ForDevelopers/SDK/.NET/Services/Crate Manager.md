# Crate Manager

The crate manager provides utility methods for processing the data in [Crate](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/Crate.md) objects.

## Features

### DTO <-> DO conversion
One of the main features of the CrateManager is to convert DTO classes to DO classes and vice-versa. The conversion is carried out using the [CrateStorageSerializer](CrateStorageSerializer.md) class.

### CrateStorage
The CrateManager utilizes the [CrateStorage](CrateStorage.md) class to maintain Crate data. More details can be found in the [CrateStorage](CrateStorage.md) section of this documentation.

### Crate creation
The CrateManager provides methods for creating Crate data objects for specific

#### Field Crates
The CrateManager provides methods for creating Crate data objects that contain design time and run-time fields from [FieldDTO](/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/DataTransfer/FieldDTO.md).

#### Event Crates

#### Table Data Crates

#### Payload Data

#### Operational Status

### Merge Content Fields
