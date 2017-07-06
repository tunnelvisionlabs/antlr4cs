# Building from Source

## Prerequisites

* Visual Studio 2015 (Community edition or higher)
* Maven 3.3.3 or higher
* Java 6 Runtime Environment (JRE)
* Java 7 Development Kit
* [Microsoft .NET Compact Framework 3.5](https://download.microsoft.com/download/c/b/e/cbe1c611-7f2f-4bcf-921d-2df718591e1e/NETCFSetupv35.msi)
* [Power Toys for the Microsoft .NET Compact Framework 3.5](https://download.microsoft.com/download/f/a/c/fac1342d-044d-4d88-ae97-d278ef697064/NETCFv35PowerToys.msi)

## Build environment

| Environment Variable | Description | Example |
| --- | --- | --- |
| `M2_HOME` | Path to Maven | `J:\apps\apache-maven-3.3.3` |

## Build script

After the prerequisites are installed, the build is executed with the following commands:

```powershell
cd build
.\build.ps1 -Verbosity minimal -GenerateTests
cd ..
```

### Running tests

Tests may be run using the commands seen in [appveyor.yml](appveyor.yml).
