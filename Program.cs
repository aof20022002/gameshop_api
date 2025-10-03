var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ** เปิด Swagger ใน Production (ไม่มีเงื่อนไข) **
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "GameShop API V1");
    c.RoutePrefix = string.Empty; // ทำให้ Swagger เป็นหน้าแรก
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();