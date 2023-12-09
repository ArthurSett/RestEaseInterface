using System.Reflection;
using RestEase;

namespace RestEaseInterface.Services.Method;

public class RestEaseInterfaceMethodInfo
{
    public Type InterfaceType { get; init; }
    public MethodInfo MethodInfo { get; init; }
    
    
    private bool returnTypeSet { get; set; }
    private Type? returnType { get; set; }
    public Type? ReturnType
    {
        get {
            if (returnTypeSet)
                return returnType;
            returnTypeSet = true;
            var arguments = MethodInfo.ReturnType.GetGenericArguments();
            if (arguments.Length == 0) return null;

            return returnType = (arguments.Length != 0 ? arguments[0] : MethodInfo.ReturnType);
        }
    }
    
    private bool? isAsync { get; set; }

    public bool IsAsync {
        get {
            if (isAsync != null)
                return isAsync.Value;
            return (bool)(isAsync = MethodInfo.ReturnType == typeof(Task) || MethodInfo.ReturnType.BaseType == typeof(Task));
        }
    }
    
    private HttpMethod? httpMethod { get; set; }
    public HttpMethod HttpMethod
    {
        get {
            if (httpMethod != null)
                return httpMethod;
            return httpMethod = MethodInfo.GetCustomAttribute<RequestAttributeBase>()!.Method;
        }
    }

    private string? endpointPath { get; set; }
    public string EndpointPath {
        get {
            if (endpointPath != null)
                return endpointPath;
            return endpointPath = string.IsNullOrWhiteSpace(BasePath)
                ? (string.IsNullOrWhiteSpace(MethodPath)
                    ? "/"
                    : "/" + MethodPath)
                : (string.IsNullOrWhiteSpace(MethodPath)
                    ? BasePath
                    : Path.Combine(BasePath, MethodPath));
        }
    }
    
    private string? basePath { get; set; }
    public string BasePath {
        get {
            if (basePath != null)
                return basePath;
            return basePath = InterfaceType.CustomAttributes.Any(x => x.AttributeType == typeof(BasePathAttribute))
                ? InterfaceType.GetCustomAttribute<BasePathAttribute>()!.BasePath
                : String.Empty;
        }
    }
    
    private string? methodPath { get; set; }
    public string MethodPath {
        get {
            if (methodPath != null)
                return methodPath;
            string path = MethodInfo.GetCustomAttribute<RequestAttributeBase>()!.Path;
            return methodPath = path?.TrimStart('/') ?? string.Empty;
        }
    }
    
    private ParameterInfo[]? parameterInfos { get; set; }
    
    public ParameterInfo[] ParameterInfos {
        get {
            if (parameterInfos != null)
                return parameterInfos;
            return parameterInfos = MethodInfo.GetParameters();
        }
    }
    private RestEaseInterfaceParameterInfo[]? parameters { get; set; }
    public RestEaseInterfaceParameterInfo[] Parameters {
        get {
            if (parameters != null)
                return parameters;
            return parameters = ParameterInfos.Select(x => {
                var attributes = x.GetCustomAttributes(typeof(Attribute), true) as Attribute[] ?? new Attribute[] { };
                var determinedParameterLocation = DetermineParameterLocation(attributes);
                return new RestEaseInterfaceParameterInfo {
                    Name = x.Name!,
                    Type = x.ParameterType,
                    Attributes = attributes,
                    Location = determinedParameterLocation.Item1,
                    AttributeDefinedName = determinedParameterLocation.Item2
                };

                (RestEaseInterfaceParameterLocation, string) DetermineParameterLocation(
                    IEnumerable<Attribute> parameterAttributes) {
                    foreach (var attribute in parameterAttributes) {
                        switch (attribute) {
                            case QueryAttribute query:
                                return (RestEaseInterfaceParameterLocation.Query, (query.HasName ? query.Name : null))!;
                            case PathAttribute path:
                                return (RestEaseInterfaceParameterLocation.Path,
                                    (!string.IsNullOrWhiteSpace(path.Name) ? path.Name : null))!;
                            case HeaderAttribute header:
                                return (RestEaseInterfaceParameterLocation.Header, header.Name);
                            case BodyAttribute _:
                                return (RestEaseInterfaceParameterLocation.Body, null)!;
                        }
                    }

                    return (RestEaseInterfaceParameterLocation.Query, null)!;
                }
            }).ToArray();
        }
    }
}

public class RestEaseInterfaceParameterInfo
{
    public string Name { get; init; }
    public string? AttributeDefinedName { get; init; }
    public RestEaseInterfaceParameterLocation Location { get; init; }
    public Type Type { get; init; }
    public Attribute[] Attributes { get; init; }
}

public enum RestEaseInterfaceParameterLocation
{
    Path,
    Query,
    Header,
    Body
}