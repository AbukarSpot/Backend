using DatabaseContex;
using ExceptionHandlers;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// "ProjectDb": "Server=sqlserverhostv.database.windows.net;Database=Project;Authentication=Sql Password;User ID=adminUsername;Password=adminPass1;TrustServerCertificate=True;"
var  corsPolicy = "_allowedOriginsPolicy";
string? connectionString = builder.Configuration.GetConnectionString("ProjectDb");
// string? connectionString = "";
if (connectionString == null) {
    throw new ArgumentException("Must provide a connection string in appsettings.json");
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddDbContext<ProjectContext>(
    options => options.UseSqlServer(
        connectionString: connectionString,
        sqlServerOptionsAction: providerOptions => {
            providerOptions.EnableRetryOnFailure(1);
        }
    )
);

builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

builder.Services.AddCors(options => {
    options.AddPolicy(
        name: corsPolicy,
        policy => {
            policy
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        }
    );
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandler>();
app.UseCors(corsPolicy);
app.UseAuthorization();
app.UseHttpsRedirection();
app.MapControllers();

app.Run();