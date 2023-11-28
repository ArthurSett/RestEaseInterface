using Newtonsoft.Json;
using RestEaseInterface.Exceptions;
using RestEaseInterface.Services.Context;
using RestEaseInterface.Services.Method;

namespace RestEaseInterface;

public static class ServiceUtils {
    private static readonly List<Type> EndpointInterfaces = new ();
    public static void AddRestEaseInterface(this IServiceCollection services) {
        services.AddSingleton<IMethodService, MethodService>();
        services.AddScoped<IContextService, ContextService>();
        var interfacesWithAttribute = AppDomain.CurrentDomain.GetAssemblies()
            .CollectRestEaseInterfaceServices();

        foreach (var interfaceType in interfacesWithAttribute)
        {
            services.AddScoped(interfaceType.Item1, interfaceType.Item2);
            EndpointInterfaces.Add(interfaceType.Item1);
        }
    }

    public static void UseRestEaseInterface(this WebApplication app) {
        var instance = app.Services.GetService<IMethodService>();
        foreach (var endpointInterface in EndpointInterfaces) {
            instance!.Add(endpointInterface).Wait();
        }
        
        var logger = app.Services.GetService<ILogger<RestEaseInterfaceMethodInfo>>()!;
        var methods = instance!.GetMethod().Result;
        if (methods.Length == 0)
            logger!.LogWarning("No Endpoints were initiated!");

        foreach (var method in methods) {
            switch (method.HttpMethod.Method.ToUpper()) {
                case "GET":
                    app.MapGet(method.EndpointPath, (context) =>
                        context.Execute(app, method));
                    break;
                case "PUT":
                    app.MapPut(method.EndpointPath, (context) =>
                        context.Execute(app, method));
                    break;
                case "DELETE":
                    app.MapDelete(method.EndpointPath, (context) =>
                        context.Execute(app, method));
                    break;
                case "POST":
                    app.MapPost(method.EndpointPath, (context) =>
                        context.Execute(app, method));
                    break;
                case "PATCH":
                    app.MapPatch(method.EndpointPath, (context) =>
                        context.Execute(app, method));
                    break;
                default:
                    throw new NotImplementedException(
                        $"Cant find any attribute in '{method.BasePath}/{method.MethodPath}'");
            }
        }

        app.MapControllers();
    }

    private static async Task Execute(this HttpContext context, WebApplication app, RestEaseInterfaceMethodInfo restEaseInterfaceMethod) {
        #region Parameters

        var parsedParameters = await restEaseInterfaceMethod.ParseParameter(context);
        if (parsedParameters == null) {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync(
                $"Cant find Parameters '{string.Join(",", restEaseInterfaceMethod.Parameters.Select(x => x.Name).ToArray())}'!");
            return;
        }
        
        #endregion

        var contextService = context.RequestServices.GetService<IContextService>();
        var endpointInstance = context.RequestServices.GetRequiredService(restEaseInterfaceMethod.InterfaceType);
        contextService!.Set(context);
        var methodService = app.Services.GetService<IMethodService>();
        if (restEaseInterfaceMethod.ReturnType == null) {
            await methodService!.Execute(restEaseInterfaceMethod, parsedParameters, endpointInstance);
            context.Response.StatusCode = 200;
            return;
        }

        if (restEaseInterfaceMethod.ReturnType == typeof(Stream)) {
            var result = await methodService!.ExecuteToStream(restEaseInterfaceMethod, parsedParameters, endpointInstance);

            await context.Response.StartAsync();

            while (context.Response.Body.CanWrite && !context.RequestAborted.IsCancellationRequested) {
                if (result.Position >= result.Length)
                    Thread.Sleep(100);
                await result.CopyToAsync(context.Response.BodyWriter.AsStream(true));
                result.SetLength(0);
            }
        }
        else {
            var result = await methodService!.ExecuteToJson(restEaseInterfaceMethod, parsedParameters, endpointInstance);
            await context.Response.WriteAsync(result);
        }
    }

    private static async Task<Dictionary<string, object?>?> ParseParameter(this RestEaseInterfaceMethodInfo method,
        HttpContext context) {
        var parameterValues = new Dictionary<string, object?>();
        
        foreach (var parameterInfo in method.Parameters) {
            switch (parameterInfo.Location) {
                case RestEaseInterfaceParameterLocation.Path:
                    if (!context.Request.RouteValues.ContainsKey(parameterInfo.Name))
                        throw new ParameterMissingException($"Parameter '{parameterInfo.Name}' is missing in path!");
                    break;
                case RestEaseInterfaceParameterLocation.Query:
                    if (!context.Request.Query.ContainsKey(parameterInfo.Name))
                        throw new ParameterMissingException($"Parameter '{parameterInfo.Name}' is missing in query!");
                    var queryValue = context.Request.Query[parameterInfo.Name];
                    if (context.Request.Query[parameterInfo.Name].Count > 1) {
                        parameterValues.Add(parameterInfo.Name, context.Request.Query[parameterInfo.Name].ToArray());
                    }
                    if (parameterInfo.Type.IsEnum) {
                        var converted = Enum.TryParse(parameterInfo.Type, queryValue, out object? enumValue);
                        if (!converted || enumValue == null)
                            throw new ParameterConversionException($"Value of enum-parameter '{parameterInfo.Name}' is not valid! '{queryValue}'");
                        parameterValues.Add(parameterInfo.Name, converted);
                    }
                    break;
                case RestEaseInterfaceParameterLocation.Header:
                    if (!context.Request.Headers.ContainsKey(parameterInfo.Name!))
                        throw new ParameterMissingException($"Parameter '{parameterInfo.Name}' is missing in headers!");
                    var parameter = Convert.ChangeType(context.Request.Headers[parameterInfo.Name!].ToString(),
                        parameterInfo.Type);
                    parameterValues.Add(parameterInfo.Name, parameter);
                    continue;
                case RestEaseInterfaceParameterLocation.Body:
                    if (parameterInfo.Type == typeof(Stream)) {
                        parameterValues.Add(parameterInfo.Name, context.Request.Body);
                        continue;
                    }

                    if (parameterInfo.Type == typeof(byte[])) {
                        MemoryStream memoryStream = new MemoryStream();
                        await context.Request.Body.CopyToAsync(memoryStream);
                        parameterValues.Add(parameterInfo.Name, memoryStream.ToArray());
                        continue;
                    }

                    var bodyResult = await context.Request.Body.ReadToEndObject(parameterInfo.Type);
                    parameterValues.Add(parameterInfo.Name, bodyResult);
                    continue;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return parameterValues;
    }

    private static async Task<string> ReadToEnd(this Stream stream) {
        return await new StreamReader(stream).ReadToEndAsync();
    }

    private static async Task<object?> ReadToEndObject(this Stream stream, Type convertType) {
        var streamString = await stream.ReadToEnd();
        if (string.IsNullOrWhiteSpace(streamString) || streamString.Replace(" ", "") == "{}")
            return null;
        return JsonConvert.DeserializeObject(streamString, convertType);
    }
}