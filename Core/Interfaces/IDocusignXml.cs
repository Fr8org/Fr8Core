namespace Core.Interfaces
{
    public interface IDocusignXml
    {
        string GetEnvelopeIdFromXml(string xmlPayload);
    }
}