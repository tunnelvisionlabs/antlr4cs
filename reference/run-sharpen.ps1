param (
	[string]$Java6Home,
	[string]$MavenHome,
	[string]$MavenRepo = "$($env:USERPROFILE)\.m2"
)

$Env:M2_HOME=$MavenHome

$Java7RegKey = 'HKLM:\SOFTWARE\JavaSoft\Java Runtime Environment\1.7'
$Java7RegValue = 'JavaHome'
If (-not $Java7Home -and (Test-Path $Java7RegKey)) {
	$JavaHomeKey = Get-Item -LiteralPath $Java7RegKey
	If ($JavaHomeKey.GetValue($Java7RegValue, $null) -ne $null) {
		$JavaHomeProperty = Get-ItemProperty $Java7RegKey $Java7RegValue
		$Java7Home = $JavaHomeProperty.$Java7RegValue
	}
}

# Build Sharpen using Maven
$OriginalPath = $PWD

cd 'sharpen'
$MavenPath = "$MavenHome\bin\mvn.cmd"
If (-not (Test-Path $MavenPath)) {
	$MavenPath = "$MavenHome\bin\mvn.bat"
}

If (-not (Test-Path $MavenPath)) {
	$host.ui.WriteErrorLine("Couldn't locate Maven binary: $MavenPath")
	cd $OriginalPath
	exit 1
}

If (-not $Java7Home -or -not (Test-Path $Java7Home)) {
	$host.ui.WriteErrorLine("Couldn't locate Java 7 installation: $Java7Home")
	cd $OriginalPath
	exit 1
}

$MavenGoal = 'package'
&$MavenPath '-DskipTests=true' $MavenGoal
if (-not $?) {
	$host.ui.WriteErrorLine('Maven build of the Sharpen failed, aborting!')
	cd $OriginalPath
	Exit $LASTEXITCODE
}

cd $OriginalPath

# Convert the C# source code using Sharpen
&"$Java7Home\bin\java" -jar sharpen\src\target\sharpencore-0.0.1-SNAPSHOT-jar-with-dependencies.jar "$PWD/antlr4" -srcFolder antlr4/runtime/Java/src -cp antlr4/runtime/JavaAnnotations/src -header sharpen-header.txt @sharpen-all-options.txt
