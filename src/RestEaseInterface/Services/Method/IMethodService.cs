namespace RestEaseInterface.Services.Method;

public interface IMethodService {
    Task Add(Type interfaceType);

    Task Execute(HttpMethod method, string requestPath, Dictionary<string, object?> parameter,
        object instance);
    Task Execute(RestEaseInterfaceMethodInfo restEaseInterfaceMethodInfo, Dictionary<string, object?> parameter,
        object instance);

    Task<object> ExecuteToObject(HttpMethod method, string requestPath,
        Dictionary<string, object?> parameter, object instance);
    
    Task<string> ExecuteToJson(HttpMethod method, string requestPath,
        Dictionary<string, object?> parameter, object instance);

    Task<string> ExecuteToJson(RestEaseInterfaceMethodInfo restEaseInterfaceMethodInfo, Dictionary<string, object?> parameter,
        object instance);

    Task<Stream> ExecuteToStream(HttpMethod method, string requestPath, Dictionary<string, object?> parameter,
        object instance);
    
    Task<Stream> ExecuteToStream(RestEaseInterfaceMethodInfo restEaseInterfaceMethodInfo, Dictionary<string, object?> parameter,
        object instance);
    Task<RestEaseInterfaceMethodInfo[]> GetMethod();
}