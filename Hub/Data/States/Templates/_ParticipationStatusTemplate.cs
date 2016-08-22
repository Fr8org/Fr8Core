using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.States.Templates
{
    public class _ParticipationStatusTemplate : IStateTemplate<ParticipationStatus>
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public String Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
