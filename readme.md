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

**Documentation:** [aziascreations.github.io/DotNet-Win32-Mailslot/](https://aziascreations.github.io/DotNet-Win32-Mailslot/)


## Features
* Simple and complete feature set
  * FileStream IO *(Async/Sync)*
    * With and without handle ownership
    * Thread-safety not included<i>*</i>
  * Simplified IO *(Sync)*
    * No handle ownership worries
    * Auto encoding
    * Thread-safe
  * Utilities for UNC paths
* Easy to use, lightweight and 'to-the-point' philosophy
  * No unnecessary types, classes, procedures and whatnot
  * No dependencies attached
* Supports modern developer QoL
  * ~~Fully compatible with [Microsoft.DotNet.ILCompiler](https://www.nuget.org/packages/Microsoft.DotNet.ILCompiler/)~~
  * Nullable annotations
  * Fully documented

<i>*: Must be handled in your app via a `lock`.</i>


## Requirements
* Operating System
  * Windows 8 or newer
  * Windows Server 2012 or newer
* .NET Runtime
  * .NET Framework 3.5, 4.0 or newer
  * .NET Core 8.0 or newer

<br>

---

## Remarks

### Handle Ownership
Both `MailslotClient` and `MailslotServer` are themselves `FileStream`s: there's no
 separate handle to own, duplicate or hand out. \
Pass the instance around directly to wherever it needs to be read from or written to,
 and dispose of it whenever you're done with it, same as any other `FileStream`.

You can use the `CreateAsFileStream` static class functions to get a `FileStream`
 directly, without keeping the client/server instance around yourself.

### Thread Safety
...


<br>

---

## Basic Example
This section contains examples that should be enough to get you started.

For more detailed examples, check the [???] page/section.

### Client

#### Simplified API *(Sync)*
```csharp
// Sending to `\\.\mailslot\test123`
var client = new MailslotClient(null, "test123", isAsync: false)
client.SendSync(mailslotText);
```

#### FileStream API *(Async+Sync)*
```csharp
bool isAsync = true;

// This FileStream own the Win32 API handle
var fs = MailslotClient.CreateAsFileStream(
    "\\\\.\\mailslot\\test123", isAsync: isAsync
);

byte[] data = Encoding.UTF8.GetBytes("Hello World !");

if(isAsync) {
    await fs.WriteAsync(data);
} else {
    fs.Write(data, 0, data.Length);
}

// May need to be moved around.
fs.Flush(true);
```


### Server

#### Simplified API
```csharp
// TOOD: Implement this
```

#### FileStream API *(Blocking)*
```csharp
string mailslotPath = Guid.NewGuid().ToString();
uint mailslotSize = 1024;
byte[] buffer = new byte[mailslotSize];

// Server that waits forever
Console.WriteLine($"Starting server on `\\\\.\\mailslot\\{mailslotPath}`");
var server = new MailslotServer(
    null, mailslotPath, mailslotSize,
    MailslotServer.MAILSLOT_WAIT_FOREVER
);

Console.WriteLine($"Waiting for data...");
int bytesRead = server.Read(buffer, 0, buffer.Length);

if (bytesRead > 0) {
    Console.WriteLine($"Received: `{Encoding.ASCII.GetString(buffer)}`");
} else {
    Console.Error.WriteLine("Couldn't read from the mailslot !");
    return 1;
}
```

#### FileStream API *(Polling)*
```csharp
// TOOD: Create this example
```

#### Manual checking *(Not recommended)*
```csharp
// TOOD: Create this example
```


<br>

---

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
