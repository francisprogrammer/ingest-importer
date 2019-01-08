namespace WebApi.Ingestions
{
    public class ValidationError
    {
        public string Name { get; }
        public string Location { get; }
        public string Message { get; }

        public ValidationError(string name, string location, string message)
        {
            Name = name;
            Location = location;
            Message = message;
        }
    }
}