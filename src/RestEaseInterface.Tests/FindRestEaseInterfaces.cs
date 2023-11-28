using System.Reflection;
using RestEaseInterface.Tests.Interfaces;

namespace RestEaseInterface.Tests;

public class FindRestEaseInterfaces {
    private readonly Dictionary<Type, TypeInfo> Interfaces
        = AppDomain.CurrentDomain.GetAssemblies().CollectRestEaseInterfaceServices().ToDictionary(x => x.Item1, y => y.Item2);

    [Fact]
    public void IncludeAllConfiguredInterfaces() {
        Assert.True(Interfaces.ContainsKey(typeof(ITestEndpoint)));
    }

    [Fact]
    public void ExcludePartialConfiguredInterfaces() {
        Assert.False(Interfaces.ContainsKey(typeof(IPartialConfiguredEndpoint)));
    }

    [Fact]
    public void ExcludeNotConfiguredInterfaces() {
        Assert.False(Interfaces.ContainsKey(typeof(INormalInterfaceEndpoint)));
    }

    [Fact]
    public void ExcludeInterfacesWithSyncMethods() {
        Assert.False(Interfaces.ContainsKey(typeof(ISyncEndpoint)));
    }

    [Fact]
    public void ExcludeInterfacesWithNoClass() {
        Assert.False(Interfaces.ContainsKey(typeof(IMissingClassEndpoint)));
    }
}