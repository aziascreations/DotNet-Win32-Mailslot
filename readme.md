# .NET - Win32 Mailslot

[![Nuget.org latest version](https://img.shields.io/nuget/v/NibblePoker.Win32.Mailslot?label=Latest%20version)](https://www.nuget.org/packages/NibblePoker.Win32.Mailslot)
[![Nuget.org downloads count](https://img.shields.io/nuget/dt/NibblePoker.Win32.Mailslot?label=Downloads)](https://www.nuget.org/packages/NibblePoker.Win32.Mailslot)
[![Repository's License](https://img.shields.io/github/license/aziascreations/DotNet-Win32-Mailslot)](https://github.com/aziascreations/DotNet-Win32-Mailslot/blob/master/LICENSE)

A simple library that exposes classes to interact with mailslots in a safe and .NET-friendly way.

<!--
> [!NOTE]
> This library is in low maintenance mode. <br/>
> I consider it feature-complete, and unless a bug is found, I don't plan on updating it.
-->

## Features
* ???
  * [FileStream]
  * [SimplifiedApi]
* Easy to use, lightweight and 'to-the-point' philosophy
  * No unnecessary types, classes, procedures and whatnot
  * No dependencies attached
* Supports modern developer QoL
  * ~~Fully compatible with [Microsoft.DotNet.ILCompiler](https://www.nuget.org/packages/Microsoft.DotNet.ILCompiler/)~~
  * Nullable annotations
  * Fully documented


## Requirements
* Operating System
  * Windows 8 or newer
  * Windows Server 2012 or newer
* .NET Runtime
  * .NET Framework 3.5, 4.0 or newer
  * .NET Core 8.0 or newer


## Documentation
Go to [aziascreations.github.io/DotNet-Win32-Mailslot/](https://aziascreations.github.io/DotNet-Win32-Mailslot/) for the HTML
documentation.


## Basic Example

### Client

#### Simplified API
```csharp
try {
	var mc = new MailslotClient("\\\\.\\mailslot\\test");
	mc.Send("Hello world", Encoding.ASCII);
} catch(Exception e) {
	Console.Error.WriteLine(e.Message);
}
```

#### FileStream API
```csharp
try {
	var fs = MailslotClient.CreateAsFileStream("\\\\.\\mailslot\\test");
	
	byte[] bytes = Encoding.ASCII.GetBytes("Hello world");
	
	client.Write(bytes, 0, bytes.Length);
	client.Flush();
} catch(Exception e) {
	Console.Error.WriteLine(e.Message);
}
```

### Server

#### Simplified API
```csharp
// TODO
```

#### FileStream API
```csharp
// TODO
```


## Cloning
Use this command to clone the repository and its submodules:
```
git clone --recurse-submodules https://github.com/aziascreations/DotNet-Win32-Mailslot.git
```

If you forgot the submodules, use this command:
```shell
git submodule update --init --recursive
```


## Building
1. Clone the repository
2. Restore the project: \
   `dotnet restore`
3. Build the project: \
   `dotnet build`
4. Package the project: \
   `dotnet pack`
5. Check the [NibblePoker.Win32.Mailslot/bin/Release](NibblePoker.Win32.Mailslot/bin/Release) folder


## License
The code in this repository is licensed under
[CC0 1.0 Universal (CC0 1.0) (Public Domain)](https://github.com/aziascreations/DotNet-Win32-Mailslot/blob/master/LICENSE).

The [doxygen-awesome-css](https://github.com/jothepro/doxygen-awesome-css) repository is used as a
submodule for Doxygen and is licensed under
the [MIT license](https://github.com/jothepro/doxygen-awesome-css/blob/main/LICENSE).
