using Microsoft.EntityFrameworkCore;
using PersonelLeaveManagement.Infrastructure.Persistence;
using PersonelLeaveManagement.Infrastructure.Services;
using PersonelLeaveManagement.Application.Validators;
using PersonelLeaveManagement.Infrastructure.Repositories;
using PersonelLeaveManagement.Application.Interfaces;
using FluentValidation.AspNetCore;
using PersonelLeaveManagement.Api.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<PersonelValidator>());


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
options.UseSqlServer(connectionString));

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IPersonelService, PersonelService>();
builder.Services.AddScoped<IIzinTalebiService, IzinTalebiService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMiddleware<ExceptionMiddleware>();

app.UseAuthentication();

app.MapControllers();

app.Run();
