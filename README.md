# UltraMapper.CommandLine

THIS DOCUMENTATION IS A WORK IN PROGRESS. BE PATIENT [2020-NOV-23].


Suppose you have a console program capable of executing multiple complex tasks.
You tipically will control the program by passing arguments to it.

You will face the need to give that text a sense by defining rules that will allow you to split it into meaningful chuncks;
usually those rules will allow you to split the text in such a way that it easy to identity the parameter you want to assign the value to, and the value itself.

This process is called parsing. Read more about the default syntax.

Once you identified all of the parameters and the related values you will need to cast the values from plain text into a more specific strong-type (int, double, etc..)
Once that is done, you can read those strongly-typed values to call appropriate methods.

With UltraMapper.CommandLine will drastically simplify your code as all of this is completly automatic: you just need to define the operations to be executed in a class.
You can directly set properties and call methods taking as arguments built-in types (eg: bool, int, double, string, etc..), complex user defined types (eg: your classes)
and collections of built-in and user-defined types.

An example:

The following program supports 5 commands or operations: it can sum a given amount of numbers, open any directory in explorer, clear the conosle screen, exit the program and set how much time to wait before exiting the program. 
Parsing the arguments onto the class Commands will start the process of analyzing the Commands class and generating the necessary code capable of calling its properties and methods.

Let's say i want to open C:\Temp, sum the numbers '1,2,3', set the sleeping time to 10000ms, clear screen and exit.
All of this can be done by convention writing the following command line:

    --opendir C:\temp --sum [1 2 3] --sleepingtime 10000 --clearscreen --exit

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

## Default parser syntax:

  example 1: --move C:\ThisFile.exe "C:\New Directory\ThisFile.exe" 
  
  - --<commmandName> a <b>double dash</b> indentifies a command      
  - <b>Whitespaces</b> characters delimit commmands and values
  - Double quotes escape special characters and whitespace.
  
  - if your param is a complex type, <b>round brackets</b> identies the object
    example: --sum ()
    
  - If your param is a collection <b>square brackets</b> identifies the collection
    example: --sum [[1 2 3 4 5]]  
    
  - <b>Collections of complex types</b> are supported, recursively, without limits
    example: --sum [( a b) ( c d ) (e f) ]  
  
  
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

- AutoParser works with with properties and methods but not with fields.
- Methods are only supported at first level (makes no sense otherwise)
- Methods can be called directly only if void, non abstract, non generic.    

## How can you contribute?

- Support standard command line dialects:
    
    I had to come up with a new syntax in order to support the identification of complex types and collections, however it would also be nice to support other commandline dialects and JSON (even if some UltraMapper.CommandLine features would not be available to them).
    You can try and write a JSON parser by implementing the IParser interface. (Feel free to write me to ask details)


