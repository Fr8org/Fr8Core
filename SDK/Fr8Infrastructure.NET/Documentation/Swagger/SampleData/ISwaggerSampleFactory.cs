namespace Fr8.Infrastructure.Documentation.Swagger
{
    public interface ISwaggerSampleFactory
    {
        object GetSampleData();
    }

    public interface ISwaggerSampleFactory<out TObject> : ISwaggerSampleFactory where TObject : class
    {
        /// <summary>
        /// Returns object that will be used as a sample data for the specified type
        /// </summary>
        new TObject GetSampleData();
    }
}
