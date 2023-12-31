using CloudinaryDotNet;
using dotenv.net;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using PostAPI;
using PostAPI.Interfaces;
using PostAPI.OptionsSetup;
using PostAPI.Repositories;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins, policy => { policy.WithOrigins("http://localhost:3000", "http://localhost:4200").AllowAnyHeader().AllowAnyMethod(); });
});

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "access_token";
    })
    .AddJwtBearer();

// Jwt Validation options
builder.Services.ConfigureOptions<JwtOptionsSetup>();
builder.Services.ConfigureOptions<JwtBearerOptionsSetup>();
builder.Services.ConfigureOptions<RoleAuthorizationOptionsSetup>();

builder.Services.AddScoped<IUser, UserRepository>();
builder.Services.AddScoped<IPost, PostRepository>();
builder.Services.AddScoped<IToken, TokenRepository>();
builder.Services.AddScoped<IComment, CommentRepository>();
builder.Services.AddScoped<IImage, ImageRepository>();
builder.Services.AddScoped<IGroups, GroupsRepository>();
builder.Services.AddScoped<IMessage, MessageRepository>();
builder.Services.AddScoped<ITier, TierRepository>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// * Cloudinary
DotEnv.Load(options: new DotEnvOptions(probeForEnv: true));
Cloudinary cloudinary = new(Environment.GetEnvironmentVariable("CLOUDINARY_URL"));
cloudinary.Api.Secure = true;

builder.Services.AddSingleton(cloudinary);

var app = builder.Build();

// Overriding FluentValidation language
ValidatorOptions.Global.LanguageManager.Enabled = false;

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
