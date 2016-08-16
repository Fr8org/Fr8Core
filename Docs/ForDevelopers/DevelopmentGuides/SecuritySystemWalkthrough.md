Security System Walkthrough
===========================

Before continue reading this, make sure you are familiar with security from an [administrative point of view](/Docs/SecurityHome.md). 


##Permissions

Currently the system has a dozen predefined Permission Types that can be used. Every Permission Type describes a specific action that can be done in the Fr8 system.
Permission types are defined in an Enum than can be found in Data.States.PermissionType.
First 7 are generic permission types that can be used for every Fr8Object. Other are custom defined for some specific actions.

These permission types are saved into the database and are persisted via our StateTemplate system that is being run on every application start.

```
public enum PermissionType
{
    CreateObject = 1,
    ReadObject = 2,
    EditObject = 3,
    DeleteObject = 4,
    RunObject = 5,
    ViewAllObjects = 6,
    EditAllObjects = 7,
    ManageFr8Users = 8,
    ManageInternalUsers = 9,
    EditPageDefinitions = 10,
    UseTerminal = 11
}
```
NOTE: when changing this Enum, you also need to change his Front-End representation. Go to the PermissionType.ts file in order to make the change.

##Profile and Default Permissions for Objects in Fr8.
A user can have only one profile, in opposite to the multiple Roles defined for him. Profiles define how a user can interact with object groups from general perspective into the system.
Until we have provided a management page for security settings, current Profiles are managed inside our MigrationConfiguration.cs file. There are 3 default Profile configurations in Fr8 right now.
In future we will have custom profiles that will be configured from the Fr8 Admins.
One Profile contains multiple PermissionSets linked to it for every Domain Object that is connected to the Security System. In the moment our system provides security for these next Fr8 objects  

       ⋅PlanNodeDO

       ⋅ContainerDO

       ⋅TerminalDO

       ⋅Fr8AccountDO

       ⋅PageDefinitionDO


When including additional Fr8 Object in the Security System with enabled permissions, go to the method AddDefaultProfiles() into MigrationConfiguration.cs and include that object with defining the permission sets for all 3 Default Profiles.

Example: Call this code for every defined Profile for the new Fr8 Domain Object
```
fr8AdminProfile.PermissionSets.Add(AddPermissionSet(nameof(PlanNodeDO), true, false, false, fr8AdminProfile.Id, "Fr8 Administrator Permission Set", uow));
```

##Creating Record Based Permissions for a new Fr8Object.
In parallel when the user creates some new Domain Object (ex. a new Container) into Fr8 we need to define related permissions to it, on order to be visible and manageable for a specific user/group of users. These are called Record Based Permissions because the system defines a specific permissions for every record that is being inserted into our database.  

Our UnitOfWork pattern is tracking all the entities that need be saved onto the database. Once those objects has been successfully create, the system also create a related permissions for the new entity with calling the method from our **SecurityServices #SetDefaultRecordBasedSecurityForObject**.
This method can be placed into every Domain Object method AfterCreate. AfterCreate is a hook method being called after the entity is successfully created.

Example: PlanNodeDO.cs
```
public override void AfterCreate()
{
    base.AfterCreate();

    var securityService = ObjectFactory.GetInstance<ISecurityServices>();
    securityService.SetDefaultRecordBasedSecurityForObject(Roles.OwnerOfCurrentObject, Id, nameof(PlanNodeDO));
}
```
In moment of creation of Domain Object we are using the Role OwnerOfCurrentObject. This helps us to allocate who is owner of the Domain Object being created. So he as an user can manage that specific object afterwards.

This method has additional parameter: customPermissionTypes where an user can define an array of Permissions to set for that Domain Object:

Example:
```
 securityServices.SetDefaultRecordBasedSecurityForObject(Roles.OwnerOfCurrentObject, terminal.Id, nameof(TerminalDO), new List<PermissionType>() { PermissionType.UseTerminal });
```
If you don't provide values for the parameter customPermissionTypes then the system will enter default permissionTypes:
```
{ PermissionType.ReadObject, PermissionType.EditObject, PermissionType.CreateObject, PermissionType.DeleteObject, PermissionType.RunObject }
```

###PermissionSet
After the method **SetDefaultRecordBasedSecurityForObject** has been invoked, then comes in play the PermissionSet object.

PermissionSet is a composition of Object Type and an array of Permission Types.

So the system check if our array provided in the SetDefaultRecordBasedSecurityForObject method in connection with the type of the object is already defined in the system.
If not it will create a new PermissionSet for our TerminalDO object with only one UseTerminal permission type and after that will link that PermissionSet with the Role OwnerOfCurrentObject. Every other TerminalDO that will have UseTerminal permission type will use this PermissionSet.

In order to create new PermissionSet go to SecurityObjectsStorage and use method:
```
PermissionSetDO GetOrCreateDefaultSecurityPermissionSet(string dataObjectType, List<PermissionType> customPermissionTypes = null)
```
where customPermissionTypes parameter is described above.

SetDefaultRecordBasedSecurityForObject generates ObjectRolePermissions record into database.


##Authorize Activities on Domain Objects based on Permissions
In general all our authorize activity logic need to be placed on the Service Layer of the application. Security System need to be invoked before every Business Logic has taken place.

There is couple of ways to do this:

1. Using AuthorizeActivityAttribute.
This is the best principle in order to decouple our permissions check code from the Service logic, and prevent mixing business logic code with permissions related logic.

```    
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
public class AuthorizeActivityAttribute : Attribute
{
    public AuthorizeActivityAttribute()
    {
        ParamType = typeof(Guid);
        Permission = PermissionType.ReadObject;
    }

    /// <summary>
    /// Permission name that must be checked for authorization
    /// </summary>
    public PermissionType Permission { get; set; }

    /// <summary>
    /// Type of the argument/parameter where dataObjectId is located.
    /// Default values is Guid, and could be used with BaseObjects that contains key as Id
    /// </summary>
    public Type ParamType { get; set; }

    /// <summary>
    /// Global Object type for whom we are checking granted permissions
    /// </summary>
    public Type TargetType { get; set; }

    /// <summary>
    /// Attribute is set on a property
    /// </summary>
    public bool IsProperty { get; set; }
}
```
Usage: Activity.cs method GetById
```
[AuthorizeActivity(Permission = PermissionType.ReadObject, ParamType = typeof(Guid), TargetType = typeof(PlanNodeDO))]
public ActivityDO GetById(IUnitOfWork uow, Guid id)
```
Here as you can see we are checking the if current user has permission to read the object that is defined with this (Guid id) identifier. Also is mandatory to define the type of the object that we are using in order to know what PermissionSet to take into consideration.

This attribute check is triggered by an Interceptor(check AuthorizeActivityInterceptor) that tracks all method calls into Activity.cs and check if that method call is decorated with AuthorizeActivity Attribute. So before specific implementation of GetById method can be invoked, the interceptor will check if current user is allowed to call GetById activity, and will raise an Http.Forbidden exception in negative outcome.

Interceptor method is controlled from our StructureMap.Boostrapper Registries where we decorate with dynamic proxy generator our Service implementations:
```
var dynamicProxy = new ProxyGenerator();
For<IActivity>().Use<Activity>().Singleton().DecorateWith(z => dynamicProxy.CreateInterfaceProxyWithTarget(z, new AuthorizeActivityInterceptor(ObjectFactory.GetInstance<ISecurityServices>())));
```
Don't forget to check the StructureMap Registry when decorating some Service with AuthorizeActivity Attribute. This approach is helping to control the Security System from one place.


2. There is some scenarios for methods that can't use this AuthorizeActivity Attribute. For example you need to do some internal processing in a method, and after that to check the permissions for that object. Then you can call directly the method AuthorizeActivity from our SecurityServices.cs implementation.

Example:
```
public Task<ActivateActivitiesDTO> Activate(Guid planId, bool planBuilderActivate)
{
   //here goes some processing
   if (_securityServices.AuthorizeActivity(PermissionType.EditObject, planId, nameof(PlanNodeDO)))
   {
        //continue processing of method Activate
   }
   else
   {
       throw new HttpException(403, "You are not authorized to perform this activity!");
   }
}
```
Note: In your AuthorizeActitivy checks the most important part is providing the Id of the Domain Object on which we want to check Permissions.

###Check if User has Allowed Permission for Some Objects Group on Front-End
When there is some cases when we need to check if user has some allowed permissions for some object group on the Front-End side, we have defined an API endpoint about that.
Call **api/users/checkpermission** with parameters for User Id, Permission Type and Object Type

For example when checking if Current user logged in the Fr8, has permissions to "EditAllObjects" from type "TerminalDO", then this api endpoint will return correct answer about.
Note: This permissions use the Profiles defined permissions, to check if some user has the rights to CRUD some objects group.

## Get Allowed Objects for User

In order to get all allowed objects for current user, the system at first checks the user roles, and based on that user roles returns a list of ObjectRolePermissions so it can evaluate if a set of PermissionTypes linked with roles are defined for some object group.

Example:
```
public IEnumerable<TerminalDO> GetAllowedTerminalsByUser(IEnumerable<TerminalDO> terminals) in the SecurityServices.cs
```
This method will iterate every item in the collection, and will return a filtered collection that the user can have access to.

## Caching

All permission related logic is being cached into our system. Once we provide new ObjectRolePermissions for newly created Domain Object the SecurityObjectsCache Service is coming in hand. Every ObjecrRolePermissions are being cached for a configured amount of time.

##Domain Objects Used In Security System

Most important objects used in Security System are the ObjectRolePermissions.
Those are detached from the Entity Framework context because of the usage of AspNetRoles. In order to provide support for caching and easy manipulation of AspNetRoles, we use a POCO implementation RoleDO inside Security System.

Note: For reference check Data.Repositories.Security.Entities.ObjectRolePermissionsWrapper class.
