using Newtonsoft.Json;
using RestEase;
using RestEaseInterface.Exceptions;

namespace RestEaseInterface.Services.Method;

public class MethodService : IMethodService {
    private Dictionary<(string, string),RestEaseInterfaceMethodInfo> _methodInfos = new();

    private RestEaseInterfaceMethodInfo GetMethod(HttpMethod method, string requestPath, Dictionary<string, object?> parameter) {
        if (!_methodInfos.ContainsKey((method.Method, requestPath)))
            throw new MethodNotFoundException("Method not found!");

        var endpoint = _methodInfos[(method.Method, requestPath)];
        if (!endpoint.Parameters.Select(y => y.Name).All(y => parameter.Keys.Any(z => y == z)))
            throw new NotImplementedException("No endpoint found!");
        
        return endpoint;
    }
    
    public async Task Add(Type interfaceType) {
        if (!interfaceType.IsInterface)
            throw new Exception($"Type '{interfaceType.Name}' is not an interface!");
        if (_methodInfos.Any(x => x.Value.InterfaceType == interfaceType))
            throw new Exception($"Interface {interfaceType.Name} cant be initialized twice!");
        foreach (var method in interfaceType.GetMethods().Where(x => x.IsPublic)) {
            if (method.ReturnType != typeof(Task) && method.ReturnType.BaseType != typeof(Task))
                throw new Exception($"Method '{interfaceType.FullName} -> {method.Name}' needs to return Task / Task<> !");
            if (!method.GetCustomAttributes(typeof(RequestAttributeBase), true).Any())
                throw new Exception($"Method '{interfaceType.FullName} -> {method.Name}' got no RequestAttribute assigned!");
            
            var methodInfo = new RestEaseInterfaceMethodInfo {
                InterfaceType = interfaceType,
                MethodInfo = method
            };
            _methodInfos.Add((methodInfo.HttpMethod.Method, methodInfo.EndpointPath), methodInfo);
        }
    }

    public async Task Execute(HttpMethod method, string requestPath, Dictionary<string, object?> parameter,
        object instance) {
        var endpoint = GetMethod(method, requestPath, parameter);

        await (Task)endpoint.MethodInfo.Invoke(instance, endpoint.Parameters
            .Select(x => x.Name).Join(parameter,
                order => order,
                kvp => kvp.Key,
                (order, kvp) => kvp)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value).Values.ToArray())!;
    }

    public async Task Execute(RestEaseInterfaceMethodInfo restEaseInterfaceMethodInfo, Dictionary<string, object?> parameter,
        object instance) {
        await Execute(restEaseInterfaceMethodInfo.HttpMethod, restEaseInterfaceMethodInfo.EndpointPath, parameter, instance);
    }

    public async Task<object> ExecuteToObject(HttpMethod method, string requestPath,
        Dictionary<string, object?> parameter, object instance) {
        var endpoint = GetMethod(method, requestPath, parameter);
        
        Task executingTask = (Task)endpoint.MethodInfo.Invoke(instance, endpoint.Parameters
            .Select(x => x.Name).Join(parameter,
                order => order,
                kvp => kvp.Key,
                (order, kvp) => kvp)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value).Values.ToArray())!;
        object result = executingTask.GetType().GetProperty("Result")!.GetValue(executingTask)!;
        return result;
    }

    public async Task<string> ExecuteToJson(HttpMethod method, string requestPath,
        Dictionary<string, object?> parameter, object instance) {
        var endpoint = GetMethod(method, requestPath, parameter);

        Task executingTask = (Task)endpoint.MethodInfo.Invoke(instance, endpoint.Parameters
            .Select(x => x.Name).Join(parameter,
                order => order,
                kvp => kvp.Key,
                (order, kvp) => kvp)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value).Values.ToArray())!;
        var objectType = executingTask.GetType().GetProperty("Result")!;
        object? objectResult = objectType.GetValue(executingTask)!;
        
        if (objectResult.GetType().IsPrimitive || objectResult is String)
            return objectResult.ToString() ?? String.Empty;

        return JsonConvert.SerializeObject(objectResult);
    }

    public async Task<string> ExecuteToJson(RestEaseInterfaceMethodInfo restEaseInterfaceMethodInfo, Dictionary<string, object?> parameter,
        object instance) {
        return await ExecuteToJson(restEaseInterfaceMethodInfo.HttpMethod, restEaseInterfaceMethodInfo.EndpointPath, parameter, instance);
    }

    public async Task<Stream> ExecuteToStream(HttpMethod method, string requestPath, Dictionary<string, object?> parameter, object instance) {
        var endpoint = GetMethod(method, requestPath, parameter);

        Task executingTask = (Task)endpoint.MethodInfo.Invoke(instance, endpoint.Parameters
            .Select(x => x.Name).Join(parameter,
                order => order,
                kvp => kvp.Key,
                (order, kvp) => kvp)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value).Values.ToArray())!;
        return (Stream)executingTask.GetType().GetProperty("Result")!.GetValue(executingTask)!;
    }

    public async Task<Stream> ExecuteToStream(RestEaseInterfaceMethodInfo restEaseInterfaceMethodInfo, Dictionary<string, object?> parameter,
        object instance) {
        return await ExecuteToStream(restEaseInterfaceMethodInfo.HttpMethod, restEaseInterfaceMethodInfo.EndpointPath,
            parameter, instance);
    }

    public async Task<RestEaseInterfaceMethodInfo[]> GetMethod() {
        return _methodInfos.Select(x => x.Value).ToArray();
    }
}