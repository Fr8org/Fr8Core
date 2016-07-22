# Security Internals
[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)

## Object Role Permissions

Object Role Permissions are object maintained by the Hub that represent a Permission that all users of a particular Role have in regard to a specified Object. This is how object-level security is implemented. For example, when user Mary creates Object Foo, an Object Role Permission is created giving Edit Object permission on that object to the Current Owner Role.

## Permission Hierarchy

Fr8 checks for the presence of Object Role Permissions. If it finds at least one, then it takes precedence over Profile-level security.  For example: in Data Entry Mode where one plan can be shared through a link and every user needs to be able to run that plan.


## Database Architecture


#### Permissions
These are defined in dbo_PermissionTypeTemplate. As the Hub's functionality evolves, additional Permissions are getting defined. As of this writing, the set includes:
CreateObject

ReadObject

EditObject

DeleteObject

RunObject

ViewAllObjects

ModifyAllObjects

ManageFr8Users

ManageInternalUsers

EditPageDefinitions

#### PermissionSets

Permissions are generally managed in groups, via PermissionSets. The connection between Permissions and PermissionSets is a join table called dbo.PermissionSetPermissions

Each PermissionSet relates an ObjectType (such as a PlanNode) to a set of Permissions (such as ViewObject and CreateObject).

#### Roles

Roles reflect groups that a User belongs to, and simplify the creation of associations between specific users and specific objects.  Currently, they're generated automatically and can't be manually changed. Some key Roles include

| Role  |  Notes|
|---|---|
|  OwnerOfCurrentObject  |   |
| AdminOfOrganizationFoo  | Created when an Organization is created  |
|  MemberOfOrganizationFoo  | Created when an Organization is created  |

The dbo.RolePermissions join table associates Roles with PermissionSets (and thus to specific Permissions)

As an example: when a User creates an object or loads an object that they're the Owner of, they are given the Role OwnerOfCurrentObject. In the RolePermissions join table, this Role is linked to several distinct PermissionSets

Direct record access is controlled by creating tuples that relate an ObjectId to a Role to a Permission. These are stored in a join table called ObjectRolePermissions that associates RolePermissions with ObjectIds

.NET Implementation Notes - 
In the .NET Hub implementation, Roles are implemented using ASP Identity 2.0 Roles. Because ASP Identity Roles are not cacheable, Fr8 wraps them in RoleDO objects.

#### Profiles
A user always belongs to a single Profile. 

