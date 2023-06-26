using System;
using Xunit;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TypeMerger.Tests {

    public class TypeMergerTests {

        [Fact]
        public void Test_Basic_Merge_Types() {


            var obj1 = new { Property1 = "1", Property2 = "2", Property3 = "3", Property4 = "4", Property5 = "5", Property6 = "6", Property7 = "7", Property8 = "8", Property9 = "9", Property10 = "10" };
            var obj2 = new { Property11 = "11", Property12 = "12", Property13 = "13", Property14 = "14", Property15 = "15", Property16 = "16", Property17 = "17", Property18 = "18", Property19 = "19", Property20 = "20" };

            var result = TypeMerger.Merge(obj1, obj2);

            result.GetType().GetProperties().Length.Should().Be(20);

            result.GetType().GetProperty("Property1").GetValue(result).Should().Be("1");
            result.GetType().GetProperty("Property2").GetValue(result).Should().Be("2");
            result.GetType().GetProperty("Property3").GetValue(result).Should().Be("3");
            result.GetType().GetProperty("Property4").GetValue(result).Should().Be("4");
            result.GetType().GetProperty("Property5").GetValue(result).Should().Be("5");
            result.GetType().GetProperty("Property6").GetValue(result).Should().Be("6");
            result.GetType().GetProperty("Property7").GetValue(result).Should().Be("7");
            result.GetType().GetProperty("Property8").GetValue(result).Should().Be("8");
            result.GetType().GetProperty("Property9").GetValue(result).Should().Be("9");
            result.GetType().GetProperty("Property10").GetValue(result).Should().Be("10");
            result.GetType().GetProperty("Property11").GetValue(result).Should().Be("11");
            result.GetType().GetProperty("Property12").GetValue(result).Should().Be("12");
            result.GetType().GetProperty("Property13").GetValue(result).Should().Be("13");
            result.GetType().GetProperty("Property14").GetValue(result).Should().Be("14");
            result.GetType().GetProperty("Property15").GetValue(result).Should().Be("15");
            result.GetType().GetProperty("Property16").GetValue(result).Should().Be("16");
            result.GetType().GetProperty("Property17").GetValue(result).Should().Be("17");
            result.GetType().GetProperty("Property18").GetValue(result).Should().Be("18");
            result.GetType().GetProperty("Property19").GetValue(result).Should().Be("19");
            result.GetType().GetProperty("Property20").GetValue(result).Should().Be("20");
        }

        [Fact]
        public void Merge_Types_with_Ignore_Policy() {

            var obj1 = new { Property1 = "value1", Property2 = "value1" };
            var obj2 = new { Property1 = "value2", Property4 = "value4" };

            var result = TypeMerger.Ignore(() => obj1.Property1)
                                   .Ignore(() => obj2.Property4)
                                   .Merge(obj1, obj2);

            result.GetType().GetProperties().Length.Should().Be(2);
            result.GetType().GetProperty("Property1").GetValue(result).Should().Be("value2");
            result.GetType().GetProperty("Property2").Should().NotBeNull();

        }

        [Fact]
        public void Merge_Types_with_Ignore_Policy_By_Name() {

            var obj1 = new { Property1 = "value1", Property2 = "value1" };
            var obj2 = new { Property1 = "value2", Property4 = "value4" };

            var result = TypeMerger.Ignore(obj1, nameof(obj1.Property1))
                                   .Ignore(obj2, nameof(obj2.Property4))
                                   .Merge(obj1, obj2);

            result.GetType().GetProperties().Length.Should().Be(2);
            result.GetType().GetProperty("Property1").GetValue(result).Should().Be("value2");
            result.GetType().GetProperty("Property2").Should().NotBeNull();

        }

        [Fact]
        public void Merge_Types_with_Name_Collision() {

            var obj1 = new { Property1 = "value1", Property2 = "2" };
            var obj2 = new { Property1 = "value2", Property3 = "3" };

            var result1 = TypeMerger.Merge(obj1, obj2);

            result1.GetType().GetProperties().Length.Should().Be(3);
            result1.GetType().GetProperty("Property1").GetValue(result1).Should().Be("value1");
            result1.GetType().GetProperty("Property3").GetValue(result1).Should().Be("3");

            var result2 = TypeMerger.Merge(obj2, obj1);

            result2.GetType().GetProperties().Length.Should().Be(3);
            result2.GetType().GetProperty("Property1").GetValue(result2).Should().Be("value2");
            result2.GetType().GetProperty("Property3").GetValue(result2).Should().Be("3");

        }

        [Fact]
        public void Test_Use_Method_to_Handle_Name_Collision_Priority() {

            var obj1 = new { Property1 = "value1", Property2 = "2" };
            var obj2 = new { Property1 = "value2", Property3 = "3" };

            var result1 = TypeMerger.Use(() => obj2.Property1)
                                    .Merge(obj1, obj2);

            result1.GetType().GetProperties().Length.Should().Be(3);
            result1.GetType().GetProperty("Property1").GetValue(result1).Should().Be("value2");

        }

        // This test will fail... Should we support this?
        // What causes this to fail is that we are using 2 objects that are the same type and TypeMerger.MergeValues can't handle that
        // [Fact]
        // public void Test_Use_Method_with_same_object_types() {

        //     var obj1 = new { Property1 = "value1", Property2 = "2", Property3 = "4" };
        //     var obj2 = new { Property1 = "value2", Property2 = "3", Property3 = "5"};

        //     var result1 = TypeMerger.Use(() => obj2.Property2)
        //                             .Merge(obj1, obj2);

        //     result1.GetType().GetProperties().Length.Should().Be(3);
        //     result1.GetType().GetProperty("Property2").GetValue(result1).Should().Be("3");

        // }


        [Fact]
        public void Test_Ignore_and_Use_Methods_used_in_Single_Merge_Policy() {

            var obj1 = new { Property1 = "value1", Property2 = "2" };
            var obj2 = new { Property1 = "value2", Property3 = "3" };

            var result = TypeMerger.Use(() => obj2.Property1)
                                   .Ignore(() => obj2.Property3)
                                   .Merge(obj1, obj2);

            result.GetType().GetProperties().Length.Should().Be(2);
            result.GetType().GetProperty("Property1").GetValue(result).Should().Be(obj2.Property1);
            result.GetType().GetProperty("Property2").GetValue(result).Should().Be(obj1.Property2);

        }

        [Fact]
        public void Test_Multiple_Type_Creation_from_Same_Anonymous_Types_Sources() {

            var obj1 = new { Property1 = "value1", Property2 = "2" };
            var obj2 = new { Property1 = "value2", Property3 = "3" };

            var result1 = TypeMerger.Merge(obj1, obj2);

            result1.GetType().GetProperties().Length.Should().Be(3);
            result1.GetType().GetProperty("Property1").GetValue(result1).Should().Be("value1");

            var result2 = TypeMerger.Ignore(() => obj1.Property2)
                                .Merge(obj1, obj2);

            result2.GetType().GetProperties().Length.Should().Be(2);
            result2.GetType().GetProperty("Property3").GetValue(result2).Should().Be("3");
        }

        [Fact]
        public void Test_Type_Creation_from_Concrete_Classes() {

            var class1 = new TestClass1 {
                Name = "Foo",
                Description = "Test Class Instance",
                Number = 10,
                SubClass = new TestSubClass1 {
                    Internal = "Inside"
                }
            };

            var class2 = new TestClass2 {
                FullName = "Test Class 2",
                FullAddress = "123 Main St.",
                Total = 28
            };

            var result = TypeMerger.Ignore(() => class1.Name)
                               .Ignore(() => class2.Total)
                               .Merge(class1, class2);

            result.GetType().GetProperties().Length.Should().Be(5);

            result.GetType().GetProperty("SubClass").PropertyType.Should().Be(typeof(TestSubClass1));
            result.GetType().GetProperty("SubClass").GetValue(result)
                            .GetType().GetProperty("Internal")
                            .GetValue(result.GetType().GetProperty("SubClass")
                            .GetValue(result)).Should().Be(class1.SubClass.Internal);

        }

        [Fact]
        public void Test_Class_with_Built_in_Types() {

            var obj1 = new { Property1 = "value1", Property2 = "2" };
            var obj2 = new AllBuiltInTypes {
                ByteType = Byte.MaxValue,
                SByteType = SByte.MaxValue,
                Int32Type = Int32.MaxValue,
                UInt32Type = UInt32.MaxValue,
                Int16Type = Int16.MaxValue,
                UInt16Type = UInt16.MaxValue,
                Int64Type = Int64.MaxValue,
                UInt64Type = UInt64.MaxValue,
                SingleType = Single.MaxValue,
                DoubleType = Double.MaxValue,
                DecimalType = 300.5m,
                BooleanType = false,
                CharType = '\x0058',
                ObjectType = new { Test = 1 },
                StringType = "foo",
                DateTimeType = DateTime.Now,
                EnumType = TestEnum.Val1
            };

            var result1 = TypeMerger.Merge(obj1, obj2);

            result1.GetType().GetProperties().Length.Should().Be(19);

            result1.GetType().GetProperty("Property1").GetValue(result1).Should().Be(obj1.Property1);
            result1.GetType().GetProperty("Property2").GetValue(result1).Should().Be(obj1.Property2);
            result1.GetType().GetProperty("ByteType").GetValue(result1).Should().Be(obj2.ByteType);
            result1.GetType().GetProperty("SByteType").GetValue(result1).Should().Be(obj2.SByteType);
            result1.GetType().GetProperty("Int32Type").GetValue(result1).Should().Be(obj2.Int32Type);
            result1.GetType().GetProperty("UInt32Type").GetValue(result1).Should().Be(obj2.UInt32Type);
            result1.GetType().GetProperty("Int16Type").GetValue(result1).Should().Be(obj2.Int16Type);
            result1.GetType().GetProperty("UInt16Type").GetValue(result1).Should().Be(obj2.UInt16Type);
            result1.GetType().GetProperty("Int64Type").GetValue(result1).Should().Be(obj2.Int64Type);
            result1.GetType().GetProperty("UInt64Type").GetValue(result1).Should().Be(obj2.UInt64Type);
            result1.GetType().GetProperty("SingleType").GetValue(result1).Should().Be(obj2.SingleType);
            result1.GetType().GetProperty("DoubleType").GetValue(result1).Should().Be(obj2.DoubleType);
            result1.GetType().GetProperty("DecimalType").GetValue(result1).Should().Be(obj2.DecimalType);
            result1.GetType().GetProperty("BooleanType").GetValue(result1).Should().Be(obj2.BooleanType);
            result1.GetType().GetProperty("CharType").GetValue(result1).Should().Be(obj2.CharType);
            result1.GetType().GetProperty("ObjectType").GetValue(result1).Should().Be(obj2.ObjectType);
            result1.GetType().GetProperty("SingleType").GetValue(result1).Should().Be(obj2.SingleType);
            result1.GetType().GetProperty("DateTimeType").GetValue(result1).Should().Be(obj2.DateTimeType);
            result1.GetType().GetProperty("EnumType").GetValue(result1).Should().Be(TestEnum.Val1);

        }

        [Fact]
        public void Test_Derived_Class_with_Ignored_Base_Class_Property() {

            var obj1 = new DerivedClass {
                Name = "foo"
            };

            var obj2 = new { Value = 123 };

            var result = TypeMerger.Ignore(() => obj1.Name).Merge(obj1, obj2);

            result.GetType().GetProperties().Length.Should().Be(1);

        }

        [Fact]
        public void Test_Anonymous_Types_With_Names_Greater_Than_1024_Should_Use_Hash() {

            // Arrange
            var count = 30;
            object result = null;

            var l = Enumerable.Range(0, count - 1).Select(n => {
                var k = n * 3;

                return new {
                    Prop1 = k + 1,
                    Prop2 = k + 2,
                    Prop3 = k + 3,
                };
            }).ToList();

            // Act
            l.ForEach(o => result = TypeMerger.Merge(result ?? new { }, o));

            // Assert
            result.Should().NotBeNull();

            var anonymousTypes = typeof(TypeMerger).GetField("anonymousTypes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null) as Dictionary<string, Type>;

            anonymousTypes.Keys.Last().IndexOf('_').Should().BeGreaterThan(5);
        }

        [Fact]
        public void Test_TypeMergerPolicy_Maintained_On_High_Volume() {

            // Arrange

            // setup some objects
            var obj1 = new { Prop1 = 1, Prop2 = 2, Prop3 = 3 };
            var obj2 = new { Prop1 = 4, Prop2 = 5, Prop4 = 6 };
            var obj3 = new { Prop1 = 7, Prop2 = 8, Prop5 = 9 };
            var obj4 = new { Prop1 = 10, Prop2 = 11, Prop6 = 12 };
            var obj5 = new { Prop1 = 13, Prop2 = 14, Prop7 = 15 };
            var obj6 = new { Prop1 = 16, Prop2 = 17, Prop8 = 18 };
            var obj7 = new { Prop1 = 19, Prop2 = 20, Prop9 = 21 };
            var obj8 = new { Prop1 = 22, Prop2 = 23, Prop10 = 24 };
            var obj9 = new { Prop1 = 25, Prop2 = 26, Prop11 = 27 };
            var obj10 = new { Prop1 = 28, Prop2 = 29, Prop12 = 30 };

            // store them in a list of tuple<object, object, TypeMergerPolicy, object>
            var objects = new List<Tuple<object, object, TypeMergerPolicy, object>> {
                new Tuple<object, object, TypeMergerPolicy, object>(obj1, obj2, TypeMerger.Use(() => obj2.Prop2), new { Prop1 = 1, Prop2 = 5 }),
                new Tuple<object, object, TypeMergerPolicy, object>(obj2, obj3, TypeMerger.Use(() => obj3.Prop1), new { Prop1 = 7, Prop2 = 5}),
                new Tuple<object, object, TypeMergerPolicy, object>(obj3, obj4, TypeMerger.Use(() => obj4.Prop2), new { Prop1 = 7, Prop2 = 11}),
                new Tuple<object, object, TypeMergerPolicy, object>(obj4, obj5, TypeMerger.Use(() => obj5.Prop1), new { Prop1 = 13, Prop2 = 11}),
                new Tuple<object, object, TypeMergerPolicy, object>(obj5, obj6, TypeMerger.Use(() => obj6.Prop2), new { Prop1 = 13, Prop2 = 17}),
                new Tuple<object, object, TypeMergerPolicy, object>(obj6, obj7, TypeMerger.Use(() => obj7.Prop1), new { Prop1 = 19, Prop2 = 17}),
                new Tuple<object, object, TypeMergerPolicy, object>(obj7, obj8, TypeMerger.Use(() => obj8.Prop2), new { Prop1 = 19, Prop2 = 23}),
                new Tuple<object, object, TypeMergerPolicy, object>(obj8, obj9, TypeMerger.Use(() => obj9.Prop1), new { Prop1 = 25, Prop2 = 23}),
                new Tuple<object, object, TypeMergerPolicy, object>(obj9, obj10, TypeMerger.Use(() => obj10.Prop2), new { Prop1 = 25, Prop2 = 29}),
                new Tuple<object, object, TypeMergerPolicy, object>(obj10, obj1, TypeMerger.Use(() => obj1.Prop1), new { Prop1 = 1, Prop2 = 29})
            };

            var results = new Object[10];

            // Act
            Parallel.For(0, 9, (i, loopState) => {
                results[i] = objects[(int)i].Item3.Merge(objects[(int)i].Item1, objects[(int)i].Item2);
            });

            // Assert
            for (int i = 0; i < 9; i++) {
                for (int j = 1; j < 3; j++) {
                    var val1 = results[i].GetType().GetProperty($"Prop{j}").GetValue(results[i]);
                    var val2 = objects[i].Item4.GetType().GetProperty($"Prop{j}").GetValue(objects[i].Item4);
                    val1.Should().Be(val2);
                }
            }
        }

    }

    public class BaseClass {
        public string Name { get; set; }
    }

    public class DerivedClass : BaseClass { }

    public class TestClass1 {

        public string Name { get; set; }
        public string Description { get; set; }
        public int Number { get; set; }
        public TestSubClass1 SubClass { get; set; }

    }

    public class TestClass2 {

        public string FullName { get; set; }
        public string FullAddress { get; set; }
        public int Total { get; set; }
    }

    public class TestSubClass1 {
        public string Internal { get; set; }
    }

    public class AllBuiltInTypes {
        public Byte ByteType { get; set; }
        public SByte SByteType { get; set; }
        public Int32 Int32Type { get; set; }
        public UInt32 UInt32Type { get; set; }
        public Int16 Int16Type { get; set; }
        public UInt16 UInt16Type { get; set; }
        public Int64 Int64Type { get; set; }
        public UInt64 UInt64Type { get; set; }
        public Single SingleType { get; set; }
        public Double DoubleType { get; set; }
        public Char CharType { get; set; }
        public Boolean BooleanType { get; set; }
        public Object ObjectType { get; set; }
        public String StringType { get; set; }
        public Decimal DecimalType { get; set; }
        public DateTime DateTimeType { get; set; }
        public Enum EnumType { get; set; }
    }

    public enum TestEnum { Val1 = 1, Val2, Val3, Val4, Val5 };
}
