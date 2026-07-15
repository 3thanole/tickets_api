using System.Text.Json.Serialization;
using FluentValidation;
using TicketManagementApi.BackgroundServices;
using TicketManagementApi.ExceptionHandling;
using TicketManagementApi.Filters;
using TicketManagementApi.Repositories;
using TicketManagementApi.Services;
using TicketManagementApi.Validators;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers(options => options.Filters.Add<ValidationFilter>())
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddValidatorsFromAssemblyContaining<CreateTicketRequestValidator>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<ITicketRepository, InMemoryTicketRepository>();
builder.Services.AddSingleton<ITicketService, TicketService>();
builder.Services.AddHostedService<TicketCleanupService>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();
