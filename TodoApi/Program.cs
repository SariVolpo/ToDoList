using Microsoft.EntityFrameworkCore;
using TodoApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// הזרקת ה-Context 
builder.Services.AddDbContext<ToDoDbContext>();
// הגדרת CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(origin => true)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // מאפשר שליחת טוקנים בבטחה
    });
});

builder.Services.AddControllers();

// הוספת מחולל ה-Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();   

// מפתח סודי 
var jwtKey = "YourSuperSecretLongKey1234567890!";
var key = Encoding.ASCII.GetBytes(jwtKey);

// טיפול בטוקן
builder.Services.AddAuthentication(options =>
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
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCors("AllowAll");

// הפעלת Swagger 
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = "swagger";
});

// הגשת קבצים סטטיים
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

//  ה-Routes של ה-API
// שליפת כל המשימות
app.MapGet("/items/all", async (ToDoDbContext db) =>
    await db.Items.ToListAsync());

// שליפת כל המשימות של המשתמש
app.MapGet("/items", async (ToDoDbContext db, ClaimsPrincipal user) =>
{
    // שליפת ה-Claim
    var userIdClaim = user.FindFirst("id")?.Value;
    // בדיקה שהערך קיים והוא אכן מספר
    if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
    {
        return Results.Unauthorized();
    }
    return Results.Ok(await db.Items.Where(i => i.UserId == userId).ToListAsync());
}).RequireAuthorization();

// הוספת משימה חדשה המשויכת למשתמש מהטוקן
app.MapPost("items/", async (ToDoDbContext db, Item newItem, ClaimsPrincipal user) =>
{
    // 1. שליפת ה-ID מהטוקן 
    var userIdClaim = user.FindFirst("id")?.Value;
    if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
    {
        return Results.Unauthorized();
    }
    // 2. עדכון ה-UserId בתוך האובייקט החדש
    newItem.UserId = userId;
    // 3. שמירה למסד הנתונים
    db.Items.Add(newItem);
    await db.SaveChangesAsync();

    return Results.Created($"/items/{newItem.IdItems}", newItem);
}).RequireAuthorization(); //  מוודא שרק משתמש עם טוקן יכול לגשת

// עדכון משימה
app.MapPut("/items/{id}", async (ToDoDbContext db, int id, Item inputItem) =>
{
    var todo = await db.Items.FindAsync(id);
    if (todo is null)
        return Results.NotFound();
    if (inputItem.Name != null)
        todo.Name = inputItem.Name;
    todo.IsComplete = inputItem.IsComplete;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// מחיקת משימה
app.MapDelete("/items/{id}", async (ToDoDbContext db, int id) =>
{
    var item = await db.Items.FindAsync(id);
    if (item is null)
        return Results.NotFound();

    db.Items.Remove(item);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// הרשמה
app.MapPost("/register", async (ToDoDbContext db, User newUser) =>
{
    // 1. בדיקה אם שם המשתמש כבר קיים
    if (await db.Users.AnyAsync(u => u.Username == newUser.Username))
    {
        return Results.BadRequest("כבר קיים שם משתמש בשם זה");
    }
    // 2. הצפנת הסיסמה (Hashing)
    // הפונקציה הופכת את הסיסמה למחרוזת מוצפנת שלא ניתן להחזיר למצבה הקודם
    newUser.Password = BCrypt.Net.BCrypt.HashPassword(newUser.Password);
    // 3. שמירה למסד הנתונים
    db.Users.Add(newUser);
    await db.SaveChangesAsync();

    return Results.Ok(new { message = "User registered successfully" });
});

// כניסה
app.MapPost("/login", async (ToDoDbContext db, User loginUser) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.Username == loginUser.Username);

    if (user == null || !BCrypt.Net.BCrypt.Verify(loginUser.Password, user.Password))
    {
        return Results.Unauthorized();
    }
    var tokenHandler = new JwtSecurityTokenHandler();
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[] {
            new Claim("id", user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username ?? "")
        }),
        Expires = DateTime.UtcNow.AddDays(7),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };
    var token = tokenHandler.CreateToken(tokenDescriptor);
    var tokenString = tokenHandler.WriteToken(token);

    return Results.Ok(new { token = tokenString });
});


app.MapFallbackToFile("index.html");

app.Run();
