using RestEaseInterface.Tests.Interfaces;

namespace RestEaseInterface.Tests.Classes; 

public class PartialConfiguredEndpoint : IPartialConfiguredEndpoint {
    public Task<string> Configured() {
        throw new NotImplementedException();
    }

    public Task<string> NoAttribute() {
        throw new NotImplementedException();
    }
}