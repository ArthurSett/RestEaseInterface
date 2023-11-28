using System.Net;

namespace RestEaseInterface.Exceptions; 

public class ParameterConversionException : RestEaseInterfaceException {
    public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;
    public sealed override string Content { get; init; }

    public ParameterConversionException(string content) {
        Content = content;
    }
}