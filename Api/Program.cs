using Api.Auth;
using Api.Auth.Database;
using Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.ConfigureAuthServices(builder.Configuration);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using(var scope = app.Services.CreateScope()){ // TO DO + private endpoint gives 401 
    var authContext = scope.ServiceProvider.GetRequiredService<AuthContext>();
    authContext.Database.EnsureCreated();
}


app.UseHttpsRedirection();
app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.UseMiddleware<ErrorHandlerMiddleware>();

app.Run();
