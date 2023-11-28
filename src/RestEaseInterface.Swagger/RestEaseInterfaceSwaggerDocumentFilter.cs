using Microsoft.OpenApi.Models;
using RestEase;
using RestEaseInterface.Services.Method;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace RestEaseInterface.Swagger;

public class RestEaseInterfaceSwaggerDocumentFilter : IDocumentFilter {
    private IMethodService _methodService;

    public RestEaseInterfaceSwaggerDocumentFilter(IMethodService methodService) {
        _methodService = methodService;
    }

    public void Apply(OpenApiDocument openApiDocument, DocumentFilterContext context) {
        var securityRequirement = new OpenApiSecurityRequirement();
        openApiDocument.SecurityRequirements.Add(securityRequirement);
        foreach (var cloudMethod in _methodService.GetMethod().Result) {
            var operation = new OpenApiOperation();
            operation.Tags.Add(new OpenApiTag { Name = cloudMethod.EndpointPath });
            operation.Parameters = new List<OpenApiParameter>();
            foreach (var parameter in cloudMethod.Parameters) {
                if (parameter.Attributes.Any(x =>
                        x is BodyAttribute)
                    || !parameter.Type.IsPrimitive && parameter.Type != typeof(String)
                                                   && !parameter.Type.IsEnum
                                                   && parameter.Attributes.All(x =>
                                                       x is not QueryAttribute && x is not HeaderAttribute)) {
                    if (parameter.Type == typeof(Stream)) {
                        operation.RequestBody = new OpenApiRequestBody {
                            Content = new Dictionary<string, OpenApiMediaType> {
                                {
                                    "application/octet-stream", new OpenApiMediaType {
                                        Schema = new OpenApiSchema {
                                            Type = "object",
                                            Properties = new Dictionary<string, OpenApiSchema> {
                                                {
                                                    "file", new OpenApiSchema {
                                                        Type = "string",
                                                        Format = "binary"
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            },
                            Required = true
                        };
                    }
                    else {
                        operation.RequestBody = new OpenApiRequestBody {
                            Content = new Dictionary<string, OpenApiMediaType> {
                                {
                                    "application/json", new OpenApiMediaType {
                                        Schema = parameter.Type.GetSchemaForType()
                                    }
                                }
                            },
                            Required = true
                        };
                    }
                }
                else {
                    operation.Parameters.Add(new OpenApiParameter() {
                        Name = parameter.Name,
                        Schema = parameter.Type.GetSchemaForType(),
                        In = parameter.Attributes.Any(x => x is HeaderAttribute)
                            ? ParameterLocation.Header
                            : ParameterLocation.Query
                    });
                }
            }

            var properties = new Dictionary<string, OpenApiSchema>();

            if (cloudMethod.ReturnType != null) {
                foreach (var property in cloudMethod.ReturnType.GetProperties()) {
                    properties.Add(property.Name, property.DeclaringType!.GetSchemaForType());
                }
            }

            var response = new OpenApiResponse {
                Description = "Success"
            };

            if (cloudMethod.ReturnType != null) {
                if (cloudMethod.ReturnType == typeof(Stream)) {
                    response.Content.Add("application/octet-stream", new OpenApiMediaType {
                        Schema = new OpenApiSchema {
                            Type = "object",
                            Properties = new Dictionary<string, OpenApiSchema> {
                                {
                                    "file", new OpenApiSchema {
                                        Type = "string",
                                        Format = "binary"
                                    }
                                }
                            }
                        }
                    });
                }
                else {
                    response.Content.Add("application/json", new OpenApiMediaType {
                        Schema = cloudMethod.ReturnType.GetSchemaForType()
                    });
                }
            }

            operation.Responses.Add("200", response);

            var pathItem = new OpenApiPathItem();

            switch (cloudMethod.HttpMethod.Method.ToUpper()) {
                case "POST":
                    pathItem.AddOperation(OperationType.Post, operation);
                    break;
                case "GET":
                    pathItem.AddOperation(OperationType.Get, operation);
                    break;
                case "PUT":
                    pathItem.AddOperation(OperationType.Put, operation);
                    break;
                case "DELETE":
                    pathItem.AddOperation(OperationType.Delete, operation);
                    break;
                case "PATCH":
                    pathItem.AddOperation(OperationType.Patch, operation);
                    break;
                default:
                    throw new NotImplementedException(
                        $"Cant find any valid HttpMethod [{cloudMethod.HttpMethod.Method.ToUpper()}] '{cloudMethod.EndpointPath}'!");
            }

            openApiDocument?.Paths.Add(cloudMethod.EndpointPath, pathItem);
        }
    }
}