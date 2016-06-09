using Fr8.Infrastructure.Data.Crates;
namespace terminalFr8Core.Converters
{
    public interface ICrateConversion
    {
        Crate Convert(Crate input);
    }
}