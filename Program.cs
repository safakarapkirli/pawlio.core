using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Pawlio;
using Pawlio.Hubs;
using Pawlio.IsyerimPos;
using Pawlio.Core.Utils;
using Microsoft.AspNetCore.HttpOverrides;
using Hangfire;
using Hangfire.PostgreSql;
using HangfireBasicAuthenticationFilter;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

var builder = WebApplication.CreateBuilder(args);

PostgreSqlDbContext.ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
Pawlio.Log.PostgreSqlLogDbContext.SetConnectionString(builder.Configuration.GetConnectionString("LogDbConnection")!);

IsyerimPosUtils.isTest = builder.Configuration["Environment"] == "DEV";
FirebasePushNotificationUtils.Init(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("_myAllowSpecificOrigins",
        policy =>
        {
            policy.WithOrigins(
                "http://192.168.1.10",
                "https://api.pawlio.app",
                "https://app.pawlio.app")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});
builder.Services.AddDbContext<PostgreSqlDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connectionString)
        .UseLoggerFactory(LoggerFactory.Create(b => b.AddConsole().AddFilter(level => level >= LogLevel.Information)));
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});
builder.Services.AddControllers().AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
    });

builder.Services.AddHangfire(x => x.UsePostgreSqlStorage(options => 
    options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("HangfireConnection")!)));
builder.Services.AddHangfireServer(options =>
{
    options.ServerName = "PawlioServer";
    options.WorkerCount = 1;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSignalR();
// Swagger / OpenAPI
//builder.Services.AddSwaggerGen(options =>
//{
//    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Pawlio API", Version = "v1" });
//    // Exclude actions that don't have an explicit HTTP method to avoid ambiguous-method errors
//    options.DocInclusionPredicate((docName, apiDesc) => apiDesc.HttpMethod != null);
//    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//    {
//        Name = "Authorization",
//        Type = SecuritySchemeType.Http,
//        Scheme = "bearer",
//        BearerFormat = "JWT",
//        In = ParameterLocation.Header,
//        Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\n\nExample: \"Bearer eyJhb...\""
//    });
//    options.AddSecurityRequirement(new OpenApiSecurityRequirement
//    {
//        {
//            new OpenApiSecurityScheme
//            {
//                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
//            },
//            new List<string>()
//        }
//    });
//});

var app = builder.Build();

// Seed data
// using (var scope = app.Services.CreateScope())
// {
//     var context = scope.ServiceProvider.GetRequiredService<PostgreSqlDbContext>();
//     // context.Database.EnsureCreated(); // Migration kullanıldığı için gerek olmayabilir ama ilk açılışta yararlı olabilir
//     DataSeeder.SeedAnimalData(context);
// }

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});
var fileServerOptions = new FileServerOptions()
{
    EnableDefaultFiles = true,
    EnableDirectoryBrowsing = false,
    RequestPath = new PathString(string.Empty),
};
fileServerOptions.StaticFileOptions.ServeUnknownFileTypes = true;
app.UseFileServer(fileServerOptions);

// Log
app.Use((context, next) =>
{
    context.Request.EnableBuffering();
    return next();
});
app.UseMiddleware<RequestResponseLoggerMiddleware>();
RequestResponseLoggerMiddleware.Init(); // Start Timer

app.UseStaticFiles();
app.UseRouting();
app.UseExceptionHandler("/error");
app.UseAuthentication();
app.UseAuthorization();

// Enable middleware to serve generated Swagger as JSON endpoint and Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "HizliIs API V1");
        c.RoutePrefix = "swagger"; // serve at /swagger
    });
}

app.MapControllers();
app.MapHub<MainHub>("/hubs/main");
app.MapHub<PosHub>("/hubs/pos");

app.UseHangfireDashboard("/hf-jobs", new DashboardOptions
{
    Authorization = new[]
    {
        new HangfireCustomBasicAuthenticationFilter
        {
             User = builder.Configuration.GetSection("HangfireSettings:UserName").Value,
             Pass = builder.Configuration.GetSection("HangfireSettings:Password").Value
        }
    }
});

app.Run();
