# HardwareInformation

.NET 5 Cross-Platform Hardware Information Gatherer
================

![GitHub release (latest by date)](https://img.shields.io/github/v/release/L3tum/HardwareInformation?style=flat-square)
![Nuget](https://img.shields.io/nuget/v/HardwareInformation?style=flat-square)

![Build](https://github.com/L3tum/HardwareInformation/workflows/.NET%20Core%20CI/badge.svg?style=flat-square)
![Simple Test](https://github.com/L3tum/HardwareInformation/workflows/.NET%20Core%20Simple%20Test/badge.svg?style=flat-square)

- [Features](#features)
- [Goal](#goal)

## Usage

Download from [Nuget](https://www.nuget.org/packages/HardwareInformation/) via your favorite Nuget client like dotnet

`dotnet add package HardwareInformation`

Starting with version 2.1.20 HardwareInformation is also available via Github Package Registry!

Get hardware information from the gatherer

`MachinInformation info = MachineInformationGatherer.GatherInformation(bool skipClockspeedTest = true)`

Result is cached internally so don't worry about calling it multiple times

## Overall Features

- No Kernel Driver, Module or Administrator/Elevated Rights needed
- Supports Windows, Linux and OSX
- Pretty fast :)
- Supports multiple CPUs (i.e. multiple Sockets) and GPUs
- Supports BIG.little as far as Intel and Apple provided it
- Supports ARM, x86 and AMD64

A detailed list of information gathered by this library can be found in [here](./docs/SupportedInformation.md), although the OS-specific remarks may not always be accurate.

## Goal

The immediate goal is somewhat feature-parity with CPU-Z/CPUID. While this may be impossible for some features (like RAM
latencies) without a kernel driver, part of the goal is also to bring these kind of capabilities without requiring a
software installation or kernel driver.

In some parts, this library has already more features than CPU-Z/CPUID, since it's cross-platform (for most features).

TODOs are tracked in the feature table. Crosses basically mean TODO.
