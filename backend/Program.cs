using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Resturant_Backend.Common.Helpers;
using Resturant_Backend.Common.Responses;
using Resturant_Backend.Data;
using Resturant_Backend.Helpers;
using Resturant_Backend.Helpers.PhotosHandle;
using Resturant_Backend.Hubs;
using Resturant_Backend.Interfaces;
using Resturant_Backend.Middlewares;
using Resturant_Backend.Models;
using Resturant_Backend.Repository;
using Resturant_Backend.Services;
using System.Text;
using System.Text.Json;

namespace Resturant_Backend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            // Unifying automatic Model Validation errors to be returned within the unified ApiResponse
            builder.Services.AddControllers()
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = context =>
                    {
                        var errors = context.ModelState
                            .Where(e => e.Value != null && e.Value.Errors.Count > 0)
                            .ToDictionary(
                                kvp => StringExtensions.ToCamelCase(kvp.Key),
                                kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToList()
                            );

                        var response = ApiResponse<object>.FailureResponse(
                            statusCode: 400,
                            message: "يرجى التأكد من صحة البيانات المدخلة.",
                            errors: errors
                        );

                        return new BadRequestObjectResult(response);
                    };
                });

            builder.Services.AddEndpointsApiExplorer();



            builder.Services.AddSignalR();








            // Swagger Config
            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "ادخل التوكن بتاعك هنا مباشرة (بدون كلمة Bearer)"
                });

                options.AddSecurityRequirement(document => new OpenApiSecurityRequirement{
                    {
                        new OpenApiSecuritySchemeReference("Bearer", document),
                        new List<string>()
                    }
                });
            });

            builder.Services.Configure<JwtHelper>(builder.Configuration.GetSection("JWT"));

            // Identity Config
            builder.Services.AddIdentity<Appuser, IdentityRole>(op =>
            {
                op.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                op.Lockout.MaxFailedAccessAttempts = 3;
                op.Lockout.AllowedForNewUsers = true;

                op.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ أبتثجحخدذرزسشصضطظعغفقكلمنهوي";

                op.Password.RequireDigit = true;
                op.Password.RequireLowercase = true;
                op.Password.RequireUppercase = true;
                op.Password.RequireNonAlphanumeric = true;
            }).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

            // Authentication & JwtBearer Config
            builder.Services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,

                    ValidIssuer = builder.Configuration["JWT:Issuer"],
                    ValidAudience = builder.Configuration["JWT:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]!))
                };


                o.Events = new JwtBearerEvents
                {

                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if(!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    },
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";

                        var response = ApiResponse<object>.FailureResponse(401, "غير مصرح لك بالوصول، يرجى تسجيل الدخول أولاً.");

                        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
                    },
                    OnForbidden = async context =>
                    {
                        context.Response.StatusCode = 403;
                        context.Response.ContentType = "application/json";

                        var response = ApiResponse<object>.FailureResponse(403, "ليس لديك الصلاحيات الكافية لإتمام هذا الإجراء.");

                        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
                    }
                };
            });

            // CORS Config
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins("http://localhost:5174")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            builder.Services.AddAutoMapper(cfg => { }, typeof(Program).Assembly);
            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
            builder.Services.AddTransient<IEmailService, EmailService>();



            builder.Services.Configure<PaymobSettings>(builder.Configuration.GetSection("Paymob"));
            builder.Services.AddHttpClient<PaymobService>();





            builder.Services.AddDbContext<AppDbContext>(op =>
            {
                op.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            builder.Services.AddScoped<IAuthService, Authservice>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWorkRepo>();

            builder.Services.AddScoped<IPhotoService, PhotoService>();

            var app = builder.Build();
            app.MapHub<OrderHub>("/hubs/orders");

            app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

            if(app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("AllowFrontend");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}