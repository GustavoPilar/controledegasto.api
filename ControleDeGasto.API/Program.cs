using ControleDeGasto.API.Api.Filters;
using ControleDeGasto.API.Application.Configuration;
using ControleDeGasto.API.Application.Interfaces;
using ControleDeGasto.API.Application.Services;
using ControleDeGasto.API.Domain.Entities;
using ControleDeGasto.API.Infra.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options => { options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")); });

// Configuração do Identity
builder.Services.AddIdentityCore<User>(options =>
{
    options.User.AllowedUserNameCharacters = builder.Configuration["Identity:AllowedUserNameCharacters"]!;
    options.User.RequireUniqueEmail = true;

    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;

    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

    options.SignIn.RequireConfirmedEmail = true;
    options.SignIn.RequireConfirmedAccount = true;
})
    .AddRoles<IdentityRole<Guid>>() // Adiciona as Roles
    .AddSignInManager() // Adiciona o serviço gerenciador de SigIn
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders(); // Adiciona o provedor padrão de token

// Adicionando autenticação via Cookie
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddIdentityCookies(); // Registra o cookie handler para os schemes do Identity

// Configuração do Cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    // Impede acesso via JS (evita XSS)
    options.Cookie.HttpOnly = true;

    // Cookie só trafega em HTTPS
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

    // Apenas requisições do mesmo site (porta não interfere, desde seja a mesma origem)
    options.Cookie.SameSite = SameSiteMode.Strict;

    // Tempo de expiração da sessão
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;

    // O que acontece com status 401
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };

    // O que acontece com status 403
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

// Politica de cors para SPA com cookies
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularSpaPolicy", policy =>
    {
        policy.WithOrigins("https://localhost:4201")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // Necessário para cookie ser enviado/recebido
    });
});

// Adicionando Antiforgery (Proteção CSRF)
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = builder.Configuration["XSRF:XSRF_HEADER_NAME"];
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// Adicionando politica de Fallback para controladores
// nascerem com [Authorize] automaticamente
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Adiciona a Rating Limit (proteção contra muitas requisições)
builder.Services.AddRateLimiter(options =>
{
    // Response padrão quando o limite é execido. Default => 503
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Política para endpoint de login: evita spam de login
    options.AddPolicy("LoginPolicy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknow",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5, // Quantidade máxima de requisições permitida por janela de tempo
                Window = TimeSpan.FromMinutes(1), // Duração da janela de tempo
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst, //  FIFO -> Primeiro a entrar é o primeiro a sair
                QueueLimit = 0 // Quantidade máxima de requisições que podem aguardar na fila (0 = não enfileira)
            }));

    // Politica para endpoint de registro: evita spam de criação de contas
    options.AddPolicy("RegisterPolicy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknow",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 3,
                Window = TimeSpan.FromMinutes(5),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // Política genérica para o restante da API (proteção geral)
    options.AddPolicy("GlobalPolicy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknow",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 1
            }));

    // Callback executado quando a requisição é rejeitada - customiza o corpo da resposta
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "application/json";

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter = retryAfter.TotalSeconds.ToString();
        }

        await context.HttpContext.Response.WriteAsJsonAsync(
            new { Message = "Muitas requisições. Tente novamente em instantes." },
            cancellationToken);
    };

});

// Adicionando o filtro para validar antiforgery (Proteção CSRF)
builder.Services.AddScoped<ValidateAntiforgeryTokenFilter>();

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

// Vale para todos os tokens do DataProtection (confirmação de e-mail, reset de senha).
// 15 minutos dão folga para o e-mail ser entregue e aberto, sem deixar um link
// válido por muito tempo caso a caixa de entrada seja comprometida.
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromMinutes(15);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.UseCors("AngularSpaPolicy");
}

app.UseHttpsRedirection();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers().RequireRateLimiting("GlobalPolicy");

using (IServiceScope scope = app.Services.CreateScope())
{
    RoleManager<IdentityRole<Guid>> roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

    await RoleSeender.SeedAsync(roleManager);
}

app.Run();
