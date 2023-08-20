using UserTask.BLL.Interfaces;
using UserTask.BLL.Services;
using UserTask.DAL.Interfaces;
using UserTask.DAL.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<UserTaskDbConfigMongo>(builder.Configuration);
builder.Services.AddScoped<IUserTaskRepository, UserTaskRepositoryMongo>();
builder.Services.AddScoped<IUserTaskListService, UserTaskListService>();
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

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
