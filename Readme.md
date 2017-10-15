# ChromeCast TV Service

## What does this do?
I only use my TV for Chromecasting, which is 99% great, but there are a few problems:
* Chromecast can turn on the TV when casting starts from any device, but doesn't expose a way to turn the TV off. After casting stops the TV just displays the [Backdrop](https://www.google.com/chromecast/backdrop/) forever, wasting electricity until I figure out where the TV remote is so I can turn it off
* Starting a new Casting session sometimes sets volume to 100%, blasting the speakers (and your eardrums).

This Service eliminates those problems:
* Uses the [Simple CEC API](https://github.com/Marcus-L/cecapi) to turn the TV off after it becomes idle
* Uses [GoogleCast](https://github.com/kakone/GoogleCast) to turn the volume down to 50% (configurable) if it notices that it's been set to the maximum (100%)

## Prerequisites

* Windows (for the CCTV.Service Version)
* .NET Core 2.0 Environment for the CCTV.Core.App version

## Installation/Usage

### NetcoreApp (OSX/Linux/Windows)

Run from the top-level directory:
```
dotnet run --project CCTV.Core.App
```

### Windows App/Service

To run from the command line, build then run:
```
cctv.service.exe
```

To install the service, build then run the command:

```
cctv.service.exe install
```

Start/stop the service with net:

```
net start CCTV
net stop CCTV
```

## Notes

This was interesting to do as both a Windows Service using [Topshelf](https://github.com/Topshelf/Topshelf) as a thin wrapper around a .NET Standard 2.0 library, and as a .NET Core 2.0 console app using the same code. I would have run it on the Raspberry Pi hooked up to the HDMI CEC itself but I'm using a super old ARMv6 Pi1 model B which is not supported.