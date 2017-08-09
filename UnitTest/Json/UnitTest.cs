namespace UnitTest.Json
{
    using Framework;
    using Framework.Json;
    using System;
    using System.Collections.Generic;

    public class Data
    {
        public string Name;

        public int NumberInt;

        public double NumberDouble;

        public Guid Guid;
    }

    public class DataNullable
    {
        public string Name;

        public int? NumberInt;

        public double? NumberDouble;

        public Guid? Guid;

        public string Type;
    }

    public class DataWithList
    {
        public string Name;

        public List<DataWithListItem> List;
    }

    public class DataWithListItem
    {
        public string Name;
    }

    public class DataWithListItem2 : DataWithListItem
    {
        public string Type;
    }

    public class DataWithDictionary
    {
        public Dictionary<string, int> List;
    }

    public class DataWithDictionary2
    {
        public Dictionary<string, DataWithListItem> List;
    }

    public class DataWithListNested
    {
        public List<Dictionary<string, int>> List;

        public List<List<int>> List2;
    }

    public class DataWithObject
    {
        public object Value;
    }

    public class DataListNull
    {
        public string Name;

        public List<object> List;

        public Dictionary<string, int> List2;
    }

    public class DataListNestedNull
    {
        public List<List<object>> List;

        public Dictionary<string, Dictionary<string, int>> List2;
    }

    public class UnitTest : UnitTestBase
    {
        public void Test01()
        {
            Data data = new Json.Data();
            data.Name = "Name";
            data.NumberInt = 88;
            data.NumberDouble = 34.223;
            Guid guid = Guid.NewGuid();
            string json = JsonConvert.Serialize(data);
            var data2 = JsonConvert.Deserialize<Data>(json);
            UtilFramework.Assert(data.Name == data2.Name);
            UtilFramework.Assert(data.NumberInt == data2.NumberInt);
            UtilFramework.Assert(data.NumberDouble == data2.NumberDouble);
            UtilFramework.Assert(data.Guid == data2.Guid);
        }

        public void Test02()
        {
            DataNullable data = new Json.DataNullable();
            data.Name = "Name";
            data.NumberInt = 88;
            data.NumberDouble = 34.223;
            Guid guid = Guid.NewGuid();
            string json = JsonConvert.Serialize(data);
            var data2 = JsonConvert.Deserialize<DataNullable>(json);
            UtilFramework.Assert(data.Name == data2.Name);
            UtilFramework.Assert(data.NumberInt == data2.NumberInt);
            UtilFramework.Assert(data.NumberDouble == data2.NumberDouble);
            UtilFramework.Assert(data.Guid == data2.Guid);
        }

        public void Test03()
        {
            DataWithList data = new Json.DataWithList();
            data.Name = "L";
            data.List = null;
            string json = JsonConvert.Serialize(data);
            DataWithList data2 = JsonConvert.Deserialize<DataWithList>(json);
        }

        public void Test04()
        {
            DataWithList data = new Json.DataWithList();
            data.Name = "L";
            data.List = new List<Json.DataWithListItem>();
            data.List.Add(new Json.DataWithListItem() { Name = "X1" });
            data.List.Add(new Json.DataWithListItem2() { Name = "X2" });
            data.List.Add(null);
            string json = JsonConvert.Serialize(data);
            var data2 = JsonConvert.Deserialize<DataWithList>(json);
            UtilFramework.Assert(data.List[0].Name == data2.List[0].Name);
            UtilFramework.Assert(data.List[1].GetType() == typeof(DataWithListItem2));
        }

        public void Test05()
        {
            DataWithDictionary data = new Json.DataWithDictionary();
            data.List = new Dictionary<string, int>();
            data.List["F"] = 33;
            data.List["G"] = 44;
            string json = JsonConvert.Serialize(data);
            var data2 = JsonConvert.Deserialize<DataWithDictionary>(json);
            UtilFramework.Assert(data.List["F"] == 33);
            UtilFramework.Assert(data.List["G"] == 44);
        }

        public void Test06()
        {
            var data = new Json.DataWithDictionary2();
            data.List = new Dictionary<string, Json.DataWithListItem>();
            data.List["F"] = new DataWithListItem() { Name = "FF" };
            data.List["G"] = new DataWithListItem2() { Name = "GG" };
            data.List["H"] = null;
            string json = JsonConvert.Serialize(data);
            var data2 = JsonConvert.Deserialize<DataWithDictionary2>(json);
            UtilFramework.Assert(data.List["F"].Name == "FF");
            UtilFramework.Assert(data.List["G"].Name == "GG");
            UtilFramework.Assert(data.List["G"].GetType() == typeof(DataWithListItem2));
            UtilFramework.Assert(data.List["H"] == null);
        }

        public void Test07()
        {
            var data = new Json.DataWithListNested();
            data.List = new List<Dictionary<string, int>>();
            data.List.Add(new Dictionary<string, int>());
            data.List[0]["X"] = 99;
            data.List2 = new List<List<int>>();
            data.List2.Add(new List<int>());
            data.List2[0].Add(88);
            string json = JsonConvert.Serialize(data);
            var data2 = JsonConvert.Deserialize<DataWithListNested>(json);
        }

        public void Test08()
        {
            var data = new DataWithObject();
            data.Value = "H";
            string json = JsonConvert.Serialize(data);
            var data2 = JsonConvert.Deserialize<DataWithObject>(json);
            UtilFramework.Assert((string)data2.Value == "H");
        }

        public void Test09()
        {
            var data = new DataWithObject();
            data.Value = 2.23;
            string json = JsonConvert.Serialize(data);
            var data2 = JsonConvert.Deserialize<DataWithObject>(json);
            UtilFramework.Assert((double)data2.Value == 2.23);
        }

        public void Test10()
        {
            var data = new DataWithObject();
            data.Value = (double)2;
            string json = JsonConvert.Serialize(data);
            var data2 = JsonConvert.Deserialize<DataWithObject>(json);
            UtilFramework.Assert((double)data2.Value == 2);
        }

        public void Test11()
        {
            var data = new DataWithObject();
            data.Value = (int)2;
            try
            {
                string json = JsonConvert.Serialize(data);
            }
            catch (Framework.Json.JsonException exception)
            {
                UtilFramework.Assert(exception.Message == "Allowed types: string, double or bool!");
            }
        }

        public void Test12()
        {
            var data = new DataWithObject();
            data.Value = new DataWithListItem();
            try
            {
                string json = JsonConvert.Serialize(data);
            }
            catch (Framework.Json.JsonException exception)
            {
                UtilFramework.Assert(exception.Message == "Object has no Type field!");
            }
        }

        public void Test13()
        {
            var data = new DataWithObject();
            data.Value = new DataWithListItem2() { Name = "M" };
            string json = JsonConvert.Serialize(data);
            var data2 = JsonConvert.Deserialize<DataWithObject>(json);
            UtilFramework.Assert(data2.Value.GetType() == typeof(DataWithListItem2));
            UtilFramework.Assert(((DataWithListItem2)data2.Value).Name == "M");
        }

        public void Test14()
        {
            var data = new DataListNull();
            data.Name = "H";
            string json = JsonConvert.Serialize(data);
            var data2 = JsonConvert.Deserialize<DataListNull>(json);
            UtilFramework.Assert(!json.Contains("List"));
            UtilFramework.Assert(data.List != null);
            UtilFramework.Assert(data.List2 != null);
            UtilFramework.Assert(data2.List != null);
            UtilFramework.Assert(data2.List2 != null);
            UtilFramework.Assert(data.Name == "H");
            UtilFramework.Assert(data2.Name == "H");
        }

        public void Test15()
        {
            var data = new DataListNestedNull();
            data.List = new List<List<object>>();
            data.List.Add(new List<object>());
            data.List.Add(null);
            string json = JsonConvert.Serialize(data);
            var data2 = JsonConvert.Deserialize<DataListNestedNull>(json);
            UtilFramework.Assert(data2.List[1] != null);
        }

        public void Test16()
        {
            var data = new DataListNestedNull();
            data.List2 = new Dictionary<string, Dictionary<string, int>>();
            data.List2.Add("X", new Dictionary<string, int>());
            data.List2.Add("Y", null);
            string json = JsonConvert.Serialize(data);
            var data2 = JsonConvert.Deserialize<DataListNestedNull>(json);
            UtilFramework.Assert(data2.List2["Y"] != null);
        }

        public void Test17()
        {
            var data = new DataListNestedNull();
            data.List2 = new Dictionary<string, Dictionary<string, int>>();
            string json = JsonConvert.Serialize(data);
            var data2 = JsonConvert.Deserialize<DataListNestedNull>(json);
            UtilFramework.Assert(data2.List2.Count == 0);
        }

        public class DataDictionaryKey
        {
            public Dictionary<object, object> List;
        }

        public void Test18()
        {
            var data = new DataDictionaryKey();
            data.List = new Dictionary<object, object>();
            data.List["A"] = "X";
            data.List["D"] = null;
            string json = JsonConvert.Serialize(data);
            var data2 = JsonConvert.Deserialize<DataDictionaryKey>(json);
            UtilFramework.Assert((string)data2.List["A"] == "X");
            UtilFramework.Assert(data2.List["D"] == null);
        }

        public void Test19()
        {
            var data = new DataDictionaryKey();
            data.List = new Dictionary<object, object>();
            data.List["A"] = "X";
            data.List[8] = null;
            try
            {
                string json = JsonConvert.Serialize(data);
            }
            catch (Framework.Json.JsonException exception)
            {
                UtilFramework.Assert(exception.Message == "Dictionary key needs to be of type string!");
            }
        }

        public class DataDerivedDictionary0
        {
            public MyDictionary<string, int> List;

            public int SelectIndex2;
        }

        public void Test20()
        {
            var data = new DataDerivedDictionary0();
            data.List = new MyDictionary<string, int>();
            data.List.Add("SelectIndex3", 46);
            data.List.SelectIndex = 43;
            data.SelectIndex2 = 44;
            try
            {
                string json = JsonConvert.Serialize(data);
            }
            catch (Framework.Json.JsonException exception)
            {
                UtilFramework.Assert(exception.Message == "No derived list or dictionary for json!");
            }
        }

        public class DataDerivedList
        {
            /// <summary>
            /// (Row, Cell)
            /// </summary>
            public List<Row<GridCell>> CellList;
        }

        public class Row<T> : List<T>
        {
            public int SelectIndex;
        }

        public class GridCell
        {
            public object Value;
        }

        public void Test21()
        {
            var data = new DataDerivedList();
            data.CellList = new List<Json.UnitTest.Row<Json.UnitTest.GridCell>>();
            var row0 = new Row<GridCell>();
            row0.SelectIndex = 45;
            row0.Add(new Json.UnitTest.GridCell() { Value = "V" });
            row0.Add(null);
            data.CellList.Add(row0);
            try
            {
                string json = JsonConvert.Serialize(data);
            }
            catch (Framework.Json.JsonException exception)
            {
                UtilFramework.Assert(exception.Message == "No derived list or dictionary for json!");
            }
        }

        public class DataDerivedDictionary
        {
            /// <summary>
            /// (Row, Cell)
            /// </summary>
            public List<MyDictionary<string, int>> List;
        }

        public class MyDictionary<TKey, TValue> : Dictionary<TKey, TValue>
        {
            public int SelectIndex;
        }

        public void Test22()
        {
            var data = new DataDerivedDictionary();
            data.List = new List<Json.UnitTest.MyDictionary<string, int>>();
            var myList = new MyDictionary<string, int>();
            myList.SelectIndex = 33;
            myList.Add("X", 22);
            data.List.Add(myList);
            data.List.Add(null);
            data.List.Add(new Json.UnitTest.MyDictionary<string, int>());
            try
            {
                string json = JsonConvert.Serialize(data);
            }
            catch (Framework.Json.JsonException exception)
            {
                UtilFramework.Assert(exception.Message == "No derived list or dictionary for json!");
            }
        }

        public class DataBool
        {
            public object H;
        }

        public void Test23()
        {
            var data = new DataBool();
            data.H = true;
            string json = JsonConvert.Serialize(data);
            var data2 = JsonConvert.Deserialize<DataBool>(json);
            UtilFramework.Assert((bool)data2.H == true);
        }

        public class DataSelector
        {
            public string Type;

            public Label L;
        }

        public class Label
        {

        }

        public class MyLabel : Label
        {
            public MyLabel()
            {
                Type = "Label";
            }

            public string Type;
        }

        public void Test24()
        {
            var data = new DataSelector();
            data.L = new MyLabel();
            try
            {
                string json = JsonConvert.Serialize(data);
            }
            catch (Framework.Json.JsonException exception)
            {
                UtilFramework.Assert(exception.Message == "Object has no TypeCSharp field!");
            }
        }

        public class MyLabel2 : Label
        {
            public MyLabel2()
            {
                Type = "Label";
            }

            public string Type;

            public string TypeCSharp;
        }

        public void Test25()
        {
            var data = new DataSelector();
            data.L = new MyLabel2();
            string json = JsonConvert.Serialize(data);
            var data2 = JsonConvert.Deserialize<DataSelector>(json);
            UtilFramework.Assert(data2.L is MyLabel2);
        }

        public class Data26Root
        {
            public DifferentNamespace.Data26Base Data26;
        }

        public void Test26()
        {
            {
                var data = new Data26Root();
                data.Data26 = new DifferentNamespace.Data26();
                data.Data26.Name = "My26";
                string json = JsonConvert.Serialize(data, typeof(DifferentNamespace.Data));
                try
                {
                    var data2 = JsonConvert.Deserialize<Data26Root>(json);
                    UtilFramework.Assert(data2.Data26.Name == "My26");
                }
                catch (Exception exception)
                {
                    UtilFramework.Assert(exception.Message == "Type not found!");
                }
            }
            {
                var data = new Data26Root();
                data.Data26 = new DifferentNamespace.Data26();
                data.Data26.Name = "My26";
                string json = JsonConvert.Serialize(data, typeof(DifferentNamespace.Data));
                var data2 = JsonConvert.Deserialize<Data26Root>(json, typeof(DifferentNamespace.Data));
                UtilFramework.Assert(data2.Data26.Name == "My26");
            }
            {
                var data = new Data26Root();
                data.Data26 = new DifferentNamespace.Data26();
                data.Data26.Name = "My26";
                ((DifferentNamespace.Data26)data.Data26).Data2 = new DifferentNamespace.Data26Root() { Text = "TypeType" };
                try
                {
                    string json = JsonConvert.Serialize(data, typeof(DifferentNamespace.Data));
                }
                catch (Exception exception)
                {
                    UtilFramework.Assert(exception.Message == "More than one type found!");
                }
            }
        }

        public class Data27
        {
            public string Field;

            public string Property { get; set; }
        }

        public void Test27()
        {
            var data = new Data27();
            data.Field = "Field2";
            data.Property = "Property2";
            string json = JsonConvert.Serialize(data);
            var data2 = JsonConvert.Deserialize<Data27>(json);
            UtilFramework.Assert(data2.Field == "Field2");
            UtilFramework.Assert(data2.Property == "Property2");
        }

        public class Data28
        {
            public Guid? GuidNullable;

            public Guid Guid;
        }

        public void Test28()
        {
            var data = new Data28();
            data.GuidNullable = Guid.NewGuid();
            data.Guid = Guid.NewGuid();
            string json = JsonConvert.Serialize(data);
            var data2 = JsonConvert.Deserialize<Data28>(json);
            UtilFramework.Assert(data.Guid == data2.Guid);
            UtilFramework.Assert(data.GuidNullable == data2.GuidNullable);
            UtilFramework.Assert(data.GuidNullable != data2.Guid);
        }

        public enum MyEnum
        {
            None = 0,
            One = 1,
            Two = 2
        }

        public class Data29
        {
            public MyEnum MyEnum;

            public MyEnum? MyEnumNullable;
        }

        public void Test29()
        {
            {
                var data = new Data29();
                data.MyEnum = MyEnum.One;
                data.MyEnumNullable = MyEnum.Two;
                string json = JsonConvert.Serialize(data);
                var data2 = JsonConvert.Deserialize<Data29>(json);
                UtilFramework.Assert(data.MyEnum == data2.MyEnum);
                UtilFramework.Assert(data.MyEnumNullable == data2.MyEnumNullable);
            }
            {
                var data = new Data29();
                data.MyEnumNullable = null;
                string json = JsonConvert.Serialize(data);
                var data2 = JsonConvert.Deserialize<Data29>(json);
                UtilFramework.Assert(data.MyEnumNullable == data2.MyEnumNullable);
            }
        }
    }
}

namespace UnitTest.Json.DifferentNamespace
{
    public class Data
    {

    }

    public class Data26Base
    {
        public string Name;
    }

    public class Data26 : Data26Base
    {
        public string Type;

        public UnitTest.Data26Root Data2;
    }

    public class Data26Root : UnitTest.Data26Root
    {
        public string Text;

        public string Type;
    }
}