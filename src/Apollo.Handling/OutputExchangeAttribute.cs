namespace Apollo.Handling;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class OutputExchangeAttribute(string exchangeName) : Attribute
{
    public string ExchangeName { get; } = exchangeName;
}