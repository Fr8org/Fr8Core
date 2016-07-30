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



Permission Sets
==================
Like a profile, a permission set is a collection of settings and permissions that determine what a user can do. Permission sets include some of the same permissions and settings you’ll find in profiles.

Permission Sets make it easier to work with sets of permissions. They are a representation of specific group of permissions, grouped by the type of the object that those permissions need to authorize.   
Permission Sets cannot currently be directly manipulated. They are used internally to form Profiles.

So why profiles and permission sets?

The most significant difference between the two is that users can have only one profile, but they can have many permission sets. This means you can use profiles to grant the minimum permissions and settings that every type of user needs, then use permission sets to grant additional permissions, without changing anyone’s profiles.

There are a couple of ways to use permission sets to your advantage.

To grant access to custom objects or entire apps.
Let’s say you have many users in your organization with the same fundamental job functions. You can assign them all one profile that grants them all the access they need to do their job. But suppose a few of those users are working on a special project and they need access to an app that no one else uses. And suppose a few other users need access to that app, as well as another app that the first group doesn’t need. If we only had profiles, you’d have to create more profiles that were customized to those few users’ needs, or take your chances and add more access to the original profile, making the apps available to users that don’t need it. Neither of these options is ideal, especially if your organization is growing and your users’ needs change regularly. Permission sets make it easy to grant access to the various apps and custom objects in your organization, and to take away access when it’s no longer needed.

