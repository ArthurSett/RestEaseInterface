namespace RestEaseInterface.Services.Context;

public interface IContextService
{
    void Set(HttpContext httpContext);
    HttpContext Get();
}