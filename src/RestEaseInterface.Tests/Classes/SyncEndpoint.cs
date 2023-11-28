using RestEaseInterface.Tests.Interfaces;

namespace RestEaseInterface.Tests.Classes; 

public class SyncEndpoint : ISyncEndpoint {
    public string NoTask() {
        throw new NotImplementedException();
    }
}