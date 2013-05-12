Readme.txt
==========


Nmp (Net Macro Processor) is a text templating engine that runs under .Net 4.0 and Mono (2.10.9).

You can use Nmp in one of four ways:

* From the **command line** using the NmpCommandLineHost _nmp.exe_
* Using **Visual Studio 2012** and the _NmpCustomTool.dll_
* Using **MSBuild** with the _NmpMSBuildTask.dll_
* As a **library** from your own project using the _NmpBaseEvaluator_ class in _Nmp.dll_

Before we go any farther here's a simple _**non programming**_ example:
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
This next line invokes the macros

      "#whosePlaying(#player)"

And this is the result:

      "Tracy is playing for gold coins!"

If that looked at all interesting take a look at this next bit that invokes a macro name `#Property':

      #Property( `AssemblyPath', `string', `string.Empty')

Which generates:

````
  /////////////////////////////////////////////////////////////////////////////
  //
  // the property "AssemblyPath" has been auto generated, any changes you make to this
  // code will be lost next time the file is regenerated
  //
  /////////////////////////////////////////////////////////////////////////////
  
  partial void onAssemblyPathChanging( string newValue, string oldValue );
  
  string _assemblyPath = string.Empty;
  
  public string AssemblyPath
  {
  	get {
  		return _assemblyPath;
  	}
  	set {
  		onAssemblyPathChanging( value, _assemblyPath );
  		_assemblyPath = value;
  		NotifyOfPropertyChanged( "AssemblyPath");
  	}
  }
````

You can see the `#Property' macro itself <a href="#propMacro"> here </a>


### Installing

You can install Nmp by downloading a Zip file [here](http://sdrv.ms/12z7QC8); as of this writing the current version of Nmp is 3.0.6, so the file will be named "Nmp3.0.6.zip". Read the section below titled "Visual Studio Custom Tool" to see how to make Nmp run with VS 2012.

### Running from the command line

With the Nmp directory in your path running nmp.exe is as simple as:

      nmp nmpScriptFile.txt.nmp

The file "nmpScriptFile.txt" will be output by Nmp because the command line host defaults to naming the output file the same as the input file with the trailing file extension removed. You can add a file extension from within a script by using the #.setOutputExtension(.ext) macro.

### Visual Studio Custom Tool:
You will need the command line versions of Nmp (nmp.exe) in your path - if you have everything installed in the same directory (as would be the case if you download the pre-built binaries in the zip file) you're good to go. In the install directory you need to run the macro script file "NmpRegister.reg.nmp" through nmp.exe.

      nmp NmpRegister.reg.nmp

The file "NmpRegister.reg" should be generated. If you look at the file you'll see its a reg file with all the registry entries for adding a custom tool to Visual Studio. It should automatically figure out if you're running on a 32 or 64 bit system (important for registery locations); it defaults to Visual Studio 2012 (version 11.0). The location of the DLL's (Codebase) is the directory in which the reg file lives. To register:

      regedit NmpRegister.reg

One more thing, you need to run Visual Studio with the /setup switch.

      devenv /setup

The command will probably come back with the error "The operation could not be completed. Access is denied." Don't worry about it, in my experence VS will have updated the current users setting to reflect that Nmp has been installed.

To use the Nmp custom tool create a new text file in VS and name it with the ".nmp" file extension. Each time you save the file it should be interpreted by Nmp and the output placed in the VS file tree beneath the source file.



### Source

You can view, compile and play with the source by cloning the 'dev' branch from [GitHub](https://github.com/jmclain/Nmp). If you don't want to mess with git you can download the source via zip file at the same location.

To compile the source you **must** have **nuget.exe** in your path. Before you compile Nmp with VS 2012 you must go to the "packages" directory (off the Nmp root directory) and execute the "**get-packages.bat**" file to download the "xunit" and "xunit.extension" nuget packages.

Once compiled you will find various combinations of files in the "Current" directory that will located off the the Nmp root directory.

### MSBuild task

To do.

### From your own code

To do.

* * *
<a name="propMacro"/>
### #Property Macro
````
  ;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
  ;;
  
  #.define( `_#propBackingName', `_`'#char.ToLower( $[]0[0])`'$[]0.Substring(1)')
  
  
  ;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
  ;;
  
  (#defmacro `#Property', `name', `type', `value')
  
  		/////////////////////////////////////////////////////////////////////////////
  		//
  		// the property "$name" has been auto generated, any changes you make to this
  		// code will be lost next time the file is regenerated
  		//
  		/////////////////////////////////////////////////////////////////////////////
  
  		partial void on`'$name`'Changing( $type newValue, $type oldValue );
  
  		$type _#propBackingName($name)`'#if.NotEmpty($value,` = $value',`');
  
  		public $type $name
  		{
  			get {
  				return _#propBackingName($name);
  			}
  			set {
  				on`'$name`'Changing( value, _#propBackingName($name) );
  				_#propBackingName($name) = value;
  				NotifyOfPropertyChanged( "$name");
  			}
  		}
  (#endmacro)
````
* * *

## License

[Eclipse Public License](license.txt)
