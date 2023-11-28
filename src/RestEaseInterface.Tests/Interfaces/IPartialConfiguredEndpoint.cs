using RestEase;

namespace RestEaseInterface.Tests.Interfaces; 

public interface IPartialConfiguredEndpoint {
    [Get("configured")]
    Task<string> Configured();

    Task<string> NoAttribute();
}