# INTRODUCTION
[Return](/Docs/Home.md)

Fr8 access control uses the Salesforce.com architecture and is based on User Profiles, Roles and Permissions. 

Their primary purpose is to give an easy way to manage access rules for group of users, restrict a set of web pages for certain users, and allow certain action to specific objects.  

Fr8 has support for registering accounts as individual users or users that are part of some [Organization](/Docs/ForDevelopers/Objects/Organizations.md). New registered users are normally assigned the Standard User Profile.  

Users inside Organizations are assigned the Role "MemberOfOrganization[orgname]". 

System administrators inside the application can manage all users accounts and be able to change global permissions and settings for a selected user with changing their assigned Profile. This can be done from the Admin Menu -> Manage Users view.

## USERS & SECURITY

System permissions grant access to different objects which apply to the entire Fr8 environment. Those permissions are grouped in sets which are linked to Profiles. Every user can have only one Profile assigned to him.
Fr8 support two level of security principles for accessing its data:
–  Object Based Permissions
–  Record Based Permissions

### Object-Type Permissions

Object based permissions define global privileges to different Fr8 objects. Example: Edit Plans, View Activities.

These permissions let administrators revoke access for all objects from interaction, or add a possibility for CRUD operations on all group of objects (ex. possibility to edit all plans in the system).

### Record-Based Permissions

Where Object Role Permission operates on a broad object scale,  Fr8 system has support for adding a more specific level of access control for your data through record based permissions.

Record-Based Permissions determines the ability to grant access to individual object instances, or records.  Record based permissions are helpful in the structure of sharing a private object with a group of users.

Record-Based Security has higher priority from Object Based Security. So the security system first checks to see if an object contains some record-based security applicable to the current User. If none are found, the Object Type Permissions associated with the User's Profile are used.  

System Administrators can apply Record Based Permission to whitelist  specific objects for a group of users.  

[More Details](/Docs/ForDevelopers/DevelopmentGuides/SecuritySystemWalkthrough.md)
=====================================
