using Microsoft.EntityFrameworkCore;
using System.Text;
using WebManager.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using WebManager.Services;

var builder = WebApplication.CreateBuilder(args);

// Adiciona suporte a controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<FinanceService>();

// Configura o DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configura o CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev",
        builder =>
        {
            builder.WithOrigins("http://localhost:4200") // Endereço padrão do Angular em desenvolvimento
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

// Configura a autenticação JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII
                .GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value ?? throw new InvalidOperationException("JWT Token not configured."))),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

var app = builder.Build();

// Configuração do ambiente de desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Seed data
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<AppDbContext>();
        try
        {
            context.Database.Migrate();
            Console.WriteLine("Program.cs: Database migrations applied.");
        }
        catch (Exception ex)
        {
            // Log the error, but allow the application to continue if it's just a migration issue
            Console.WriteLine($"Program.cs: Error applying migrations: {ex.Message}");
        }
        Console.WriteLine("Program.cs: Calling DataSeeder.SeedData...");
        DataSeeder.SeedData(context);
        Console.WriteLine("Program.cs: DataSeeder.SeedData completed.");

        // Process fixed finances
        Console.WriteLine("Program.cs: Calling FinanceService.ProcessFixedFinances...");
        var financeService = services.GetRequiredService<FinanceService>();
        financeService.ProcessFixedFinances();
        Console.WriteLine("Program.cs: FinanceService.ProcessFixedFinances completed.");

        Console.WriteLine($"Program.cs: Final count of Finance entries: {context.Finances.Count()}");
    }
}

// Aplica a política de CORS
app.UseCors("AllowAngularDev");

// HTTPS redirection
app.UseHttpsRedirection();

// Adiciona o middleware de autenticação
app.UseAuthentication();
app.UseAuthorization();

// Aqui mapeia seus controllers
app.MapControllers();

app.Run();