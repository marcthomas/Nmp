readme.txt
==========

This project copies files on the developers machine, it is unlikely anyone else will place the Nmp runtime files in exactly the same location so the TCC .btm file will terminate if it does not see the environment variable "NmpBuildMachine".


This project exists only to processed AFTER all other projects have run AND is dependent upon ALL of them. The <Import project="..."/> for Microsoft.CSharp.Targets has been commented out so this project IS NO GOOD for building anything.

Note: copy-files-local.btm is called by MSBuild as part of the "Build" target WHICH WE HAVE CREATED IN AfterBuild.csproj

Note: the build events exposed in Visual Studio are NOT USED (will not execute!!)

