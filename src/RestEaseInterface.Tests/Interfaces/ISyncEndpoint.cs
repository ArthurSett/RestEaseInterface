using RestEase;

namespace RestEaseInterface.Tests.Interfaces; 

public interface ISyncEndpoint {
    [Get("no-task")]
    string NoTask();
}