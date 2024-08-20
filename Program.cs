using DatabaseContex;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
var  allowedOrigins = "_allowedOriginsPolicy";
string? connectionString = builder.Configuration.GetConnectionString("ProjectDb");
if (connectionString == null) {
    throw new ArgumentException("Must provide a connection string in appsettings.json");
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddDbContext<ProjectContext>(
    options => options.UseSqlServer(connectionString)
);

builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

builder.Services.AddCors(options => {
    options.AddPolicy(
        name: allowedOrigins,
        policy => {
            policy.WithOrigins(
                "http://localhost:3000/*"
            )
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

app.UseHttpsRedirection();
app.MapControllers();

app.Run();