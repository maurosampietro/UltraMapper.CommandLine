# UltraMapper.CommandLine

THIS DOCUMENTATION IS A WORK IN PROGRESS. BE PATIENT [2020-NOV-24].

Parse commandline args and map values to primitive and non-primitive user-defined types (eg: your classes, here after called 'complex types').
Set properties and invoke methods automatically, passing as arguments any number of parameters. 
IEnumerable, List and arrays are supported

UltraMapper.CommandLine will drastically simplify your code: just define the operations you want to be available in the 'commands' class.

Example:
    
    //--opendir "C:\my folder"

    static void Main( string[] args )
    {
        CommandLine.Instance.Parse<Commands>( args );
    }
    
    public class Commands
    {
        public void OpenDir( string path )
        {
            Process.Start( path );
        }
    }
    
You can directly set properties and call methods taking as arguments as many parameters as you want, 
built-in types (eg: bool, int, double, string, etc..), complex user defined types (eg: your classes)
and collections of built-in and user-defined types.

UltraMapper.CommandLine is powered by UltraMapper which uses Expressions to <b>generate the code</b> needed to deal with your commands, instead of Reflection, to guarantee good performances.



## Getting started

All of the examples use the default built-in syntax. Read more about the syntax here.

In order to parse and execute your args commands you need to call CommandLine.Instance.Parse<Commands>( args )
where 'Commands' is a class where you define the operations that you want to allow at commandline level.

    static void Main( string[] args )
    {
        //Creates a new instance of type UserInfo and writes on it
        var userInfo = CommandLine.Instance.Parse<UserInfo>( args );
    }
    
    public class UserInfo
    {
        ...
    }
    
You can work with the same instance preserving state.
(Subsequent calls write on the same instance instead of creating a new one each time)

    static void Main( string[] args )
    {
        //--name "John Smith"
        var userInfo = new UserInfo()
        CommandLine.Instance.Parse<UserInfo>( args, userInfo );
        
        //--age 26
        string newArgs = Console.Readline()
        CommandLine.Instance.Parse<UserInfo>( newArgs, userInfo );
        
        Assert.IsTrue( userInfo.Name == "John Smith" && userInfo.Age == 26)
    }
    
    public class UserInfo
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
    
You can call the following utility method to setup an infinite loop reading and parsing args:

  - Creating a new instance each time:

        ConsoleLoop.Start<UserInfo>( args, parsed =>
        {
            //do what you want with your strongly-type parsed args
        } );

  - Writing on the same instance:     

        static void Main( string[] args )
        {
            var userInfo = new UserInfo();
            ConsoleLoop.Start( args, userInfo, parsed =>
            {
                //do what you want with your strongly-type parsed args
            } );
        }
   
## Set property
  
  Conversion from string to any primitive type is supported.
  Complex types, collections and collections of complex types are supported too.
  
      static void Main( string[] args )
      {
          var userInfo = CommandLine.Instance.Parse<UserInfo>( args );
      }
    
      public class UserInfo
      {
          public string Name { get; set; }
          public int Age { get; set; }
          public bool IsMarried { get; set; }

          public class BankAccountInfo
          {
              public string AccountNumber { get; set; }
              public double Balance { get; set; }
          }
          public BankAccountInfo BankAccount { get; set; }
      }
         
You can set multiple properties with a single commandline just by separating each command with a whitespace like this:    
    --name "John Smith" --age 26 --ismarried --bankaccount (aa5500001123 1500,50)
     
You can also set individual properties like this:        
    --name "John Smith"
    --age 26    
    --ismarried 
    --bankaccount (aa5500001123 1500,50)
    
### Bool properties

Bool propeties are special: if you want to set a boolean property to 'true', you can omit to write 'true' as value

example:

    --isMarried true   
    
is equivalent to   

    --isMarried

    
## Method call

While other commandline parser libraries only allow you to set properties (and thus force you to set up fake flags
indicating what methods to call, and, more importantly, force you to write the logic responsible of executing the relevant methods);

UltraMapper.CommandLine allows you to call methods directly, avoiding a whole lot messy boilerplate code, 
and virtually eliminating the need to define properties at Commands class level altogether, 
since you can define a method taking as input as many params as you want.

    public class Commands
    {
        //call example from commandline: --opendir C:\
        public void OpenDir( string path )
        {
            Process.Start( path );
        }

        //call example from commandline: --opendirs [C:\ C:\windows\]
        public void OpenDirs( string[] paths )
        {
            foreach( var path in paths )
                Process.Start( path );
        }       
    }
    
 If a method takes too many parameters as input and you want to organize them in a better way
 you can define a new class and work with it. Complex types and collections of comple types are fully supported.
 
    public class Commands
    {
        public class MoveParams
        {
            public string From { get; set; }
            public string To { get; set; }
        }

        //call example from commandline: --movefile (C:\temp\file.txt C:\file.txt")
        public void MoveFile( MoveParams moveParams )
        {
            Console.WriteLine( $"You want to move from:{moveParams.From} to:{moveParams.To}" );
        }

        //call example from commandline: --movefiles [(C:\temp\file1.txt C:\file1.txt") (C:\temp\file2.txt C:\file2.txt")]
        public void MoveFiles( IEnumerable<MoveParams> moveParams )
        {
            foreach( var item in moveParams )
                Console.WriteLine( $"moving file from '{item.From}' to '{item.To}'" );
        }
    }

## Default parser syntax:
 
  - --<commmandName> a <b>double dash</b> indentifies a command      
    example: --close
    
  - <b>Whitespaces</b> characters delimit commmands and values       
  example: --move C:\Temp\file.txt C:\Archive\file.txt 
    
  - <b>Double quotes</b> escape special characters, including whitespaces.      
  example: --move "C:\folder with spaces in the name\file.txt" C:\Archive\file.txt 
        
  - if your param is a complex type, <b>round brackets</b> identies the object      
  example: --sum ()
    
  - If your param is a collection <b>square brackets</b> identifies the collection       
  example: --sum [1 2 3 4 5]    
    
  - <b>Collections of complex types</b> are supported, recursively, without limits     
  example: --sum [ (a b) (c d) (e f) ]    
  
  - Parameters can be specified indicating the param name.
    Named params must appear after non-named params.
    Non-named params must be provided in a exact order.
    By default the order is the definition order of your params inside your commands class.
    You can override the definition order by setting the property 'Order' via the 'Option' attribute.
  
## Multiple advancements compared to other similar projects include:

  - The ability to invoke a method directly:

        public class Commands
        {
            public void Move( string from, string to ){ ... }
        }

        var args = "--move C:\text.txt C:\subfolder\text.txt";   
        //Move method gets executed on parse
        AutoParser.Instance.Parse<Commands>( args );       

  - Map values to complex types, instantiating null instances.
    No hierarchy limits in depth.

        public class Commands
        {
            public class MoveCommand
            {
                public string From { get; set; }
                public string To { get; set; }
            }

            public MoveCommand Move{get; set;}
        }

        var args = "--move C:\text.txt C:\subfolder\text.txt";   
        var parsedObj = AutoParser.Instance.Parse<Commands>( args );       
        Assert.IsTrue( parsedObj.Move.From = "C:\text.txt" );
        Assert.IsTrue( parsedObj.Move.To = "C:\subfolder\text.txt" );
      
      Complex types support make UltraMapper.CommandLine especially useful when trying to organize
      a non trivial number of parameters in a natural way.
      
- Array and IEnumerable support

        public class Commands
        {
            public void Open( IEnumerable<string> paths ){ ... }
        }

        var args = "--open [C:\ C:\subfolder1 C:\subfolder2]";   
        //Open method gets executed on parse
        AutoParser.Instance.Parse<Commands>( args );      

- All of the previous features combined:

        public class Commands
        {
            public class MoveCommand
            {
                public string From { get; set; }
                public string To { get; set; }
            }

            public void MoveAndOpen( MoveCommand move, IEnumerable<string> openPaths ){ ... }
        }

        var args = "--open [C:\ C:\subfolder1 C:\subfolder2]";   
        //Open method gets executed on parse
        AutoParser.Instance.Parse<Commands>( args );      


## Provide your very own syntax!
You can provide and use a new syntax along with UltraMapper.CommandLine by implementing ICommandParser.
A few rules apply to it. Read more here.
    
## --help command and IHelpProvider:
If you manually define a help command in your commands class, it will be invoked when invoking --help

If you do not define a help command, a help command is automatically generated for you.   
The default help provider used in this latter case will analyze your commands class and generate adequate usage documentation keeping into account both operations and parameters.

You can also provide a new helper by implementing an IHelpProvider. A few rules apply to it. Read more here.
    
## Remarks:

- Works with with properties and methods but not with fields.
- Value types are not supported.
- Method calls are only supported if the method is defined in the 'Commands' class: you cannot call a parameter's method!
- Methods can be called directly only if void, non abstract, non generic.    

## How can you contribute?

- Support standard command line dialects:
    
    I had to come up with a new syntax in order to support the identification of complex types and collections, however it would also be nice to support other commandline dialects and JSON (even if some UltraMapper.CommandLine features would not be available to them).
    You can try and write a JSON parser by implementing the IParser interface. (Feel free to write me to ask details)


