var builder = WebApplication.CreateBuilder(args);

// Voeg controllers toe
builder.Services.AddControllers();

// Swagger voor API-documentatie (alleen in development)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
