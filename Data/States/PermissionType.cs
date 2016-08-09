namespace Data.States
{
    // When adding or changing items in this enum please also do the same in PermissionType.ts.
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
}
