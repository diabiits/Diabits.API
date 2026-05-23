using Diabits.API.Helpers;

using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configure API controllers with JSON serialization
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Configure OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithAuth();

// Configure cross-origin requests for Blazor client
//TODO Refactor to make secure (also check workflow)
if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("CorsTestEnv", policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });
}
else 
{
    builder.Services.AddDiabitsCors(builder.Configuration);
}

// Configure data access and identity
builder.Services.AddDiabitsDataAccess(builder.Configuration);

// Configure JWT authentication and authorization
builder.Services.AddJwtAuthentication(builder.Configuration);

// Register application services and mappers
builder.Services.AddDiabitsServices();

var app = builder.Build();

// Configure development-specific middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Diabits API V1");
        c.RoutePrefix = string.Empty;
    });
}
else
{
    app.UseHttpsRedirection();
}

// Configure request pipeline
app.UseRouting();
app.UseCors("BlazorWasm");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Initialize database (migrations and seeding)
if (!app.Environment.IsEnvironment("Test"))
{
    await app.Services.InitializeDatabaseAsync();
}

app.Run();
