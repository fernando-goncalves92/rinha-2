using Api.Filters;
using Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration["ConnectionStrings:Postgres"];

builder.Services.AddNpgsqlDataSource(connectionString);
builder.Services.AddTransient<Uow>();

builder.Services.AddControllers(o =>
{
    o.Filters.Add(typeof(CancelledOperationExceptionFilter));
});

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}

var app = builder.Build();
if (builder.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();