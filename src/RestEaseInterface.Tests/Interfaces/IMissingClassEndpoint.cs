using RestEase;

namespace RestEaseInterface.Tests.Interfaces; 

public interface IMissingClassEndpoint {
    [Get("missing-class")]
    Task MissingClass();
}