//using CommandLine.AutoParser.Attributes;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace CommandLine.AutoParser.UnitTest
//{
//    [TestClass]
//    //cannot test independently!!!!!!!!
//    public class NameOverride
//    {
//        public class MyCommands
//        {
//            [Option( Name = "quit" )]
//            public void Exit() { }

//            [Option( Name = "1open" )]
//            public bool Open { get; set; }

//            public void Move( [Option( Name = "--from" )] string from, string to )
//            {

//            }
//        }

//        [TestMethod]
//        public void PropertyNameOverride()
//        {
//            var args2 = "--quit";
//            var parsed2 = AutoCommand.Instance.Parse<MyCommands>( args2 );
//        }

//        [TestMethod]
//        public void MethodNameOverride()
//        {
//            var args2 = "--1open";
//            var parsed2 = AutoCommand.Instance.Parse<MyCommands>( args2 );
//        }

//        [TestMethod]
//        public void MethodParamNameOverride()
//        {
//            var args2 = "--move --from:fromhere tohere";
//            var parsed2 = AutoCommand.Instance.Parse<MyCommands>( args2 );
//        }
//    }
//}
