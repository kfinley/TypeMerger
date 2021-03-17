using System;
using Xunit;
using FluentAssertions;

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