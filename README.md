# SpadTools

A collection of tools for working with Spad.neXt profiles.

## Projects

### ProfileManager 

Allows you to scan and manipulate Spad.neXt profiles:

- Compare two profiles and see the differences across devices and events
- Copy values between profiles (events or whole devices)

Currently in active development. Features may change and new features may be added frequently.

Docs will be available in the project folder soon.

### ProfileCompiler

Allows you to compile Spad.neXt profiles from pre-existing components, enabling profile inheritance.

This tool is literally just getting started and there's basically no code yet. Track the commits to see progress.

### NewProfileManager

An attempt to port ProfileManager to use Spectre.Console for a better developer experience. 

DOES NOT WORK AT ALL YET.

## Installation

1. Clone the repository
2. Run `dotnet build` to build the project
3. Run `dotnet run` to start the tool

# Requires

- .NET 10.0 SDK or later - a Visual Studio solution is provided.
- Spad.neXt (if you want to do anything useful with it)