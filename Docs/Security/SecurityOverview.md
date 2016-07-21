# INTRODUCTION
[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)

Main Fr8 Security system is based on User Roles and Permissions. Their primary purpose is to give an easy way to manage access rules for group of users, restrict a set of web pages for certain users, and allow certain action to specific objects. Our Role management gives a possibility to treat groups of users as a unit by assigning users to roles such as administrator, standard user, admin of organization, member of organization, and so on.   

Fr8 has support for registering accounts as individual users or users that are part of some [Organization](https://github.com/Fr8org/Fr8Core/blob/master/Docs/ForDevelopers/Objects/Organizations.md). Every new registered user into Fr8 receives the role of customer or a standard user. Also users inside Organizations receive additional role as Member of that Organization, role that is used to identify all users as a group in the Fr8 system, which gives an opportunity for a system administrator for easier managing user accounts.

Access to different business domain aspects and resources is provided via Permissions and Profiles.

System administrators inside the application can manage all users accounts and be able to change global permissions and settings for a selected user with changing their assigned Profile. This can be done from the Admin Menu -> Manage Users view.

## USERS & SECURITY

System permissions grant access to different objects which apply to  entire Fr8 environment. Those permissions are grouped in sets which are linked to Profiles. Every user can have only one Profile assigned to him.
Fr8 support two level of security principles for accessing its data:
–  Object Based Permissions
–  Record Based Permissions

### Object Based Permissions

Object based permissions define global privileges to different Fr8 objects, like grant access on users to seeing, creating, editing or deleting any instance of a particular type of object, such as plan or activity.

These permissions let you revoke access for all objects from interaction, or add a possibility for CRUD operations on all group of objects (ex. possibility to edit all plans in the system).

### Record Based Permissions

Where Object Role Permission operates on a broad object scale,  Fr8 system has support for adding a more specific level of access control for your data through record based permissions.

Record Based Permissions determines the ability to grant access to individual records within a particular object. One system administrator can set the access level to a record to be visible for all, with sharing this record with other users. Record based permissions are helpful in the structure of sharing a private object with a group of users. For example for plans that are created in the Data Entry Mode we can easy share them.

Record Based Security has higher priority from Object Based Security. So in security checks, at first we need to see if object contains some record based security defined for him. When Record based security permissions are found for an object, security is invoked based on them. Else there comes in action default object based security defined in the Profiles.

System Administrators can see Record Based Permission as implementation of white listing for specific objects for a group of users. For example if we want to grant access on terminals in a organization, we need to create a record based permission with a privilege for modifying that terminal.
