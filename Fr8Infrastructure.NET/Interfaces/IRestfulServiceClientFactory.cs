namespace Fr8.Infrastructure.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IRestfulServiceClientFactory
    {
        /// <summary>
        /// Creates a restfulservice client with given signature
        /// </summary>
        /// <param name="signature"></param>
        /// <returns></returns>
        IRestfulServiceClient Create(IRequestSignature signature);
    }
}
