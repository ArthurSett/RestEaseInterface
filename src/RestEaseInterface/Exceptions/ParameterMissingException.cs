using System.Net;

namespace RestEaseInterface.Exceptions; 

public class ParameterMissingException : RestEaseInterfaceException {
    public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;
    public sealed override string Content { get; init; }

    public ParameterMissingException(string content) {
        Content = content;
    }
}