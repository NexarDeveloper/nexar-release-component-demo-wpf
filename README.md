# Nexar.ReleaseComponent Demo

[nexar.com]: https://nexar.com/

Demo Altium 365 release component powered by Nexar.

**Projects:**

- `Nexar.ReleaseComponent` - WPF application for releasing components
- `Nexar.Client` - GraphQL StrawberryShake client

## Prerequisites

Visual Studio 2019.

You need your Altium Live credentials and have to be a member of at least one Altium 365 workspace.

In addition, you need a Nexar application at [nexar.com] with the Design scope.
Use this application client ID and secret and set the environment variables `NEXAR_CLIENT_ID` and `NEXAR_CLIENT_SECRET`.

## How to use

Open the solution in Visual Studio.
Ensure `Nexar.ReleaseComponent` is the startup project, build, and run.

If you run with the debugger then it may break due to "cannot read settings".
Ignore and continue (<kbd>F5</kbd>). The settings are stored on exiting.
Next runs should not have this issue.

The identity server sign in page appears. Enter your credentials and click `Sign In`.

The application window appears with the left tree panel populated with your workspaces.

Select a workspace to enable the "Release" button.
Click the "Release" button to show the component release dialog with various inputs.

Expand workspaces to component folders, expand folders to components, select a component.
The right panel shows the component information like description, comment, state, etc.

## Release dialog

The symbols and footprint data folders specify existing directories using absolute or relative paths.
Relative path are resolved with the folder of app executable as the root path.

Make sure the life cycle definition has Symbol, Footprint, and Component content types enabled.
Use Altium Designer in order to configure.

The "Reset" button resets fields to the default values with a new time stamp added.

The "Release" button calls the release component mutation and closes the dialog.

Check "Generate item names" to allow Symbol and Footprint item names generation instead of using user defined.

## Building blocks

The app is built using Windows Presentation Foundation, .NET Framework 4.7.2.

The data are provided by Nexar API: <https://api.nexar.com/graphql>.
This is the GraphQL endpoint and also the Banana Cake Pop GraphQL IDE in browsers.

The [HotChocolate StrawberryShake](https://github.com/ChilliCream/hotchocolate) package
is used for generating strongly typed C# client code for invoking GraphQL queries.
Note that StrawberryShake generated code must be compiled as netstandard.
That is why it is in the separate project `Nexar.Client` (netstandard).
The main project `Nexar.ReleaseComponent` (net472) references `Nexar.Client`.
