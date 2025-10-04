using gameshop_api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DatabaseHelper
builder.Services.AddSingleton<DatabaseHelper>();

// Add CORS 
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "GameShop API V1");
    c.RoutePrefix = string.Empty;
});


app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();