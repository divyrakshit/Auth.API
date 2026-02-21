using Auth.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddOpenApi();

// Add custom services
builder.Services.AddSingleton<IUserStore, InMemoryUserStore>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Add controllers
builder.Services.AddControllers();

// Configure JWT Authentication
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"];
if (string.IsNullOrWhiteSpace(jwtSecretKey) || jwtSecretKey.Length < 32)
{
    throw new InvalidOperationException("JWT:SecretKey must be configured in appsettings.json and be at least 32 characters long");
}

var key = Encoding.UTF8.GetBytes(jwtSecretKey);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "AuthAPI",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "AuthAPIUsers",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("default", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("default");

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Add root endpoint to show API homepage
app.MapGet("/", () => Results.Content(GetHomeHtml(), "text/html"));

app.MapControllers();

string GetHomeHtml()
{
    return """
        <!DOCTYPE html>
        <html lang="en">
        <head>
            <meta charset="UTF-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
            <title>Auth.API - Authentication Service</title>
            <style>
                * { margin: 0; padding: 0; box-sizing: border-box; }
                body { 
                    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                    min-height: 100vh; display: flex; justify-content: center; align-items: center; padding: 20px;
                }
                .container { background: white; border-radius: 15px; box-shadow: 0 20px 60px rgba(0,0,0,0.3);
                    max-width: 900px; width: 100%; padding: 50px;
                }
                h1 { color: #333; margin-bottom: 10px; font-size: 2.5em; }
                .subtitle { color: #666; margin-bottom: 30px; font-size: 1.1em; }
                .status { background: #d4edda; border: 2px solid #28a745; border-radius: 10px;
                    padding: 15px; margin-bottom: 30px; color: #155724; font-weight: bold;
                    text-align: center; font-size: 1.1em;
                }
                .section { margin-bottom: 40px; }
                .section h2 { color: #667eea; margin-bottom: 15px; font-size: 1.5em;
                    border-bottom: 3px solid #667eea; padding-bottom: 10px;
                }
                .endpoints { display: grid; gap: 15px; margin-bottom: 20px; }
                .endpoint { background: #f8f9fa; border-left: 4px solid #667eea;
                    padding: 15px; border-radius: 5px;
                }
                .endpoint-method { display: inline-block; background: #667eea; color: white;
                    padding: 5px 10px; border-radius: 3px; font-weight: bold;
                    margin-right: 10px; min-width: 60px; text-align: center;
                }
                .endpoint-method.post { background: #28a745; }
                .endpoint-method.get { background: #007bff; }
                .endpoint-url { font-family: 'Courier New', monospace; color: #333; font-size: 1em; }
                .endpoint-desc { color: #666; font-size: 0.9em; margin-top: 5px; }
                .users { display: grid; grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
                    gap: 15px;
                }
                .user-card { background: #e7f3ff; border: 2px solid #b3d9ff; border-radius: 8px;
                    padding: 15px;
                }
                .user-card strong { color: #004085; font-size: 1.1em; }
                .user-card .password { color: #666; font-family: 'Courier New', monospace;
                    margin: 5px 0;
                }
                .user-card .roles { color: #ffc107; margin-top: 8px; font-size: 0.9em; }
                .quick-links { display: flex; gap: 15px; flex-wrap: wrap; margin-top: 30px; }
                .btn { display: inline-block; background: #667eea; color: white;
                    padding: 12px 25px; border-radius: 8px; text-decoration: none;
                    font-weight: bold; transition: background 0.3s; text-align: center;
                    border: none; cursor: pointer; font-size: 1em;
                }
                .btn:hover { background: #764ba2; }
                .btn-secondary { background: #6c757d; }
                .btn-secondary:hover { background: #5a6268; }
            </style>
        </head>
        <body>
            <div class="container">
                <h1>🔐 Auth.API</h1>
                <p class="subtitle">JWT Authentication & Authorization Service</p>
                
                <div class="status">
                    ✓ API is running successfully on http://localhost:5268
                </div>

                <div class="section">
                    <h2>📋 API Endpoints</h2>
                    <div class="endpoints">
                        <div class="endpoint">
                            <span class="endpoint-method post">POST</span>
                            <span class="endpoint-url">/api/auth/login</span>
                            <div class="endpoint-desc">Generate JWT token with credentials</div>
                        </div>
                        <div class="endpoint">
                            <span class="endpoint-method get">GET</span>
                            <span class="endpoint-url">/api/auth</span>
                            <div class="endpoint-desc">Health check endpoint</div>
                        </div>
                        <div class="endpoint">
                            <span class="endpoint-method get">GET</span>
                            <span class="endpoint-url">/api/secure/user</span>
                            <div class="endpoint-desc">Get user info (requires authentication)</div>
                        </div>
                        <div class="endpoint">
                            <span class="endpoint-method get">GET</span>
                            <span class="endpoint-url">/api/secure/admin</span>
                            <div class="endpoint-desc">Get admin info (requires Admin role)</div>
                        </div>
                    </div>
                </div>

                <div class="section">
                    <h2>👥 Test Users</h2>
                    <div class="users">
                        <div class="user-card">
                            <strong>admin</strong>
                            <div class="password">Password: admin123</div>
                            <div class="roles">✓ Admin, User</div>
                        </div>
                        <div class="user-card">
                            <strong>user</strong>
                            <div class="password">Password: user123</div>
                            <div class="roles">✓ User</div>
                        </div>
                        <div class="user-card">
                            <strong>moderator</strong>
                            <div class="password">Password: moderator123</div>
                            <div class="roles">✓ Moderator, User</div>
                        </div>
                    </div>
                </div>

                <div class="section">
                    <h2>🚀 Quick Start</h2>
                    <div class="quick-links">
                        <a href="ApiTester.html" class="btn">🧪 Open Visual Tester</a>
                    </div>
                </div>
            </div>
        </body>
        </html>
        """;
}

app.Run();
