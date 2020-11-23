# UltraMapper.CommandLine

Parse and map command line args, then invoke methods automatically.
UltraMapper.CommandLine uses Expressions to generate all the code needed to deal with your commands, instead of Reflection to guarantee good performances.     

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
