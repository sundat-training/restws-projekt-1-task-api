using feature_complete.Data;
using feature_complete.Middleware;
using feature_complete.Services;
using feature_complete.Validators;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Database
builder.Services.AddSingleton<DatabaseConfig>();

// Services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<TaskService>();

// Validators
builder.Services.AddScoped<IValidator<CreateTaskRequest>, CreateTaskRequestValidator>();
builder.Services.AddScoped<IValidator<UpdateTaskRequest>, UpdateTaskRequestValidator>();
builder.Services.AddScoped<IValidator<RegisterRequest>, RegisterRequestValidator>();
builder.Services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();
builder.Services.AddScoped<IValidator<TaskQueryParameters>, TaskQueryParametersValidator>();

// Explicit validator registrations for controller injection
builder.Services.AddScoped<CreateTaskRequestValidator>();
builder.Services.AddScoped<UpdateTaskRequestValidator>();
builder.Services.AddScoped<RegisterRequestValidator>();
builder.Services.AddScoped<LoginRequestValidator>();
builder.Services.AddScoped<TaskQueryParametersValidator>();

// Learn more about configuring Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Custom authentication middleware
app.UseAuthenticationMiddleware();

app.UseAuthorization();
app.MapControllers();

app.Run();
