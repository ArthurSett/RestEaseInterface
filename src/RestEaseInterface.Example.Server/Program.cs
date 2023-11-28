using RestEaseInterface;
using RestEaseInterface.Swagger;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

//Swagger implementation (optional)
builder.Services.AddSwaggerGen(x =>
{
    x.EnableAnnotations();
    x.DocumentFilter<RestEaseInterfaceSwaggerDocumentFilter>();
});

//RestEase implementation
builder.Services.AddRestEaseInterface(); 

var app = builder.Build();

app.UseHttpsRedirection();

//Swagger implementation (optional)
app.UseSwagger();
app.UseSwaggerUI();

//RestEase implementation
app.UseRestEaseInterface(); 

app.UseAuthorization();

app.MapControllers();

app.Run();