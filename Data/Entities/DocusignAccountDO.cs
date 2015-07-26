using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Interfaces;
using DocuSign.Integrations.Client;

namespace Data.Entities
{
    public class DocusignAccountDO : BaseDO, IDocusignAccountDO
    {
        private readonly Account _account;

        public DocusignAccountDO()
            : this(new Account())
        {

        }

        public DocusignAccountDO(Account account)
        {
            _account = account;
        }

        [Key]
        public int Id { get; set; }

        /*
        // when we need a DocuSign Account property to be persisted we code the following:
        public Guid AccountIdGuid
        {
            get { return _account.AccountIdGuid; }
            set { _account.AccountIdGuid = value; }
        }
        */
    }
}
