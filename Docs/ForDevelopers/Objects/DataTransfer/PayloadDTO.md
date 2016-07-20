#PayloadDTO
The Payload Data Transfer object is the basic data transfer object that contains a [CrateStorageDTO](CrateStorageDTO.md) as well as a container ID.

##Fields
_Guid ContainerId_: The ID of the PayloadDTO. Defined with a getter and setter.
_CrateStorageDTO CrateStorage_: The contents of the PayloadDTO. Stored in JSON with the key "container". Defined with a getter and setter.

##Constructor
**PayloadDTO(Guid containerId)**
The single parameter constructor sets the ContainerId field using the containerId parameter.

