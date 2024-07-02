//Program.cs
using MudBlazor.Services;
using BlazorApp1_MudBlazor.Components;
using System.Net.WebSockets;
using RosbridgeNet.RosbridgeClient.Common;
using RosbridgeNet.RosbridgeClient.Common.Interfaces;
using RosbridgeNet.RosbridgeClient.ProtocolV2;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add RosbridgeNet services
builder.Services.AddScoped<IRosbridgeMessageDispatcher>(sp =>
{
    var webSocketUri = new Uri("ws://192.168.137.218:9090");
    var cts = new CancellationTokenSource();

    var socket = new Socket(new ClientWebSocket(), webSocketUri, cts);
    var messageSerializer = new RosbridgeMessageSerializer();
    var messageDispatcher = new RosbridgeMessageDispatcher(socket, messageSerializer);
    messageDispatcher.StartAsync().Wait();
    return messageDispatcher;
});

builder.Services.AddMudServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
