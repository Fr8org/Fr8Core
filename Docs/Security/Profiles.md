# Profiles
[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)

Profiles define how users access objects and data, and what they can do within the application. When users are created, a profile is assigned for each one of them.

Using Profiles we can assign general CRUD permissions to objects and determine the ability for one user to create, read, edit and delete all records for a specific object.

Profiles for users can be accessed through Admin -> Manage Users -> Edit an Account view and Profile change a for selected user can be performed (when current user changing the Profile, has the permission to do that action).
Right now Fr8 supports 3 standard Profiles that contain this given permission for them:

## 1. STANDARD USER

Basic permissions for CRUD interaction inside Fr8 system.

    |	Create Object | Read Object	| Edit Object |	Delete Object | Run Object | View All Objects | Modify All Objects    
--- | --- | --- | --- | --- | ---  | ---  | ---     
Plan | yes | yes | yes | yes | yes  | no  | no     
Activity | yes | yes | yes | yes | yes  | no  | no     
Container | yes | yes | yes | yes | yes  | no  | no     
User | yes | yes | yes | yes | yes  | no  | no
Terminal | yes | yes | yes | yes | yes  | no  | no

## 2. SYSTEM ADMINISTRATOR

This Profile is assigned to users that are Organization Administrators (users that register and created an Organization). This Profile has also an extra permission “Manage Internal Users” that grants permission to manage users inside the organization.

    |	Create Object | Read Object	| Edit Object |	Delete Object | Run Object | View All Objects | Modify All Objects    
--- | --- | --- | --- | --- | ---  | ---  | ---     
Plan | yes | yes | yes | yes | yes  | yes  | yes     
Activity | yes | yes | yes | yes | yes  | yes  | yes     
Container | yes | yes | yes | yes | yes  | yes  | yes     
User | yes | yes | yes | yes | yes  | yes  | yes
Terminal | yes | yes | yes | yes | yes  | yes  | yes


## 3. FR8 ADMINISTRATOR

This Profile is the same as the System Administrator Profile, only it contains Manage F8 Users permission, that gives ability for all users with this Profile, to manage all user accounts in the system.

    |	Create Object | Read Object	| Edit Object |	Delete Object | Run Object | View All Objects | Modify All Objects    
--- | --- | --- | --- | --- | ---  | ---  | ---     
Plan | yes | yes | yes | yes | yes  | yes  | yes     
Activity | yes | yes | yes | yes | yes  | yes  | yes     
Container | yes | yes | yes | yes | yes  | yes  | yes     
User | yes | yes | yes | yes | yes  | yes  | yes
Terminal | yes | yes | yes | yes | yes  | yes  | yes
