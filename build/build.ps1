param (
	[switch]$Debug,
	[string]$VisualStudioVersion = "14.0",
	[switch]$NoClean,
	[string]$Verbosity = "normal",
	[string]$Logger,
	[string]$Java6Home,
	[string]$MavenHome,
	[string]$MavenRepo = "$($env:USERPROFILE)\.m2",
	[switch]$SkipMaven,
	[switch]$SkipKeyCheck
)

# build the solutions
$SolutionPath = "..\Runtime\CSharp\Antlr4.sln"
$CF35SolutionPath = "..\Runtime\CSharp\Antlr4.VS2008.sln"

# make sure the script was run from the expected path
if (!(Test-Path $SolutionPath)) {
	echo "The script was run from an invalid working directory."
	exit 1
}

. .\version.ps1

If ($Debug) {
	$BuildConfig = 'Debug'
} Else {
	$BuildConfig = 'Release'
}

If ($AntlrVersion.Contains('-')) {
	$KeyConfiguration = 'Dev'
} Else {
	$KeyConfiguration = 'Final'
}

If ($NoClean) {
	$Target = 'build'
} Else {
	$Target = 'rebuild'
}

If (-not $MavenHome) {
	$MavenHome = $env:M2_HOME
}

$Java6RegKey = 'HKLM:\SOFTWARE\JavaSoft\Java Runtime Environment\1.6'
$Java6RegValue = 'JavaHome'
If (-not $Java6Home -and (Test-Path $Java6RegKey)) {
	$JavaHomeKey = Get-Item -LiteralPath $Java6RegKey
	If ($JavaHomeKey.GetValue($Java6RegValue, $null) -ne $null) {
		$JavaHomeProperty = Get-ItemProperty $Java6RegKey $Java6RegValue
		$Java6Home = $JavaHomeProperty.$Java6RegValue
	}
}

# this is configured here for path checking, but also in the .props and .targets files
[xml]$pom = Get-Content "..\tool\pom.xml"
$CSharpToolVersionNodeInfo = Select-Xml "/mvn:project/mvn:version" -Namespace @{mvn='http://maven.apache.org/POM/4.0.0'} $pom
$CSharpToolVersion = $CSharpToolVersionNodeInfo.Node.InnerText.trim()

$nuget = '..\runtime\CSharp\.nuget\NuGet.exe'

# build the main project
if ($VisualStudioVersion -eq '4.0') {
	$msbuild = "$env:windir\Microsoft.NET\Framework64\v4.0.30319\msbuild.exe"
} Else {
	$msbuild = "${env:ProgramFiles(x86)}\MSBuild\$VisualStudioVersion\Bin\MSBuild.exe"
}

If ($Logger) {
	$LoggerArgument = "/logger:$Logger"
}

&$nuget 'restore' $SolutionPath
&$msbuild '/nologo' '/m' '/nr:false' "/t:$Target" $LoggerArgument "/verbosity:$Verbosity" "/p:Configuration=$BuildConfig" "/p:VisualStudioVersion=$VisualStudioVersion" "/p:KeyConfiguration=$KeyConfiguration" $SolutionPath
if (-not $?) {
	$host.ui.WriteErrorLine('Build failed, aborting!')
	Exit $LASTEXITCODE
}

# build the compact framework project
$msbuild = "$env:windir\Microsoft.NET\Framework\v4.0.30319\msbuild.exe"

&$nuget 'restore' $CF35SolutionPath
&$msbuild '/nologo' '/m' '/nr:false' '/t:rebuild' $LoggerArgument "/verbosity:$Verbosity" "/p:Configuration=$BuildConfig" "/p:KeyConfiguration=$KeyConfiguration" $CF35SolutionPath
if (-not $?) {
	$host.ui.WriteErrorLine('.NET 3.5 Compact Framework Build failed, aborting!')
	Exit $LASTEXITCODE
}

if (-not (Test-Path 'nuget')) {
	mkdir "nuget"
}

# Build the Java library using Maven
If (-not $SkipMaven) {
	$OriginalPath = $PWD

	cd '..\tool'
	$MavenPath = "$MavenHome\bin\mvn.cmd"
	If (-not (Test-Path $MavenPath)) {
		$MavenPath = "$MavenHome\bin\mvn.bat"
	}

	If (-not (Test-Path $MavenPath)) {
		$host.ui.WriteErrorLine("Couldn't locate Maven binary: $MavenPath")
		cd $OriginalPath
		exit 1
	}

	If (-not $Java6Home -or -not (Test-Path $Java6Home)) {
		$host.ui.WriteErrorLine("Couldn't locate Java 6 installation: $Java6Home")
		cd $OriginalPath
		exit 1
	}

	$MavenGoal = 'package'
	&$MavenPath '-B' '-DskipTests=true' '--errors' '-e' '-Dgpg.useagent=true' "-Djava6.home=$Java6Home" '-Psonatype-oss-release' $MavenGoal
	if (-not $?) {
		$host.ui.WriteErrorLine('Maven build of the C# Target custom Tool failed, aborting!')
		cd $OriginalPath
		Exit $LASTEXITCODE
	}

	cd $OriginalPath
}

$JarPath = "..\tool\target\antlr4-csharp-$CSharpToolVersion-complete.jar"
if (!(Test-Path $JarPath)) {
	$host.ui.WriteErrorLine("Couldn't locate the complete jar used for building C# parsers: $JarPath")
	exit 1
}

# By default, do not create a NuGet package unless the expected strong name key files were used
if (-not $SkipKeyCheck) {
	. .\keys.ps1

	foreach ($pair in $Keys.GetEnumerator()) {
		$assembly = Resolve-FullPath -Path "..\runtime\CSharp\Antlr4.Runtime\bin\$($pair.Key)\$BuildConfig\Antlr4.Runtime.dll"
		# Run the actual check in a separate process or the current process will keep the assembly file locked
		powershell -Command ".\check-key.ps1 -Assembly '$assembly' -ExpectedKey '$($pair.Value)' -Build '$($pair.Key)'"
		if (-not $?) {
			Exit $LASTEXITCODE
		}
	}
}

$packages = @(
	'Antlr4.Runtime'
	'Antlr4'
	'Antlr4.VS2008')

ForEach ($package in $packages) {
	If (-not (Test-Path ".\$package.nuspec")) {
		$host.ui.WriteErrorLine("Couldn't locate NuGet package specification: $package")
		exit 1
	}

	&$nuget 'pack' ".\$package.nuspec" '-OutputDirectory' 'nuget' '-Prop' "Configuration=$BuildConfig" '-Version' "$AntlrVersion" '-Prop' "M2_REPO=$M2_REPO" '-Prop' "CSharpToolVersion=$CSharpToolVersion" '-Symbols'
	if (-not $?) {
		Exit $LASTEXITCODE
	}
}
