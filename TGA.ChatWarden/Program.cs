using TGA.ChatWarden;
using TGA.CoreLib.Bot.UpdatesProcessing.Interfaces;
using TGA.CoreLib.DI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBotComponents();
builder.Services.AddSingleton<IUpdatesProcessor, UpdatesProcessor>();
var app = builder.Build();

app.Run();
