using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Fr8.Infrastructure.Data.States;

namespace Data.States.Templates
{
    public class _AuthenticationTypeTemplate : IStateTemplate<AuthenticationType>
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
