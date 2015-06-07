using System;
using System.ComponentModel.DataAnnotations;
using Data.Entities;

namespace Data.Interfaces
{
    public interface IPerson
    {
        [Key]
        int Id { get; set; }       

        String FirstName { get; set; }
        String LastName { get; set; }

        EmailAddressDO EmailAddress { get; set; }
    }
}
        
