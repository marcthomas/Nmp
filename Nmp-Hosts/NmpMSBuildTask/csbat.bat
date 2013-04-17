@echo off

:: :: echo command line is: %$
:: 
:: ::set sourcePath=%@path[%1]
:: ::pushd "%sourcePath"
:: 
:: ::
:: :: souce is in project root or one directory deeper so if we don't find
:: :: the config file in the directory where the source file is located we
:: :: need to back up one directory
:: ::
:: iff NOT ISFILE cscConfig.inc then
:: 	cd ..
:: endiff
:: 
:: :: cd
:: 
:: :: echo.
:: 
:: csc.exe @cscConfig.inc %1
:: 
:: iff ERRORLEVEL == 0 then
:: 
:: 	echo good build
:: 	echo ``
:: 	
:: endiff
:: 
:: popd
:: 
:: activate "codewright*"


set BuildTask=1

cd ..
csbat.bat %$
 
