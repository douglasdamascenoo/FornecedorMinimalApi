using minimalFornecedor.Data;
using Microsoft.EntityFrameworkCore;
using minimalFornecedor.Models;
using MiniValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NetDevPack.Identity.Jwt;
using NetDevPack.Identity.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

#region Configure Services
builder.Services.AddDbContext<FornecedorContextDb>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Configuração do contexto para o Identity
builder.Services.AddIdentityEntityFrameworkContextConfiguration(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    b => b.MigrationsAssembly("minimalFornecedor")));

// Configuração do Identity e JWT
builder.Services.AddIdentityConfiguration();
builder.Services.AddJwtConfiguration(builder.Configuration, "AppSettings");

// Configuração básica para autorização nos endpoints - [Authorize];
// builder.Services.AddAuthorization()
// Configuração de Claim para endpoint
// - Requer a criação dela na base de dados do contexto do Identity
// - Requer a anotação de metadados no endpoint com "RequireAuthorization(claimName)"
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ExcluirFornecedor",
        policy => policy.RequireClaim("ExcluirFornecedor"));
});

builder.Services.AddEndpointsApiExplorer();
// Swagger genérico
//builder.Services.AddSwaggerGen();
// Swagger - configuração para receber token jwt
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Fornecedor Minimal API",
        Description = "Developed by Douglas Damasceno",
        Contact = new OpenApiContact { Name = "Douglas Damasceno", Email = "douglasddx@gmail.com" },
        License = new OpenApiLicense { Name = "MIT", Url = new Uri("https://opensource.org/licenses/MIT") }
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Insira o token JWT: Bearer {seu token}",
        Name = "Authorization",
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
#endregion

#region Configure Pipeline
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthConfiguration();
app.UseHttpsRedirection();

MapEndpoints(app);
#endregion

app.Run();

#region Map Endpoints
void MapEndpoints(WebApplication app)
{
    app.MapPost("/registro", [AllowAnonymous] async (
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager,
        IOptions<AppJwtSettings> appJwtSettings,
        RegisterUser registerUser) =>
        {
            if (registerUser == null)
                return Results.BadRequest("Usuário não informado");

            if (!MiniValidator.TryValidate(registerUser, out var errors))
                return Results.ValidationProblem(errors);
            var user = new IdentityUser
            {
                UserName = registerUser.Email,
                Email = registerUser.Email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, registerUser.Password);

            if (!result.Succeeded)
                return Results.BadRequest(result.Errors);

            var jwt = new JwtBuilder()
                .WithUserManager(userManager)
                .WithJwtSettings(appJwtSettings.Value)
                .WithEmail(user.Email)
                .WithJwtClaims()
                .WithUserClaims()
                .WithUserRoles()
                .BuildUserResponse();

            return Results.Ok(jwt);
        })
        .ProducesValidationProblem()
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .WithName("RegistroUsuario")
        .WithTags("Usuario");

    app.MapPost("/login", [AllowAnonymous] async (
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager,
        IOptions<AppJwtSettings> appJwtSettings,
        LoginUser loginUser) =>
        {
            if (loginUser == null)
                return Results.BadRequest("Usuário não informado");

            if (!MiniValidator.TryValidate(loginUser, out var errors))
                return Results.ValidationProblem(errors);

            var result = await signInManager.PasswordSignInAsync(loginUser.Email, loginUser.Password, false, true);

            if (result.IsLockedOut)
                return Results.BadRequest("Usuário bloqueado");

            if (!result.Succeeded)
                return Results.BadRequest("Usuário ou senha inválidos");

            var jwt = new JwtBuilder()
                        .WithUserManager(userManager)
                        .WithJwtSettings(appJwtSettings.Value)
                        .WithEmail(loginUser.Email)
                        .WithJwtClaims()
                        .WithUserClaims()
                        .WithUserRoles()
                        .BuildUserResponse();

            return Results.Ok(jwt);

        })
        .ProducesValidationProblem()
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .WithName("LoginUsuario")
        .WithTags("Usuario");

    app.MapGet("/fornecedor", [AllowAnonymous] async (
        FornecedorContextDb context) =>
        await context.Fornecedores.ToListAsync())
        .WithName("GetFornecedor")
        .WithTags("Fornecedor");

    app.MapGet("/fornecedor/{id}", [AllowAnonymous] async (
        Guid id,
        FornecedorContextDb context) =>
        await context.Fornecedores.FindAsync(id)
        is Fornecedor fornecedor
            ? Results.Ok(fornecedor)
            : Results.NotFound())
        .Produces<Fornecedor>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .WithName("GetFornecedorPorId")
        .WithTags("Fornecedor");

    app.MapPost("/fornecedor", [Authorize] async (
        FornecedorContextDb context,
        Fornecedor fornecedor) =>
        {
            if (!MiniValidator.TryValidate(fornecedor, out var errors))
                return Results.ValidationProblem(errors); //HTTP 400

            context.Fornecedores.Add(fornecedor);
            var result = await context.SaveChangesAsync();

            return result > 0
                //? Results.Created($"/forncedor/{fornecedor.Id}", fornecedor)
                ? Results.CreatedAtRoute("GetFornecedorPorId", new { id = fornecedor.Id }, fornecedor)
                : Results.BadRequest("Houve um problema ao salvar o registro");
        })
        .ProducesValidationProblem()
        .Produces<Fornecedor>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .WithName("PostFornecedor")
        .WithTags("Fornecedor");

    app.MapPut("/fornecedor/{id}", [Authorize] async (
        Guid id,
        FornecedorContextDb context,
        Fornecedor fornecedor) =>
        {
            var fornecedorBanco =
                await context.Fornecedores.AsNoTracking<Fornecedor>().FirstOrDefaultAsync(q => q.Id == id);
            if (fornecedorBanco == null) return Results.NotFound();

            if (!MiniValidator.TryValidate(fornecedor, out var errors))
                return Results.ValidationProblem(errors);

            context.Fornecedores.Update(fornecedor);
            var result = await context.SaveChangesAsync();

            return result > 0
            ? Results.NoContent()
            : Results.BadRequest("Houve um problema ao salvar o registro");
        })
        .ProducesValidationProblem()
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .WithName("PutFornecedor")
        .WithTags("Fornecedor");

    app.MapDelete("/fornecedor/{id}", [Authorize] async (
        Guid id,
        FornecedorContextDb context) =>
        {
            var fornecedor = await context.Fornecedores.FindAsync(id);
            if (fornecedor == null) return Results.NotFound();

            context.Fornecedores.Remove(fornecedor);
            var result = await context.SaveChangesAsync();

            return result > 0
            ? Results.NoContent()
            : Results.BadRequest("Houve um problema ao deletar o registro");
        })
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .RequireAuthorization("ExcluirFornecedor")
        .WithName("DeleteFornecedor")
        .WithTags("Fornecedor");
}
#endregion