Terminal Discovery
==================

Hubs periodically will seek to update their knowledge of your Terminal and its capabilties by calling your /discover endpoint. 

Responding to /discover is very simple. All terminals written to date simply hardcode their Activity and Terminal information in their controller.
Activities are represented by ActivityTemplate objects.

![sample](sample_discovery_data.png)
