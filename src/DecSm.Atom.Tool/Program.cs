// The entry point for the DecSm.Atom.Tool console application.
// This file sets up the command-line interface using ConsoleAppFramework,
// registers the main command model, and applies a custom argument filter.

var app = ConsoleApp.Create();
app.Add<CommandModel>();
app.UseFilter<RunArgsFilter>();
app.Run(args);
