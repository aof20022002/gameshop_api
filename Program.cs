var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// **สำคัญ: เปิด Swagger ทุก Environment รวมถึง Production**
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    c.RoutePrefix = string.Empty; // ทำให้ Swagger เป็นหน้าแรก (root path)
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();