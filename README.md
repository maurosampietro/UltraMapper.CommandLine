# UltraMapper.CommandLine

Suppose you have a console program capable of executing multiple complex tasks.
You tipically will control the program by passing arguments to it.

You will face the need to give that text a sense by defining rules that will allow you to split it into meaningful chuncks;
usually those rules will allow you to split the text in such a way that it easy to identity the parameter you want to assign the value to, and the value itself.

This process is called parsing.

Once you identified all of the parameters and the related values you will need to cast the values from plain text into a more specific strong-type (int, double, etc..)
Once that is done, you can read those strongly-typed values to call appropriate methods.

With UltraMapper.CommandLine all of this is automatic, you just need to define your operations to be executed.
You can directly call methods taking as arguments, built-in types (eg: bool, int, double, string, etc..) and complex user defined types (eg: your classes)
and collections of built-in and user-defined types.

An example:

    static void Main( string[] args )
    {
        CommandLine.Instance.Parse<Commands>( args );
    }

    public class Commands
    {
        public int SleepingTime { get; set; }
        
        public void ClearScreen() { Console.Clear(); }
        public void OpenDir( string path ) { Process.Start( path ); }
        public void Sum( IEnumerable<int> numbers ) {  Console.WriteLine( $"The sum is: {numbers.Sum()}" ); }
        
        public void Exit()
        {
            Console.WriteLine( "Application will close in 5 seconds" );
            Thread.Sleep( this.SleepingTime );
            Environment.Exit( 0 );
        }
    }

Parse and map command line args to built-in and complex (custom user defined) types. then invoke methods automatically.
UltraMapper.CommandLine uses Expressions to generate all the code needed to deal with your commands, instead of Reflection to guarantee good performances.     

Default syntax:

  example 1: --move C:\ThisFile.exe "C:\New Directory\ThisFile.exe" 
  
  - --<commmandName> a double dash indentifies a command      
  - Whitespaces characters delimit commmands and values
  - Double quotes escape special characters and whitespace.
  
  - if your param is a complex type, round brackets identies the object
    example: --sum ()
    
  - If your param is a collection square brackets identifies the collection
    example: --sum [[1 2 3 4 5]]  
    
  - Collections of complex type are supported, recursively, without limits
    example: --sum [( a b) ( c d ) (e f) ]  
  
  
Multiple advancements compared to other similar projects include:

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

- Simple syntax:

  Whitespaces characters delimits commmands and values.     
  --{commmandName} a double dash indentifies a command.      
  [value1 value2 value3] identifies a sequence of values.      
  "escape a whitespace" double quotes escape special characters as whitespace.   
  

Remarks:

- AutoParser works with with properties and methods but not with fields.
- Methods are only supported at first level (makes no sense otherwise)
-  Methods can be called directly only if void, non abstract, non generic.    
