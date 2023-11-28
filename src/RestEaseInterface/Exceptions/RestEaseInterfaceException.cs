using System.Net;
using Newtonsoft.Json;

namespace RestEaseInterface.Exceptions; 

public abstract class RestEaseInterfaceException : Exception {
    public abstract HttpStatusCode StatusCode { get; }
    public abstract string Content { get; init; }

    public sealed override string ToString() {
        return JsonConvert.SerializeObject(this);
    }
}