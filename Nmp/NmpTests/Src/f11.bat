@echo off
::
:: remember we're in the directory where f11.bat lives
:: 
::echo "%_cwd"
::pause

::
::xunit.console.clr4 "%_cwd\..\bin\Debug\NmpTests.dll" /silent /html "%_cwd\test.html"

xunit.console.clr4 "%_cwd\..\bin\Debug\NmpTests.dll" /html "%_cwd\test.html"

::xunit.console.clr4 "%_cwd\..\bin\Debug\NmpTests.dll"
::pause

start /b iexplore "%_cwd\test.html"

:: pause
:: activate "xUnit.net*" topmost
:: pause
