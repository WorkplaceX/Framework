namespace Framework.Test
{
    using Database.dbo;
    using Framework.Json;
    using System;
    using System.Collections.Generic;

    public static class UnitTest
    {
        public static void Run()
        {
            {
                A source = new A();
                source.MyEnum = MyEnum.Left;

                // Serialize, deserialize
                string json = UtilJson2.Serialize(source);
                A dest = (A)UtilJson2.Deserialize(json);

                UtilFramework.Assert(dest.MyEnum == MyEnum.Left);
                UtilFramework.Assert(dest.MyEnumNullable == null);
            }
            {
                A source = new A();
                source.MyEnumNullable = MyEnum.None;

                // Serialize, deserialize
                string json = UtilJson2.Serialize(source);
                A dest = (A)UtilJson2.Deserialize(json);
                UtilFramework.Assert(dest.MyEnumNullable == MyEnum.None);
            }
            {
                A source = new A();

                // Serialize, deserialize
                string json = UtilJson2.Serialize(source);
                A dest = (A)UtilJson2.Deserialize(json);

                UtilFramework.Assert(!json.Contains(nameof(A.MyEnumList)));
                UtilFramework.Assert(source.MyEnumList == null);
                UtilFramework.Assert(dest.MyEnumList != null);
                UtilFramework.Assert(dest.MyEnumList.Count == 0);
            }
            {
                A source = new A();
                source.MyEnumList = new List<MyEnum>();
                source.MyEnumList.Add(MyEnum.None);
                source.MyEnumList.Add(MyEnum.Left);
                source.MyEnumList.Add(MyEnum.Right);

                // Serialize, deserialize
                string json = UtilJson2.Serialize(source);
                A dest = (A)UtilJson2.Deserialize(json);

                UtilFramework.Assert(json.Contains(nameof(A.MyEnumList)));
                UtilFramework.Assert(dest.MyEnumList[0] == MyEnum.None);
                UtilFramework.Assert(dest.MyEnumList[1] == MyEnum.Left);
                UtilFramework.Assert(dest.MyEnumList[2] == MyEnum.Right);
            }
            {
                A source = new A();
                source.MyEnumNullableList = new List<MyEnum?>();
                source.MyEnumNullableList.Add(MyEnum.None);
                source.MyEnumNullableList.Add(MyEnum.Left);
                source.MyEnumNullableList.Add(null);
                source.MyEnumNullableList.Add(MyEnum.Right);

                // Serialize, deserialize
                string json = UtilJson2.Serialize(source);
                A dest = (A)UtilJson2.Deserialize(json);

                UtilFramework.Assert(json.Contains(nameof(A.MyEnumNullableList)));
                UtilFramework.Assert(dest.MyEnumNullableList[0] == MyEnum.None);
                UtilFramework.Assert(dest.MyEnumNullableList[1] == MyEnum.Left);
                UtilFramework.Assert(dest.MyEnumNullableList[2] == null);
                UtilFramework.Assert(dest.MyEnumNullableList[3] == MyEnum.Right);
            }
            {
                A source = new A();
                source.IntNullableList = new List<int?>();
                source.IntNullableList.Add(0);
                source.IntNullableList.Add(1);
                source.IntNullableList.Add(null);
                source.IntNullableList.Add(2);

                // Serialize, deserialize
                string json = UtilJson2.Serialize(source);
                A dest = (A)UtilJson2.Deserialize(json);

                UtilFramework.Assert(json.Contains(nameof(A.IntNullableList)));
                UtilFramework.Assert(dest.IntNullableList[0] == 0);
                UtilFramework.Assert(dest.IntNullableList[1] == 1);
                UtilFramework.Assert(dest.IntNullableList[2] == null);
                UtilFramework.Assert(dest.IntNullableList[3] == 2);
            }
            {
                A source = new A();
                source.IntList = new List<int>();
                source.IntList.Add(0);
                source.IntList.Add(1);
                source.IntList.Add(2);

                // Serialize, deserialize
                string json = UtilJson2.Serialize(source);
                A dest = (A)UtilJson2.Deserialize(json);

                UtilFramework.Assert(json.Contains(nameof(A.IntList)));
                UtilFramework.Assert(dest.IntList[0] == 0);
                UtilFramework.Assert(dest.IntList[1] == 1);
                UtilFramework.Assert(dest.IntList[2] == 2);
            }
            {
                A source = new A();
                source.V = 33;

                // Serialize, deserialize
                string json = UtilJson2.Serialize(source);
                A dest = (A)UtilJson2.Deserialize(json);

                UtilFramework.Assert((int)dest.V == 33);
            }
            {
                A source = new A();
                source.V = "Hello";

                // Serialize, deserialize
                string json = UtilJson2.Serialize(source);
                A dest = (A)UtilJson2.Deserialize(json);

                UtilFramework.Assert((string)dest.V == "Hello");
            }
            {
                var date = DateTime.Now;
                A source = new A();
                source.Row = new FrameworkScript { Id = 22, FileName = @"C:\Temp\Readme.txt", Date = date };

                // Serialize, deserialize
                string json = UtilJson2.Serialize(source);
                A dest = (A)UtilJson2.Deserialize(json);

                UtilFramework.Assert(dest.Row.Id == 22);
                UtilFramework.Assert(dest.Row.FileName == @"C:\Temp\Readme.txt");
                UtilFramework.Assert(dest.Row.Date ==date);
            }
        }
    }

    public enum MyEnum { None = 0, Left = 1, Right = 2 }

    public class A
    {
        public MyEnum MyEnum;

        public MyEnum? MyEnumNullable;

        public List<MyEnum> MyEnumList;

        public List<MyEnum?> MyEnumNullableList;

        public List<int> IntList;

        public List<int?> IntNullableList;

        public object V;

        public FrameworkScript Row;

    }
}