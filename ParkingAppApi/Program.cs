using ParkingAppApi;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

ParkingManager parkingManager = new ParkingManager();

app.MapPost("/register-user", (User user)=>
{
    if (user == null || string.IsNullOrEmpty(user.UserName) || string.IsNullOrEmpty(user.CarPlateNumber))
    {
        return Results.BadRequest("invalid user data");
    }

    string? userid = parkingManager.RegisterUser(user);
    if (userid == null)
    {
        return Results.Conflict("user already exists");
    }
        
    return Results.Ok($"User {user.UserId} registered successfully");
});
app.MapGet("/get-user-details/{userid}", (string userid) =>
{
    User? user = parkingManager.GetUserDetails(userid);
    List<Period>? periods=parkingManager.GetAllPeriodsForUser(userid);
    if (user != null)
        return Results.Ok(new
        {
            Message=" get user’s all registered details ",
            userDetails=user,
            Periods=periods
        });
    return Results.NotFound("user not found");
});
app.MapGet("/begin-new-period/{userid}",(string userid) =>
{
    if (string.IsNullOrEmpty(userid)) 
    {
        return Results.BadRequest($"{userid} was not found");
    }
    int periodid=parkingManager.BeginNewPeriod(userid);
    return Results.Ok(new
    {
        Message= $"period{periodid} begin for user {userid}",
        PeriodId=periodid,
        startTime=DateTime.Now,
    });
});
app.MapGet("/get-present-period/{userid}", (string userid) =>
{
    Period? presentPeriod=parkingManager.GetPresentPeriod(userid);
    if (presentPeriod==null)
    {
        return Results.NotFound($"There is no current period for this user:{userid}");
    }
    var totalHuors=(DateTime.Now-presentPeriod.StartTime).TotalHours;
    var totalMinutes= (DateTime.Now - presentPeriod.StartTime).TotalMinutes;
    return Results.Ok(new
    {
        Message= $"the current Period for {userid} is:  {presentPeriod.PeriodId}," ,
        TotalHours=totalHuors,
        TotalMinutes=totalMinutes
    });
});
app.MapGet("/end-present-period/{userid}", (string userid) =>
{
    Period? endPeriod = parkingManager.EndPresentPeriod(userid);
    if (endPeriod == null)
    {
        return Results.NotFound($"There is no current period for this user:{userid}");
    }
    var totalHuors = (endPeriod.EndTime.Value - endPeriod.StartTime).TotalHours;
    var totalMinutes = (endPeriod.EndTime.Value - endPeriod.StartTime).TotalMinutes;
    return Results.Ok(new
    {
        Message = $"the current Period: {endPeriod.PeriodId} is ended for {userid} ,",
        Cost=endPeriod.PeriodCost,
        TotalHours = totalHuors,
        TotalMinutes = totalMinutes
    });
});
app.MapGet("/get-Total-cost-for-user/{userid}", (string userid) =>
{
    decimal cost = parkingManager.GetCostForUser(userid);
    if (cost == 0)
        return Results.NotFound($"User {userid} doesn't exist or no periods ended .");
    return Results.Ok($"the cost for user {userid} is {cost}");
});

app.MapGet("/", () => "Parking App Swagger Api!");

app.Run();
