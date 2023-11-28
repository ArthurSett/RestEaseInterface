namespace RestEaseInterface.Services.Context;

public class ContextService : IContextService
{
    private HttpContext _context;
    
    public void Set(HttpContext httpContext)
    {
        if (_context != null)
            throw new InvalidOperationException("Cant set Context twice!");
        _context = httpContext;
    }

    public HttpContext Get()
    {
        return _context;
    }
}