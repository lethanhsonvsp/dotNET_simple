using BlazorApp1_MudBlazor.Components;
using MudBlazor.Services;
using RosbridgeNet.RosbridgeClient.Common.Interfaces;
using RosbridgeNet.RosbridgeClient.Common;
using RosbridgeNet.RosbridgeClient.ProtocolV2;
using System.Net.WebSockets;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMudServices();// Add RosbridgeNet services
builder.Services.AddScoped<IRosbridgeMessageDispatcher>(sp =>
{
    var webSocketUri = new Uri("ws://192.168.137.109:9090");
    var cts = new CancellationTokenSource();

    var socket = new Socket(new ClientWebSocket(), webSocketUri, cts);
    var messageSerializer = new RosbridgeMessageSerializer();
    var messageDispatcher = new RosbridgeMessageDispatcher(socket, messageSerializer);
    messageDispatcher.StartAsync().Wait();
    return messageDispatcher;
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
