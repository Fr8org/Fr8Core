using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class OrganizationDO : BaseObject
    {
        public OrganizationDO()
        {
            Fr8Accounts = new List<Fr8AccountDO>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }

        [InverseProperty("Organization")]
        public virtual IList<Fr8AccountDO> Fr8Accounts{ get; set; }
    }
}
