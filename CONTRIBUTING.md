# Building from Source

## Prerequisites

* Visual Studio 2015 (Community edition or higher)
* Maven 3.3.3 or higher
* Java 6 Runtime Environment (JRE)
* Java 7 Development Kit

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
