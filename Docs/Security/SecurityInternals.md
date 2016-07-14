# Security Internals
[Go to Contents](https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md)

## Object Role Permissions

Object Role Permissions are object maintained by the Hub that represent a Permission that all users of a particular Role have in regard to a specified Object. This is how object-level security is implemented. For example, when user Mary creates Object Foo, an Object Role Permission is created giving Edit Object permission on that object to the Current Owner Role.

## Permission Hierarchy

Fr8 checks for the presence of Object Role Permissions. If it finds at least one, then it takes precedence over Profile-level security.  For example: in Data Entry Mode where one plan can be shared through a link and every user needs to be able to run that plan.
