using Microsoft.VisualStudio.TestTools.UnitTesting;
using UltraMapper.MappingExpressionBuilders;
using UltraMapper.Parsing;
using UltraMapper.Parsing.Extensions;

namespace UltraMapper.CommandLine.UnitTest.UltraMapper.Extensions
{
    [TestClass]
    public class UltraMapperExt
    {
        public class Commands
        {
            public class Level1
            {
                public class Level2
                {
                    public class Level3
                    {
                        public string G { get; set; }
                        public string H { get; set; }
                    }

                    public string D { get; set; }
                    public string E { get; set; }

                    [Option( IsRequired = false )]
                    public Level3 SubLevel3 { get; set; }
                }

                public string A { get; set; }
                public string B { get; set; }

                public Level2 SubLevel2 { get; set; }
            }

            public string PropertyA { get; set; }
            public int PropertyB { get; set; }
            public Level1 SomeCommand { get; set; }
        }

        [TestMethod]
        public void BasicMapByParamName()
        {
            //funziona solo assegnando a tutti i valori un nome (per adesso)
            var args = new string[]
            {
                $"--{nameof( Commands.PropertyA )} {nameof( Commands.PropertyA )}",
                $"--{nameof( Commands.PropertyB )} {nameof( Commands.PropertyB ).GetHashCode()}",
                $"--{nameof( Commands.SomeCommand )} (a=a b=11 sublevel2=(d=d e=e sublevel3=(g=g h=h)))"
            };

            var target = CommandLine.Instance.Parse<Commands>( args );

            Assert.IsTrue( target.PropertyA == nameof( Commands.PropertyA ) );
            Assert.IsTrue( target.PropertyB == nameof( Commands.PropertyB ).GetHashCode() );
            Assert.IsTrue( target.SomeCommand.A == "a" );
            Assert.IsTrue( target.SomeCommand.B == "11" );
            Assert.IsTrue( target.SomeCommand.SubLevel2.D == "d" );
            Assert.IsTrue( target.SomeCommand.SubLevel2.E == "e" );
            Assert.IsTrue( target.SomeCommand.SubLevel2.SubLevel3.G == "g" );
            Assert.IsTrue( target.SomeCommand.SubLevel2.SubLevel3.H == "h" );
        }

        [TestMethod]
        public void BasicMapByParamIndex()
        {
            var args = new string[]
            {
                $"--{nameof( Commands.PropertyA )} a",
                $"--{nameof( Commands.PropertyB )} 11",
                $"--{nameof( Commands.SomeCommand )} (a1 b1 (d e (g h)))"
            };

            var target = CommandLine.Instance.Parse<Commands>( args );

            Assert.IsTrue( target.PropertyA == "a" );
            Assert.IsTrue( target.PropertyB == 11 );
            Assert.IsTrue( target.SomeCommand.A == "a1" );
            Assert.IsTrue( target.SomeCommand.B == "b1" );
            Assert.IsTrue( target.SomeCommand.SubLevel2.D == "d" );
            Assert.IsTrue( target.SomeCommand.SubLevel2.E == "e" );
            Assert.IsTrue( target.SomeCommand.SubLevel2.SubLevel3.G == "g" );
            Assert.IsTrue( target.SomeCommand.SubLevel2.SubLevel3.H == "h" );
        }

        [TestMethod]
        public void ComplexParamRecursion()
        {
            var mapper = new Mapper();

            mapper.Config.Mappers.AddBefore<ReferenceMapper>( new IMappingExpressionBuilder[]
            {
                new SimpleParamExpressionBuilder( mapper.Config ),
                new ComplexParamExpressionBuilder( mapper.Config )
            } );

            var complexParam = new ComplexParam()
            {
                Index = 4,
                Name = "asdf",
                SubParams = new IParsedParam[]
                {
                    new SimpleParam()
                    {
                        Name = nameof( Commands.PropertyA ),
                        Index = 0,
                        Value = "A"
                    },

                    new SimpleParam()
                    {
                        Name = nameof( Commands.PropertyB ),
                        Index = 1,
                        Value = "1"
                    },

                    new ComplexParam()
                    {
                        Name = nameof( Commands.SomeCommand ),
                        Index = 2,
                        SubParams = new IParsedParam[]
                        {
                            new SimpleParam()
                            {
                                Name = nameof( Commands.SomeCommand.A ),
                                Index = 0,
                                Value = "SomeCommand.A"
                            },

                            new SimpleParam()
                            {
                                Name = nameof( Commands.SomeCommand.B ),
                                Index = 1,
                                Value = "SomeCommand.B"
                            },

                            new ComplexParam()
                            {
                                Name = nameof( Commands.SomeCommand.SubLevel2 ),
                                Index = 2,
                                SubParams = new IParsedParam[]
                                {
                                    new SimpleParam()
                                    {
                                        Name = nameof( Commands.SomeCommand.SubLevel2.D ),
                                        Index = 0,
                                        Value = "SomeCommand.SubLevel2.D"
                                    },

                                    new SimpleParam()
                                    {
                                        Name = nameof( Commands.SomeCommand.SubLevel2.E ),
                                        Index = 1,
                                        Value = "SomeCommand.SubLevel2.E"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var target = mapper.Map<Commands>( complexParam );

            Assert.IsTrue( target.PropertyA == "A" );
            Assert.IsTrue( target.PropertyB == 1 );
            Assert.IsTrue( target.SomeCommand.A == "SomeCommand.A" );
            Assert.IsTrue( target.SomeCommand.B == "SomeCommand.B" );
            Assert.IsTrue( target.SomeCommand.SubLevel2.D == "SomeCommand.SubLevel2.D" );
            Assert.IsTrue( target.SomeCommand.SubLevel2.E == "SomeCommand.SubLevel2.E" );
        }

        [TestMethod]
        [Ignore]
        public void DirectNestedPropertyAssignment() //Is it actually useful?
        {
            var args = $"--{nameof( Commands.SomeCommand )} SomeCommand.SubLevel2.SubLevel3.G:g";
            var parsed = CommandLine.Instance.Parse<Commands>( args );

            Assert.IsTrue( parsed.SomeCommand.SubLevel2.SubLevel3.G == "g" );
        }        
    }
}
