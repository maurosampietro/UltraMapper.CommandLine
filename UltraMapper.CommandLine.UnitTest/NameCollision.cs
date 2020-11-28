using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UltraMapper.CommandLine.UnitTest
{
    /// <summary>
    /// Methods and property are already checked by the C# language
    /// property 'bool Open' and method 'void Open(bool)' cannot be defined 
    /// in the same class at the same level
    /// </summary>
    [TestClass]
    public class NameCollisions
    {
        public class Commands
        {
            public bool Open { get; set; }

            [Option( Name = "Open" )]
            public void Operation( string path ) { }

            [Option( Name = "Open" )]
            public void Operation( string path, string path2 ) { }
        }

        public class Commands2
        {
            public bool Open { get; set; }
        }

        [TestMethod]
        public void CommandExistance2()
        {
            var args = "--operation true";
            Assert.ThrowsException<UndefinedCommandException>(
                 () => CommandLine.Instance.Parse<Commands>( args ) );
        }

        [TestMethod]
        public void DuplicateCommandNames()
        {
            var args = "--open true";
            Assert.ThrowsException<DuplicateCommandException>(
                 () => CommandLine.Instance.Parse<Commands>( args ) );
        }

        [TestMethod]
        public void CommandExistance()
        {
            var args = "--thiscommanddoesnotexist true";
            Assert.ThrowsException<UndefinedCommandException>(
                 () => CommandLine.Instance.Parse<Commands2>( args ) );
        }
    }
}
