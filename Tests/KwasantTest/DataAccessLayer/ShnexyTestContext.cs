using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Mail;
using System.Web;
using Shnexy.Models;

namespace ShnexyTest.DataAccessLayer
{


    public class ShnexyTestContext : DbContext
    {

        //special constructor to enable migration to run on the test setup. 
        public  ShnexyTestContext() : base("ShnexyTestContext")
        {
            
        }

        //public ShnexyDbContext(string mode) : base(mode)
        //{
           
        //}

      
        

        public DbSet<Email> Emails { get; set; }

        public DbSet<Customer> Customers { get; set; }

        public DbSet<EmailAddress> EmailAddresses { get; set; }

        //public System.Data.Entity.DbSet<Shnexy.Models.AppointmentTable> AppointmentTables { get; set; }
        
        
      
    }

    //public class DevContext : ShnexyDbContext
    //{
    //    public DevContext() : base("localShnexyDb")
    //    {
            
    //    }

    //}
    //public class TestContext : ShnexyDbContext
    //{
    //    public TestContext()
    //        : base("ShnexyTestDb")
    //    {

    //    }

    

}