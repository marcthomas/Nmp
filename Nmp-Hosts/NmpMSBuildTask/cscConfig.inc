/nowarn:1701,1702,0414
/errorreport:prompt
/warn:4
/define:DEBUG;TRACE


/reference:"C:\Program Files\Reference Assemblies\Microsoft\Framework\v3.5\Microsoft.Build.Framework.dll"
/reference:"C:\Program Files\Reference Assemblies\Microsoft\Framework\v3.5\Microsoft.Build.Utilities.v3.5.dll"

#
# note: most of our references come from csc.rsp which gets loaded automaticaly if /noconfig is NOT
# on the command line - csc.rsp is located in the same folder as csc.exe
#
# any additional references are listed below
#

#/reference:"..\MP.JSEval\Debug\MP.JSEval.dll"
/reference:"..\MP.Base\bin\Debug\MP.Base.dll"
/reference:"..\MP.Lib\bin\Debug\MP.Lib.dll"


/debug:full
/optimize-
/target:module
/out:bin\debug\MP.MSBuildTask.dll

#
# common files
#
/nowarn:2002

#MP.Lib

..\common\InputStreamHandler.cs


"Source\Task.cs"
"Source\Utility.cs"

#
