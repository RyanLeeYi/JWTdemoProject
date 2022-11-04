using JWTdemoProject;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

var webApOpts = new WebApplicationOptions
{
    ContentRootPath = WindowsServiceHelpers.IsWindowsService() ?
        AppContext.BaseDirectory : default,
    Args = args
};
var builder = WebApplication.CreateBuilder(args);
builder.Host.UseWindowsService();

builder.Services.AddControllers();
builder.Services.AddSingleton<JwtHelpers>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // �����ҥ��ѮɡA�^�����Y�|�]�t WWW-Authenticate ���Y�A�o�̷|��ܥ��Ѫ��Բӿ��~��]
        options.IncludeErrorDetails = true; // �w�]�Ȭ� true�A���ɷ|�S�O����

        options.TokenValidationParameters = new TokenValidationParameters
        {
            // �z�L�o���ŧi�A�N�i�H�q "sub" ���Ȩó]�w�� User.Identity.Name
            NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
            // �z�L�o���ŧi�A�N�i�H�q "roles" ���ȡA�åi�� [Authorize] �P�_����
            RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",

            // �@��ڭ̳��|���� Issuer
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration.GetValue<string>("JwtSettings:Issuer"),

            // �q�`���ӻݭn���� Audience
            ValidateAudience = false,
            //ValidAudience = "JwtAuthDemo", // �����ҴN���ݭn��g

            // �@��ڭ̳��|���� Token �����Ĵ���
            ValidateLifetime = true,

            // �p�G Token ���]�t key �~�ݭn���ҡA�@�볣�u��ñ���Ӥw
            ValidateIssuerSigningKey = false,

            // "1234567890123456" ���ӱq IConfiguration ���o
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("JwtSettings:SignKey")))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Urls.Add("http://*:8888");

app.UseAuthentication();
app.UseAuthorization();

// �n�J�è��o JWT Token
app.MapPost("/signin", (LoginViewModel login, JwtHelpers jwt) =>
{
    if (ValidateUser(login))
    {
        var token = jwt.GenerateToken(login.Username);
        return Results.Ok(new { token });
    }
    else
    {
        return Results.BadRequest();
    }
})
    .WithName("SignIn")
    .AllowAnonymous();

// ���o JWT Token �����Ҧ� Claims
app.MapGet("/claims", (ClaimsPrincipal user) =>
{
    return Results.Ok(user.Claims.Select(p => new { p.Type, p.Value }));
})
    .WithName("Claims")
    .RequireAuthorization();

// ���o JWT Token �����ϥΪ̦W��
app.MapGet("/username", (ClaimsPrincipal user) =>
{
    return Results.Ok(user.Identity?.Name);
})
    .WithName("Username")
    .RequireAuthorization();

// ���o�ϥΪ̬O�_�֦��S�w����
app.MapGet("/isInRole", (ClaimsPrincipal user, string name) =>
{
    return Results.Ok(user.IsInRole(name));
})
    .WithName("IsInRole")
    .RequireAuthorization();

// ���o JWT Token ���� JWT ID
app.MapGet("/jwtid", (ClaimsPrincipal user) =>
{
    return Results.Ok(user.Claims.FirstOrDefault(p => p.Type == "jti")?.Value);
})
    .WithName("JwtId")
    .RequireAuthorization();

bool ValidateUser(LoginViewModel login)
{
    try
    {
        string connectString = @"Data Source=P43N10812-03\SQLEXPRESS02;Persist Security Info=True;Initial Catalog=TEST_CMS;User ID=sa;Password=sa";
        return DBHandler.Login(connectString,login.Username,login.Password);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
        return false;
    }
}


app.Run();

record LoginViewModel(string Username, string Password);