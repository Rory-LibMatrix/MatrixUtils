using ArcaneLibs.Extensions;
using LibMatrix.Services;
using LibMatrix.Utilities.Bot;
using MatrixUtils.RoomUpgradeCLI;
using MatrixUtils.RoomUpgradeCLI.Commands;

foreach (var group in args.Split(";")) {
    var argGroup = group.ToArray();
    var builder = Host.CreateApplicationBuilder(args);
    builder.Services.AddRoryLibMatrixServices();
    builder.Services.AddMatrixBot();

    if (argGroup.Length == 0) {
        Console.WriteLine("Unknown command. Use 'new', 'modify', 'import-upgrade-state' or 'execute'.");
        Console.WriteLine("Hint: you can chain commands with a semicolon (;) argument.");
        return;
    }

    Console.WriteLine($"Running command: {string.Join(", ", argGroup)}");

    builder.Services.AddSingleton(new RuntimeContext() {
        Args = argGroup
    });

    if (argGroup[0] == "new") builder.Services.AddHostedService<NewFileCommand>();
    else if (argGroup[0] == "new-from-room-dir") builder.Services.AddHostedService<NewFromRoomDirCommand>();
    else if (argGroup[0] == "modify") builder.Services.AddHostedService<ModifyCommand>();
    else if (argGroup[0] == "import-upgrade-state") builder.Services.AddHostedService<ImportUpgradeStateCommand>();
    else if (argGroup[0] == "execute") builder.Services.AddHostedService<ExecuteCommand>();
    // dev cmds
    else if (argGroup[0] == "dev-delete-room") builder.Services.AddHostedService<DevDeleteRoomCommand>();
    else if (argGroup[0] == "dev-delete-all-rooms") builder.Services.AddHostedService<DevDeleteAllRoomsCommand>();
    else if (argGroup[0] == "dev-get-room-dir-state") builder.Services.AddHostedService<DevGetRoomDirStateCommand>();
    else {
        Console.WriteLine("Unknown command. Use 'new', 'modify', 'import-upgrade-state' or 'execute'.");
        return;
    }

    var host = builder.Build();
    host.Run();
}