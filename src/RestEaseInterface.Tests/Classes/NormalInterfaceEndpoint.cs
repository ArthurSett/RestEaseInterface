using RestEaseInterface.Tests.Interfaces;

namespace RestEaseInterface.Tests.Classes; 

public class NormalInterfaceEndpoint : INormalInterfaceEndpoint {
    public string NormalMethod() {
        throw new NotImplementedException();
    }
}