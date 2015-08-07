namespace Core.Interfaces
{
	public interface IDocuSignNotification
	{
		void Process(string userId, string xmlPayload);
	}
}