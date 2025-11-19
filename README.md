# SpadTools

A collection of tools for working with Spad.neXt profiles.

## Status

- ProfileManager is currently in active development. Features may change and new features may be added frequently.
- ProfileCompiler is a new project aimed at allowing you to compile profiles from pre-existing components (ie profile inheritance).
- NewProfileManager is an attempt to port ProfileManager to Spectre.Console for a better user experience. Currently **very** not working.
- The Common folder contains shared code used by the tools.

## Features

- Compare Spad.neXt profiles
- Replace values between profiles
- Generate reports in CSV format

## Installation

1. Clone the repository
2. Run `dotnet build` to build the project
3. Run `dotnet run` to start the tool

## Usage

```bash
dotnet run --profile1 <path_to_profile1> --profile2 <path_to_profile2>
```
