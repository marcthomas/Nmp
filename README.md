Readme.txt
==========


Nmp (Net Macro Processor) is a text macro processor that runs under .Net 4.0, Mono (2.10.9), and will compile without changes on .Net 4.5.

You can use Nmp in one of four ways:

* From the command line using the NmpCommandLineHost "nmp.exe".
* Using Visual Studio 2012 and the NmpCustomTool
* Using MSBuild with the NmpMSBuildTask
* As a library from your own project using the NmpBaseEvaluator class in Nmp.dll.

Before we go any futher here's an example using a macro file from the command line:
````
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;;
;; here are three macros
;;

#.define( `treasure', `gold coins!')
#.define( `#player', `Tracy')

(#defmacro `#whosePlaying', `who')
$who is playing for treasure-
(#endmacro)
````
This invokes the macros
````
"#whosePlaying(#player)"
````
And this is the result:
````
"Tracy is playing for gold coins!"
````
If this looks intersting or at all usefull continue reading; otherwise, this is probably not a tool for you

### Installing

* You can install Nmp by downloading the 4.5 binaries[here](http://sdrv.ms/12z7QC8)
* Clone the dev branch on windows at [GitHub](https://github.com/jmclain/Nmp)
* Or download the zipped source on [GitHub](https://github.com/jmclain/Nmp)

### Running from the command line

With the Nmp directory in your path running nmp.exe is as simple as:
````
nmp nmpScriptFile.txt.nmp
````
The file "nmpScriptFile.txt" will be output by Nmp because the command line host defaults to naming the output file the same as the input file with the trailing file extension removed. You can add a file extension from within a script by using the #.setOutputExtension(.ext) macro.

### Visual Studio Custom Tool:
You will need the command line versions of Nmp (nmp.exe) in your path - if you have everything installed in the same diretory (as would be the case if you download the pre-built binaries in the zip file) you're good to go. In the install directory you need to run the macro script file "NmpRegister.reg.nmp" through nmp.exe.
````
  nmp NmpRegister.reg.nmp
````
The file "NmpRegister.reg" should be generated. If you look at the file you'll see its a reg file with all the registry entries for adding a custom tool to Visual Studio. It should automatically figure out if you're running on a 32 or 64 bit system (important for registery locations); it defaults to Visual Studio 2012 (version 11.0), the only version the .Net 4.5 version of Nmp will run under. The location of the DLL's (Codebase) is the directory in which the reg file lives.

One more thing, you need to run Visual Studio with the /setup switch.
````
  devenv /setup
````
The command will probably come back with the error "The operation could not be completed. Access is denied." Don't worry about it, in my experence VS will have updated the current users setting to reflect that Nmp has been installed.

Warning: if you intend on tinkering with Nmp and rebuilding make sure you've copied the binaries you want to use in Visual Studio off to a location that won't automatically be updated. If you don't do this you'll have a mess because you will be updating files in a directory where VS is currently accessing them - some will copy, some won't - a mess.

To use the Nmp custom tool create a new text file in VS and name it with the ".nmp" file extension. Each time you save the file it should be interpreted by Nmp and the output placed in the VS file tree beneath the source file.

### MSBuild task

To do.

### From your own code

To do.

## License

[Eclipse Public License](license.txt)
