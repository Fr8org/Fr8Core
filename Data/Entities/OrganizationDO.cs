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

        public string LogoUrl { get; set; }

        public string BackgroundColor { get; set; }

        public string ThemeName { get; set; }


        [InverseProperty("Organization")]
        public virtual IList<Fr8AccountDO> Fr8Accounts{ get; set; }
    }
}
