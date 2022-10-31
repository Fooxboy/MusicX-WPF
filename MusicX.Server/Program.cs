using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MusicX.Server.Hubs;
using MusicX.Server.Services;
using MusicX.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR(options => options.EnableDetailedErrors = true).AddProtobufProtocol();
builder.Services.AddMemoryCache();
builder.Services.AddAuthorization();
builder.Services.AddMvc(options => options.EnableEndpointRouting = false);
builder.Services.AddRazorPages();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new()
    {
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey
            (Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = true
    };
});

builder.Services.AddSingleton<SessionService>();
builder.Services.AddTransient<ListenTogetherService>();

var app = builder.Build();

app.Urls.Add("http://0.0.0.0:5000");
//app.Urls.Add("https://0.0.0.0:5001");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseMvc(routeBuilder => routeBuilder.MapRoute("default", "{controller}/{action=Index}"));

app.MapControllers();
app.MapRazorPages();
app.MapHub<ListenTogetherHub>("/hubs/listen").RequireAuthorization();

app.MapPost("/token",
            [AllowAnonymous] ([FromBody] long userId) =>
            {
                var issuer = builder.Configuration["Jwt:Issuer"];
                var audience = builder.Configuration["Jwt:Audience"];
                var key = Encoding.UTF8.GetBytes
                    (builder.Configuration["Jwt:Key"]);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new(new[]
                    {
                        new Claim("Id", Guid.NewGuid().ToString()),
                        new Claim(JwtRegisteredClaimNames.NameId, userId.ToString()),
                        new Claim(JwtRegisteredClaimNames.Jti,
                                  Guid.NewGuid().ToString())
                    }),
                    Issuer = issuer,
                    Audience = audience,
                    SigningCredentials = new(new SymmetricSecurityKey(key),
                                             SecurityAlgorithms.HmacSha512Signature)
                };
                
                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var stringToken = tokenHandler.WriteToken(token);
                
                return Results.Ok(stringToken);
            });




app.Run();