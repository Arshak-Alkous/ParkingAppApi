using Microsoft.AspNetCore.Mvc;
using ParkingAppApi;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowAll");
ParkingManager parkingManager = new ParkingManager();
app.MapGet("/favicon.ico", () => Results.NotFound());

app.MapPost("/register-user", (User user)=>
{
    if (user == null || string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Licenseplate))
    {
        return Results.BadRequest("invalid user data");
    }

    int? userid = parkingManager.RegisterUser(user);
    if (userid == null)
    {
        return Results.Conflict("user already exists");
    }
        
    return Results.Ok(new { Message = $"User {user.Firstname} {user.Lastname} registered successfully" , UserID=user.UserID});
});
app.MapPost("/login", (LoginRequest loginRequest) => 
{
    User? user = parkingManager.Login(loginRequest.UserName, loginRequest.Password);
    
    if (user == null) 
    {
        return Results.BadRequest("Invalid credentials");
    }
    else
    {
       // string fullName = user.Firstname + "," + user.Lastname;
        return Results.Ok(new { message = $"Successful login fo user: {loginRequest.UserName}", user});
    }
});

app.MapPost("/start-session", ([FromBody] int userId) =>
{
    if (userId <= 0)
    {
        return Results.BadRequest(new { message = "Invalid user Id" });
    }
    int periodid=parkingManager.BeginNewPeriod(userId);
    return Results.Ok(new
    {
        message= $"period{periodid} begin for user {userId}",
        periodId=periodid,
        startTime=DateTime.Now,
    });
});
app.MapPost("/end-session", ([FromBody] int userId) =>
{
    //var userIdReq = await req.ReadFromJsonAsync<UserIdReq>();
    if (userId <= 0)
    {
        return Results.BadRequest(new { message = "Invalid user Id" });
    }
    Period? endPeriod = parkingManager.EndPresentPeriod(userId);
    if (endPeriod == null)
    {
        return Results.NotFound(new { message = $"There is no current period for this user:{userId}" });
    }
    var totalHuors = (endPeriod.EndTime.Value - endPeriod.StartTime).TotalHours;
    var totalMinutes = (endPeriod.EndTime.Value - endPeriod.StartTime).TotalMinutes;
    return Results.Ok(new
    {
        message = $"the current Period: {endPeriod.PeriodId} is ended now {endPeriod.EndTime} for {userId} ,",
        cost = endPeriod.PeriodCost,
        endTime = endPeriod.EndTime,
        totalHours = totalHuors,
        totalMinutes = totalMinutes
    });
});
app.MapGet("/current-session/{userid}", (int userid) =>
{
    Period? presentPeriod=parkingManager.GetPresentPeriod(userid);
    if (presentPeriod!=null)
    {
        var totalHuors = (DateTime.Now - presentPeriod.StartTime).TotalHours;
        var totalMinutes = (DateTime.Now - presentPeriod.StartTime).TotalMinutes;
        return Results.Ok(new
        {
            message = $"the current Period for {userid} is:  {presentPeriod.PeriodId},",
            totalHours = totalHuors,
            totalMinutes = totalMinutes,
            startTime = presentPeriod.StartTime,
            cost = presentPeriod.PeriodCost,
            isActive = true

        });
    }
    return Results.Ok(new { message = $"There is no current period for this user:{userid}", isActive = false });
});
app.MapGet("/previous-sessions/{userid}", (int userid) =>
{
    List<Period>? previousPeriodes = parkingManager.GetAllPreviousPeriodsForUser(userid);
    if (previousPeriodes != null)
        return Results.Ok(new
        {
            message = $"The previous session for {userid}",
            previousSession = previousPeriodes,
        });
    else return Results.NotFound($"There is no Previous sessions for this user:{userid}");

});
app.MapGet("/user-balance/{userid}", (int userid) =>
{
    decimal totalCost = parkingManager.GetCostForUser(userid);
    if (totalCost == 0)
        return Results.NotFound($"User {userid} doesn't exist or no periods ended .");
    return Results.Ok(new {
        message = $"the cost for user {userid} is {totalCost}" ,
        totalBalance = totalCost
    });
});
app.MapGet("/user-details", (int userid) =>
{
    User? user = parkingManager.GetUserDetails(userid);
    List<Period>? periods = parkingManager.GetAllPeriodsForUser(userid);
    if (user != null)
        return Results.Ok(new
        {
            message = " get user’s all registered details ",
            fullname = user.Firstname + " " + user.Lastname,
            licenseplate = user.Licenseplate,
            balance = user.Balance,
            Periods = periods
        });
    return Results.NotFound("user not found");
});


app.MapGet("/", () => "Parking App Swagger Api!");

app.Run();
