If (-not (Test-Path 'NETCFSetupv35.msi')) {
	Invoke-WebRequest https://download.microsoft.com/download/c/b/e/cbe1c611-7f2f-4bcf-921d-2df718591e1e/NETCFSetupv35.msi -OutFile NETCFSetupv35.msi
}

If (-not (Test-Path 'NETCFv35PowerToys.msi')) {
	Invoke-WebRequest https://download.microsoft.com/download/f/a/c/fac1342d-044d-4d88-ae97-d278ef697064/NETCFv35PowerToys.msi -OutFile NETCFv35PowerToys.msi
}

msiexec.exe /i NETCFSetupv35.msi /qn | Out-Null
msiexec.exe /i NETCFv35PowerToys.msi /qn | Out-Null
cinst maven -version 3.3.1
