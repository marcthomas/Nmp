@echo off
set sourcePath=%@path[%1]

pushd ..\

:: call csbat.bat %$

:: ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
::
set verbosity=normal
set Configuration=Debug
set Platform=AnyCPU


:: ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
::
::msbuild /clp:Verbosity=%verbosity "Nmp.csproj" /p:Configuration=%Configuration;Platform=%Platform
::msbuild /clp:Verbosity=%verbosity Nmp2.sln /t:Build /p:Configuration=Debug

msbuild /clp:Verbosity=%verbosity "NmpTests.csproj" /p:Configuration=%Configuration;Platform=%Platform


iff "0" != "%ERRORLEVEL" then
 goto errorExit
endiff


:: ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
::
:errorExit

echo last command returned an error of %ERRORLEVEL


:: ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
::
:done
::: activate "codewright*"

