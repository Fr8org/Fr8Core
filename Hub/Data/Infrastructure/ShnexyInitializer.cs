using Data.Constants;
using Data.Interfaces;
using StructureMap;

namespace Data.Infrastructure
{
    public class ShnexyInitializer : System.Data.Entity.DropCreateDatabaseIfModelChanges<KwasantDbContext>
    {
        protected override void Seed(KwasantDbContext context)
        {
            IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>();
            InstructionConstants.ApplySeedData(uow);
            uow.SaveChanges();
        }
    }
}