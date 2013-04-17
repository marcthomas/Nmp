@echo off
cd ..
call csbat.bat %$

:: :: ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
:: ::
:: :: Current build configuration
:: ::
:: :: set Configuration=Release
:: 
:: 
:: :: ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
:: ::
:: iff "%Configuration" == "" then
:: 	set Configuration=Debug
:: endiff
:: 
:: 
:: :: ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
:: ::
:: iff "%Platform" == "" then
:: 	set Platform=AnyCPU
:: endiff
:: 
:: 
:: :: ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
:: ::
:: echo ``
:: echo csbat.bat in Nmp
:: echo ``
:: echo Configuration==%Configuration
:: echo Platform==%Platform
:: echo VSTool==%VSTool
:: echo BuildTask=%BuildTask
:: echo ``
:: 
:: ::set verbosity=minimal
:: set verbosity=normal
:: 
:: :: ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
:: ::
:: msbuild /clp:Verbosity=%verbosity "Nmp.csproj" /p:Configuration=%Configuration;Platform=%Platform
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
:: 
:: 
:: @echo off

