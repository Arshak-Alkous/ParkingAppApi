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
    if (user == null || string.IsNullOrEmpty(user.UserName) || string.IsNullOrEmpty(user.Licenseplate))
    {
        return Results.BadRequest("invalid user data");
    }

    int? userid = parkingManager.RegisterUser(user);
    if (userid == null)
    {
        return Results.Conflict("user already exists");
    }
        
    return Results.Ok(new { Message = $"User {user.UserId} registered successfully" , UserID=user.UserId});
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
       // string fullName = user.FirstName + "," + user.LastName;
        return Results.Ok(new { message = $"Successful login fo user: {loginRequest.UserName}", user});
    }
});
app.MapGet("/user-details/", (int userid) =>
{
    User? user = parkingManager.GetUserDetails(userid);
    List<Period>? periods=parkingManager.GetAllPeriodsForUser(userid);
    if (user != null)
        return Results.Ok(new
        {
            message = " get user’s all registered details ",
            fullname = user.FirstName + " " + user.LastName,
            licenseplate = user.Licenseplate,
            balance = user.Balance,
            Periods = periods
        });
    return Results.NotFound("user not found");
});
app.MapGet("/previous-sessions", (int userid) => 
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
app.MapPost("/start-session", (UserIdReq userIdReq) =>
{
    //var userIdReq = await req.ReadFromJsonAsync<UserIdReq>();
    if (userIdReq == null || userIdReq.UserId <= 0)
    {
        return Results.BadRequest(new { message = "Invalid user Id" });
    }
    int periodid=parkingManager.BeginNewPeriod(userIdReq.UserId);
    return Results.Ok(new
    {
        message= $"period{periodid} begin for user {userIdReq.UserId}",
        periodId=periodid,
        startTime=DateTime.Now,
    });
});
app.MapGet("/current-session", (int userid) =>
{
    Period? presentPeriod=parkingManager.GetPresentPeriod(userid);
    if (presentPeriod==null)
    {
        return Results.NotFound(new {message= $"There is no current period for this user:{userid}",isActive=false });
    }
    var totalHuors=(DateTime.Now-presentPeriod.StartTime).TotalHours;
    var totalMinutes= (DateTime.Now - presentPeriod.StartTime).TotalMinutes;
    return Results.Ok(new
    {
        message= $"the current Period for {userid} is:  {presentPeriod.PeriodId}," ,
        totalHours=totalHuors,
        totalMinutes=totalMinutes,
        startTime=presentPeriod.StartTime,
        cost=presentPeriod.PeriodCost,
        isActive=true

    });
});
app.MapPost("/end-session",  (UserIdReq userIdReq) =>
{
    //var userIdReq = await req.ReadFromJsonAsync<UserIdReq>();
    if (userIdReq == null || userIdReq.UserId <= 0)
    {
        return Results.BadRequest(new { message = "Invalid user Id" });
    }
    Period? endPeriod = parkingManager.EndPresentPeriod(userIdReq.UserId);
    if (endPeriod == null)
    {
        return Results.NotFound(new {message= $"There is no current period for this user:{userIdReq.UserId}" });
    }
    var totalHuors = (endPeriod.EndTime.Value - endPeriod.StartTime).TotalHours;
    var totalMinutes = (endPeriod.EndTime.Value - endPeriod.StartTime).TotalMinutes;
    return Results.Ok(new
    {
        message = $"the current Period: {endPeriod.PeriodId} is ended for {userIdReq.UserId} ,",
        cost=endPeriod.PeriodCost,
        totalHours = totalHuors,
        totalMinutes = totalMinutes
    });
});
app.MapGet("/user-balance", (int userid) =>
{
    decimal totalCost = parkingManager.GetCostForUser(userid);
    if (totalCost == 0)
        return Results.NotFound($"User {userid} doesn't exist or no periods ended .");
    return Results.Ok(new {
        message = $"the cost for user {userid} is {totalCost}" ,
        balance = totalCost
    });
});

app.MapGet("/", () => "Parking App Swagger Api!");

app.Run();
