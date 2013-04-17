@echo off
cd ..
call csbat.bat %$

:: 
:: cd
:: 
:: :: ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
:: ::
:: set Configuration=Debug
:: set Platform=x86
:: set verbosity=normal
:: 
:: 
:: :: ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
:: ::
:: :: msbuild /clp:Verbosity=%verbosity "NmpHost.csproj" /p:Configuration=%Configuration;Platform=%Platform
:: 
:: 
::  broken, does not work because of conflicts between projects - x86 and AnyCPU
:: 
:: msbuild /clp:Verbosity=%verbosity "NmpHost.csproj"
:: iff "0" != "%ERRORLEVEL" then
:: 	goto errorExit
:: endiff
:: 
:: 
:: goto done
:: 
:: 
:: :: ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
:: ::
:: :errorExit
:: 
:: echo last command returned an error of %ERRORLEVEL
:: 
:: 
:: :: ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
:: ::
:: :done
:: activate "codewright*"


