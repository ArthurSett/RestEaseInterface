using Newtonsoft.Json;
using RestEaseInterface.Exceptions;
using RestEaseInterface.Services.Method;
using RestEaseInterface.Tests.Classes;
using RestEaseInterface.Tests.Entities;
using RestEaseInterface.Tests.Interfaces;

namespace RestEaseInterface.Tests; 

public class MethodServiceTests {
    [Fact]
    public async Task AddInterfaceWithMethodsAndGetMethods() {
        IMethodService methodService = new MethodService();

        await methodService.Add(typeof(ITestEndpoint));
        Assert.NotEmpty(await methodService.GetMethod());
    }

    [Fact]
    public async Task CantInitializeInterfaceTwice() {
        IMethodService methodService = new MethodService();

        await methodService.Add(typeof(ITestEndpoint));
        await Assert.ThrowsAsync<Exception>(async () => {
            await methodService.Add(typeof(ITestEndpoint));
        });
    }

    [Fact]
    public async Task CantAddInterfaceWithMethodWithWrongReturnType() {
        IMethodService methodService = new MethodService();
        
        await Assert.ThrowsAsync<Exception>(async () => {
            await methodService.Add(typeof(ISyncEndpoint));
        });
    }

    [Fact]
    public async Task CantAddNonInterfaceType() {
        IMethodService methodService = new MethodService();
        
        await Assert.ThrowsAsync<Exception>(async () => {
            await methodService.Add(typeof(TestEndpoint));
        });
    }

    [Fact]
    public async Task CantAddInterfaceWithMethodWithNoRequestAttribute() {
        IMethodService methodService = new MethodService();
        
        await Assert.ThrowsAsync<Exception>(async () => {
            await methodService.Add(typeof(INormalInterfaceEndpoint));
        });
    }

    [Fact]
    public async Task AddInterfaceWithMethodsAndExecuteMethod() {
        IMethodService methodService = new MethodService();
        ITestEndpoint testEndpoint = new TestEndpoint();
        
        await methodService.Add(typeof(ITestEndpoint));

        await methodService.Execute(HttpMethod.Get, "/test", new Dictionary<string, object?>(), testEndpoint);
    }
    
    [Fact]
    public async Task AddInterfaceWithMethodsAndCantFindExecutingMethodCauseOfPath() {
        IMethodService methodService = new MethodService();
        ITestEndpoint testEndpoint = new TestEndpoint();
        
        await methodService.Add(typeof(ITestEndpoint));

        await Assert.ThrowsAsync<MethodNotFoundException>(async () => {
            await methodService.Execute(HttpMethod.Put, "/not-existing", new Dictionary<string, object?>(), testEndpoint);
        });
        
    }
    
    [Fact]
    public async Task AddInterfaceWithMethodsAndCantFindExecutingMethodCauseOfMissingParameters() {
        IMethodService methodService = new MethodService();
        ITestEndpoint testEndpoint = new TestEndpoint();
        
        await methodService.Add(typeof(ITestEndpoint));

        await Assert.ThrowsAsync<NotImplementedException>(async () => {
            await methodService.Execute(HttpMethod.Post, "/echo", new Dictionary<string, object?>(), testEndpoint);
        });
        
    }
    
    [Fact]
    public async Task AddInterfaceWithMethodsAndExecuteMethodWithParameterAndReturnValue() {
        var parameterValue = "myEcho";
        IMethodService methodService = new MethodService();
        ITestEndpoint testEndpoint = new TestEndpoint();
        
        await methodService.Add(typeof(ITestEndpoint));

        var result = 
            await methodService.ExecuteToObject(HttpMethod.Post, "/echo", 
                new Dictionary<string, object?>(){{"echoMessage", parameterValue}}, testEndpoint);
        
        Assert.Equal(parameterValue, result);
    }
    
    [Fact]
    public async Task AddInterfaceWithMethodsAndExecuteMethodWithMultipleParameterOnWrongOrderAndReturnValue() {
        var numberOne = 3;
        var numberTwo = 8;
        IMethodService methodService = new MethodService();
        ITestEndpoint testEndpoint = new TestEndpoint();
        
        await methodService.Add(typeof(ITestEndpoint));

        var result = 
            await methodService.ExecuteToObject(HttpMethod.Post, "/division", 
                new Dictionary<string, object?>(){{"numberTwo", numberTwo},{"numberOne", numberOne}}, testEndpoint);
        
        Assert.Equal(numberOne / (double)numberTwo, result);
    }
    
    [Fact]
    public async Task AddInterfaceWithMethodsAndExecuteMethodWithParameterAndReturnEntityValue() {
        var parameterName = "myName";
        var parameterDescription = "myDescription";
        var assertResult = JsonConvert.SerializeObject(new TestEntity {
            Name = parameterName,
            Description = parameterDescription
        });
        IMethodService methodService = new MethodService();
        ITestEndpoint testEndpoint = new TestEndpoint();
        
        await methodService.Add(typeof(ITestEndpoint));

        var result = 
            await methodService.ExecuteToJson(HttpMethod.Get, "/entity", 
                new Dictionary<string, object?>(){{"name", parameterName}, {"description", parameterDescription}}, testEndpoint);
        
        Assert.Equal(assertResult, result);
    }
    
    [Fact]
    public async Task AddInterfaceWithMethodsAndExecuteMethodAndReturnStream() {
        var streamMessage = "hello from the other side";
        IMethodService methodService = new MethodService();
        ITestEndpoint testEndpoint = new TestEndpoint();
        
        await methodService.Add(typeof(ITestEndpoint));

        var result = 
            await methodService.ExecuteToStream(HttpMethod.Get, "/stream", 
                new Dictionary<string, object?>(){{"streamMessage", streamMessage}}, testEndpoint);

        StreamReader reader = new StreamReader(result);
        Assert.Equal(streamMessage, await reader.ReadToEndAsync());
    }
}