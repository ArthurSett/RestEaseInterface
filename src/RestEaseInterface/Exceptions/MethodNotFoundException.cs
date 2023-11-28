using System.Net;

namespace RestEaseInterface.Exceptions; 

public class MethodNotFoundException : RestEaseInterfaceException {
    public override HttpStatusCode StatusCode => HttpStatusCode.NotFound;
    public sealed override string Content { get; init; }

    public MethodNotFoundException(string content) {
        Content = content;
    }
}