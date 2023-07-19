namespace Santander.Backend.Core.Configuration
{
    public class RestApiOptions
    {
        public const string Key = "RestApi";

        public int GetRequestDefaultTimeout { get; set; } = 0;        
    }
}
