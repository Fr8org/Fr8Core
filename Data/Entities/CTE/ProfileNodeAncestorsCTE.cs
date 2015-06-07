using System.ComponentModel.DataAnnotations;

namespace Data.Entities.CTE
{
    public class ProfileNodeAncestorsCTE
    {
        // ****** IMPORTANT ********
        // DO NOT MODIFY UNLESS YOU CHANGE THE MIGRATION FILE
        // ****** IMPORTANT ********

        //If you ever change this - be sure not to use the generated migration file.
        //Please see the migration 'ProfileNodeCTEView'
        //In case of changing this class, you'll need to drop the view, and recreate it with your new schema

        [Key]
        public int Id { get; set; } //Fake Id, otherwise EF complains...

        public int ProfileNodeID { get; set; }
        public int? ProfileParentNodeID { get; set; }

        //This is the way we can 
        public int AnchorNodeID { get; set; }
    }
}
