using LibMatrix.Services;
using LibMatrix.Utilities.Bot;
using MatrixUtils.RoomUpgradeCLI;
using MatrixUtils.RoomUpgradeCLI.Commands;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddRoryLibMatrixServices();
builder.Services.AddMatrixBot();

if (args.Length == 0) {
    Console.WriteLine("No command provided. Use 'new', 'new-upgrade', or 'import-upgrade'.");
    return;
}

builder.Services.AddSingleton<RuntimeContext>(new RuntimeContext() {
    Args = args
});

if (args[0] == "new")
    builder.Services.AddHostedService<NewFileCommand>();
else if (args[0] == "import-upgrade") { }
else {
    Console.WriteLine("Unknown command. Use 'new', 'new-upgrade', or 'import-upgrade'.");
    return;
}

var host = builder.Build();
host.Run();