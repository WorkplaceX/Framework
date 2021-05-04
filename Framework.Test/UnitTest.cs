namespace Framework.Test
{
    using Framework.Doc;
    using Database.dbo;
    using Framework.DataAccessLayer;
    using Framework.Json;
    using Framework.Json.Bootstrap;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Text;

    public static class UnitTest
    {
        public static void Run()
        {
            // Test Md
            {
                // UtilDoc.Debug();
                UnitTestMd.Run();
                // UnitTestMd.RunRandom();
            }
            {
                MyHideComponent component = new MyHideComponent(null);
                component.DtoList = new List<MyHideDto>();
                component.DtoList.Add(new MyHideDto { Text = "DtoInList", IsHide = false });
                component.DtoList.Add(new MyHideDto { Text = "DtoInList", IsHide = true });
                component.DtoList.Add(new MyHideDto { Text = "DtoInList", IsHide = false });
                component.Dto = new MyHideDto { Text = "DtoInField", IsHide = false };
                UtilJson.Serialize(component, out var jsonSession, out var jsonClient);
                UtilFramework.Assert(Regex.Matches(jsonSession, "DtoInList").Count == 3);
                UtilFramework.Assert(Regex.Matches(jsonSession, "DtoInField").Count == 1);
                UtilFramework.Assert(Regex.Matches(jsonClient, "DtoInList").Count == 2);
                UtilFramework.Assert(Regex.Matches(jsonClient, "DtoInField").Count == 1);
            }
            {
                MyHideComponent component = new MyHideComponent(null) { Text = "Parent" };
                var component2 = new MyHideComponent(component) { Text = "Child" };
                component.Ref = component2;
                UtilJson.Serialize(component, out var jsonSession, out var jsonClient);
            }
            {
                UtilFramework.CamelCase camelCase = new UtilFramework.CamelCase("AbcDef");
                UtilFramework.Assert(camelCase.TextList[0] == "Abc");
                UtilFramework.Assert(camelCase.TextList[1] == "Def");
            }
            {
                UtilFramework.CamelCase camelCase = new UtilFramework.CamelCase("abcDef");
                UtilFramework.Assert(camelCase.TextList[0] == "abc");
                UtilFramework.Assert(camelCase.TextList[1] == "Def");
            }
            {
                UtilFramework.CamelCase camelCase = new UtilFramework.CamelCase("AbcDefCSharp");
                UtilFramework.Assert(camelCase.TextList[0] == "Abc");
                UtilFramework.Assert(camelCase.TextList[1] == "Def");
                UtilFramework.Assert(camelCase.TextList[2] == "CSharp");
                UtilFramework.Assert(camelCase.EndsWith("DefCSharp"));
                UtilFramework.Assert(camelCase.EndsWith("cDefCSharp") == false);
                UtilFramework.Assert(camelCase.EndsWith("AbcDefCSharp"));
                UtilFramework.Assert(camelCase.EndsWith("AbcDefCSharpCar") == false);
                UtilFramework.Assert(camelCase.EndsWith("CarAbcDefCSharp") == false);
                UtilFramework.Assert(camelCase.EndsWith("") == true);
            }
            {
                UtilFramework.CamelCase camelCase = new UtilFramework.CamelCase("AbcDefCSharp");
                UtilFramework.Assert(camelCase.StartsWith("Abc"));
                UtilFramework.Assert(camelCase.StartsWith("AbcDef"));
                UtilFramework.Assert(camelCase.StartsWith("AbcDefCSharp"));
                UtilFramework.Assert(camelCase.StartsWith("AbcDefCShar") == false);
                UtilFramework.Assert(camelCase.StartsWith("AbcDefCSharpLk") == false);
                UtilFramework.Assert(camelCase.StartsWith("LkAbcDefCSharp") == false);
                UtilFramework.Assert(camelCase.StartsWith(""));
            }
            {
                UtilFramework.CamelCase camelCase = new UtilFramework.CamelCase("ImageFileId");
                UtilFramework.Assert(camelCase.EndsWith("FileId"));
            }
            { 
                UtilFramework.CamelCase camelCase = new UtilFramework.CamelCase("ImagEFileId");
                UtilFramework.Assert(camelCase.EndsWith("FileId") == false);
            }
            {
                var source = new AppMain();

                // Serialize, deserialize
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                var dest = (AppMain)UtilJson.Deserialize(jsonSession);
            }
            {
                var source = new MyApp();
                source.Div = new Div(source);
                source.Div.ComponentRemove();

                // Serialize, deserialize
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                var dest = (MyApp)UtilJson.Deserialize(jsonSession);
            }
            {
                var source = new MyApp();
                source.Row = new BootstrapRow(source);
                source.Col = new BootstrapCol((BootstrapRow)source.Row);

                // Serialize, deserialize
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                var dest = (MyApp)UtilJson.Deserialize(jsonSession);

                UtilFramework.Assert(!jsonSession.Contains("PropertyReadOnly"));
            }
            {
                var source = new MyApp();

                // Serialize, deserialize
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                var dest = (MyApp)UtilJson.Deserialize(jsonSession);

                UtilFramework.Assert(!jsonSession.Contains("PropertyReadOnly"));
            }
            {
                var source = new MyApp(); 

                // Serialize, deserialize
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                var dest = (MyApp)UtilJson.Deserialize(jsonSession);

                UtilFramework.Assert(!jsonSession.Contains("PropertyReadOnly"));
            }
            {
                var source = new MyApp();
                var myGrid = new MyGrid(source) { Text = "K7", IsHide = true };
                source.MyCell = new MyCell { MyGridBoth = myGrid, MyText = "7755" };

                // Serialize, deserialize
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                var dest = (MyApp)UtilJson.Deserialize(jsonSession);

                UtilFramework.Assert(UtilFramework.FindCount(jsonClient, "K7") == 1);
                UtilFramework.Assert(UtilFramework.FindCount(jsonSession, "K7") == 1); // Ensure session stores reference
                UtilFramework.Assert(dest.List[0] == dest.MyCell.MyGridBoth);
            }
            {
                var source = new MyApp();
                var myGrid = new MyGrid(source) { Text = "K7", IsHide = true };
                var myGrid2 = new MyGrid(source) { Text = "K8", IsHide = true };
                source.MyCell = new MyCell { MyGrid = myGrid, MyGrid2 = myGrid2, MyText = "7755" };

                // Serialize, deserialize
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                var dest = (MyApp)UtilJson.Deserialize(jsonSession);

                UtilFramework.Assert(jsonClient.Contains("K7"));
                UtilFramework.Assert(!jsonClient.Contains("K8"));
                UtilFramework.Assert(dest.List[1] == dest.MyCell.MyGrid2);
            }
            RunComponentJson();
            {
                A source = new A();
                source.MyEnum = MyEnum.Left;

                // Serialize, deserialize
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                A dest = (A)UtilJson.Deserialize(jsonSession);

                UtilFramework.Assert(dest.MyEnum == MyEnum.Left);
                UtilFramework.Assert(dest.MyEnumNullable == null);
            }
            {
                A source = new A();
                source.MyEnumNullable = MyEnum.None;

                // Serialize, deserialize
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                A dest = (A)UtilJson.Deserialize(jsonSession);
                UtilFramework.Assert(dest.MyEnumNullable == MyEnum.None);
            }
            {
                A source = new A();

                // Serialize, deserialize
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                A dest = (A)UtilJson.Deserialize(jsonSession);

                UtilFramework.Assert(!jsonSession.Contains(nameof(A.MyEnumList)));
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
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                A dest = (A)UtilJson.Deserialize(jsonSession);

                UtilFramework.Assert(jsonSession.Contains(nameof(A.MyEnumList)));
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
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                A dest = (A)UtilJson.Deserialize(jsonSession);

                UtilFramework.Assert(jsonSession.Contains(nameof(A.MyEnumNullableList)));
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
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                A dest = (A)UtilJson.Deserialize(jsonSession);

                UtilFramework.Assert(jsonSession.Contains(nameof(A.IntNullableList)));
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
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                A dest = (A)UtilJson.Deserialize(jsonSession);

                UtilFramework.Assert(jsonSession.Contains(nameof(A.IntList)));
                UtilFramework.Assert(dest.IntList[0] == 0);
                UtilFramework.Assert(dest.IntList[1] == 1);
                UtilFramework.Assert(dest.IntList[2] == 2);
            }
            {
                A source = new A();
                source.V = 33;

                // Serialize, deserialize
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                A dest = (A)UtilJson.Deserialize(jsonSession);

                UtilFramework.Assert((int)dest.V == 33);
            }
            {
                A source = new A();
                source.V = "Hello";

                // Serialize, deserialize
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                A dest = (A)UtilJson.Deserialize(jsonSession);

                UtilFramework.Assert((string)dest.V == "Hello");
            }
            {
                var date = DateTime.Now;
                A source = new A();
                source.Row = new FrameworkDeployDb { Id = 22, FileName = @"C:\Temp\Readme.txt", Date = date };

                // Serialize, deserialize
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                A dest = (A)UtilJson.Deserialize(jsonSession);

                UtilFramework.Assert(dest.Row.Id == 22);
                UtilFramework.Assert(dest.Row.FileName == @"C:\Temp\Readme.txt");
                UtilFramework.Assert(dest.Row.Date ==date);
            }
            {
                A source = new A();
                source.V = MyEnum.None; // TODO Serialize enum on property of type object.

                // Serialize, deserialize
                // string json = UtilJson.Serialize(source);
                // A dest = (A)UtilJson.Deserialize(json);
            }
        }

        private static void RunComponentJson()
        {
            // Reference to self
            {
                MyComponent source = new MyComponent(null);
                source.Component = source;
                UtilJson.Serialize(source, out string json, out string jsonClient);
                MyComponent dest = (MyComponent)UtilJson.Deserialize(json);
                UtilFramework.Assert(dest.Component == dest);
            }
            // ComponentJson reference to ComponentJson do not send to client
            {
                MyComponent source = new MyComponent(null);
                source.HtmlAbc = new Html(source) { TextHtml = "JK" };
                source.MyTextSession = "SessionValueX";
                source.MyTextClient = "ClientValueX";
                source.MyIgnore = "IgnoreX";
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                MyComponent dest = (MyComponent)UtilJson.Deserialize(jsonSession);
                UtilFramework.Assert(!jsonClient.Contains("HtmlAbc")); // Do not send property name of ComponentJson reference to client

                UtilFramework.Assert(jsonSession.Contains("SessionValueX"));
                UtilFramework.Assert(!jsonClient.Contains("SessionValueX"));

                UtilFramework.Assert(!jsonSession.Contains("ClientValueX"));
                UtilFramework.Assert(jsonClient.Contains("ClientValueX"));

                UtilFramework.Assert(!jsonSession.Contains("IgnoreX"));
                UtilFramework.Assert(!jsonClient.Contains("IgnoreX"));

                UtilFramework.Assert(!jsonSession.Contains("Owner"));
                UtilFramework.Assert(!jsonClient.Contains("Owner"));
            }
            // ComponentJson.IsHide
            {
                MyComponent source = new MyComponent(null);
                new Html(source) { TextHtml = "X11" };
                new Html(source) { TextHtml = "X12", IsHide = true };
                new Html(source) { TextHtml = "X13" };
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                MyComponent dest = (MyComponent)UtilJson.Deserialize(jsonSession);
                UtilFramework.Assert(dest.List.Count == 3);
                UtilFramework.Assert(jsonClient.Contains("X11"));
                UtilFramework.Assert(!jsonClient.Contains("X12"));
                UtilFramework.Assert(jsonClient.Contains("X13"));
            }
            // ComponentJson.IsHide (Dto to ComponentJson
            {
                My source = new My();
                source.MyComponent = new MyComponent(null) { Id = 789, IsHide = true };
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                My dest = (My)UtilJson.Deserialize(jsonSession);
                UtilFramework.Assert(!jsonClient.Contains("789"));
            }
            // ComponentJson.IsHide
            {
                MyComponent source = new MyComponent(null);
                source.Html = new Html(source) { TextHtml = "My123", IsHide = true };
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                MyComponent dest = (MyComponent)UtilJson.Deserialize(jsonSession);
                UtilFramework.Assert(jsonSession.Contains("My123"));
                UtilFramework.Assert(!jsonClient.Contains("My123"));
            }
            // ComponentJson.IsHide (Root)
            {
                MyComponent source = new MyComponent(null);
                source.IsHide = true;
                source.Html = new Html(source) { TextHtml = "My123", IsHide = true };
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                MyComponent dest = (MyComponent)UtilJson.Deserialize(jsonSession);
                UtilFramework.Assert(jsonSession.Contains("My123"));
                UtilFramework.Assert(jsonClient == "");
            }
            // Reference to Row
            {
                MyComponent source = new MyComponent(null);
                source.MyRow = new MyRow { Text = "My123", DateTime = DateTime.Now };
                source.MyRowList = new List<Row>();
                source.MyRowList.Add(new MyRow { Text = "My1234", DateTime = DateTime.Now });
                source.MyRowList.Add(new MyRow { Text = "My12356", DateTime = DateTime.Now });
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                MyComponent dest = (MyComponent)UtilJson.Deserialize(jsonSession);
                UtilFramework.Assert(!jsonClient.Contains("My123"));
            }
            // Reference to Row
            {
                MyComponent source = new MyComponent(null);
                source.MyRowList = new List<Row>();
                source.MyRowList.Add(new MyRow { Text = "My1234", DateTime = DateTime.Now });
                source.MyRowList.Add(new MyRow { Text = "My12356", DateTime = DateTime.Now });
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                MyComponent dest = (MyComponent)UtilJson.Deserialize(jsonSession);
                UtilFramework.Assert(!jsonClient.Contains("My123"));
            }
            // Field of object type with Row value
            {
                MyComponent source = new MyComponent(null);
                source.V = new MyRow() { Text = "Hello" };
                try
                {
                    UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                }
                catch (Exception exception)
                {
                    UtilFramework.Assert(exception.Message == "Can not send data row to client!"); // V is object declaration therefore no Row detection.
                }
            }
            // Reference to removed ComponentJson
            {
                MyComponent source = new MyComponent(null);
                var html = new Html(source) { TextHtml = "My" };
                source.Html = html;
                html.ComponentRemove();
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                MyComponent dest = (MyComponent)UtilJson.Deserialize(jsonSession);
                UtilFramework.Assert(dest.Html == null);
            }
            // ComponentJson reference in list
            {
                MyComponent source = new MyComponent(null);
                var html = new Html(source) { TextHtml = "My" };
                source.HtmlList = new List<Html>();
                source.HtmlList.Add(html);
                // Serialize, deserialize
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                try
                {
                    var dest = (MyComponent)UtilJson.Deserialize(jsonSession);
                }
                catch (Exception exception)
                {
                    UtilFramework.Assert(exception.Message == "Reference to ComponentJson in List not supported!");
                }
            }
            {
                MyComponent source = new MyComponent(null);
                new MyComponent(source);
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                var dest = (MyComponent)UtilJson.Deserialize(jsonSession);
                UtilFramework.Assert(dest.List.Count == 1);
            }
            {
                MyComponent source = new MyComponent(null);
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                var dest = (MyComponent)UtilJson.Deserialize(jsonSession);
                UtilFramework.Assert(dest.Index == null);
            }
            {
                MyComponent source = new MyComponent(null);
                source.Index = 0;
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                var dest = (MyComponent)UtilJson.Deserialize(jsonSession);
                UtilFramework.Assert(dest.Index == 0);
            }
            {
                MyComponent source = new MyComponent(null);
                source.Index = -1;
                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                var dest = (MyComponent)UtilJson.Deserialize(jsonSession);
                UtilFramework.Assert(dest.Index == -1);
            }
            {
                My source = new My();
                var myComponent1 = new MyComponent(null);
                Html html1 = new Html(myComponent1) { TextHtml = "A" };
                myComponent1.Dto = new Dto { Css = "A", Html = html1 };
                var myComponent2 = new MyComponent(null);
                Html html2 = new Html(myComponent2) { TextHtml = "B" };
                myComponent2.Dto = new Dto { Css = "B", Html = html2 };
                source.List.Add(myComponent1);

                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                var dest = (My)UtilJson.Deserialize(jsonSession);
                dest.List[0].Dto.Html.TextHtml = "abc";
                UtilFramework.Assert(((Html)dest.List[0].List[0]).TextHtml == "abc");

                source.List.Add(myComponent2);
                try
                {
                    UtilJson.Serialize(source, out jsonSession, out jsonClient);
                }
                catch (Exception exception)
                {
                    UtilFramework.Assert(exception.Message == "JsonClient can only have one ComponentJson graph!");
                }
            }
            {
                My source = new My();
                var myComponent1 = new MyComponent(null);
                Html html1 = new Html(myComponent1) { TextHtml = "A" };
                myComponent1.Dto = new Dto { Css = "A", Html = html1 };
                var myComponent2 = new MyComponent(null);
                Html html2 = new Html(myComponent2) { TextHtml = "B" };
                myComponent2.Dto = new Dto { Css = "B", Html = html2 }; 
                var myComponent3 = new MyComponent(myComponent1);
                source.List.Add(myComponent3); // Reference not to root!
                try
                {
                    UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                }
                catch (Exception exception)
                {
                    UtilFramework.Assert(exception.Message == "Referenced ComponentJson not root!");
                }
                source.List.Remove(myComponent3);
                source.List.Add(myComponent1);
                source.List.Add(myComponent2);
                try
                {
                    UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                }
                catch (Exception exception)
                {
                    UtilFramework.Assert(exception.Message == "JsonClient can only have one ComponentJson graph!");
                }
            }
            {
                My source = new My();
                var myComponent1 = new MyComponent(null);
                Html html1 = new Html(myComponent1) { TextHtml = "A" };
                myComponent1.Dto = new Dto { Css = "A", Html = html1 };
                var myComponent2 = new MyComponent(null);
                Html html2 = new Html(myComponent2) { TextHtml = "B" };
                myComponent2.Dto = new Dto { Css = "B", Html = html1 }; // Reference to object in different graph
                source.List.Add(myComponent2);
                source.List.Add(myComponent1);
                try
                {
                    UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                }
                catch (Exception exception)
                {
                    UtilFramework.Assert(exception.Message == "Referenced ComponentJson not in same object graph!");
                }
            }
            {
                var source = new MyComponent(null);

                source.Html = new Html(source) { TextHtml = "Hello" };

                UtilJson.Serialize(source, out string jsonSession, out string jsonClient);

                UtilFramework.Assert(!jsonSession.Contains("Owner"));

                var dest = (MyComponent)UtilJson.Deserialize(jsonSession);

                var htmlOne = dest.Html;
                var htmlTwo = dest.List.OfType<Html>().First();

                htmlOne.TextHtml = "K";
                UtilFramework.Assert(htmlOne.TextHtml == htmlTwo.TextHtml);
            }
            // Referenced ComponentJson not in same graph
            {
                var source = new MyComponent(null);
                source.Html = new Html(null);

                try
                {
                    UtilJson.Serialize(source, out string jsonSession, out string jsonClient);
                }
                catch (Exception exception)
                {
                    UtilFramework.Assert(exception.Message == "Referenced ComponentJson not in same object graph!");
                }
            }
        }
    }

    public static class UnitTestMd
    {
        public static void Run()
        {
            List<Item> list = new List<Item>();
            list.Add(new Item { TextMd = "Text", TextHtml = "<section>(page)<p>(p)Text(/p)</p>(/page)</section>" });
            list.Add(new Item { TextMd = "# Title", TextHtml = "<section>(page)<h1>Title</h1>(/page)</section>" });
            list.Add(new Item { TextMd = "Hello\r\n\r\nWorld", TextHtml = "<section>(page)<p>(p)Hello(/p)</p><p>(p)World(/p)</p>(/page)</section>" });
            list.Add(new Item { TextMd = "Hello\r\nWorld", TextHtml = "<section>(page)<p>(p)HelloWorld(/p)</p>(/page)</section>" });
            list.Add(new Item { TextMd = "Hello\r\n\r\nWorld", TextHtml = "<section>(page)<p>(p)Hello(/p)</p><p>(p)World(/p)</p>(/page)</section>" });
            list.Add(new Item { TextMd = "![My Cli](https://workplacex.org/Doc/Cli.png)", TextHtml = "<section>(page)<img src=\"https://workplacex.org/Doc/Cli.png\" alt=\"My Cli\" />(/page)</section>" });
            list.Add(new Item { TextMd = "![](https://workplacex.org/Doc/Cli.png)", TextHtml = "<section>(page)<img src=\"https://workplacex.org/Doc/Cli.png\" alt=\"https://workplacex.org/Doc/Cli.png\" />(/page)</section>" });
            list.Add(new Item { TextMd = "\r\n\r\n![](https://workplacex.org/Doc/Cli.png)a", TextHtml = "<section>(page)<p>(p)(/p)</p><img src=\"https://workplacex.org/Doc/Cli.png\" alt=\"https://workplacex.org/Doc/Cli.png\" /><p>(p)a(/p)</p>(/page)</section>" });
            list.Add(new Item { TextMd = "\r\n![](https://workplacex.org/Doc/Cli.png)a", TextHtml = "<section>(page)<p>(p)(/p)</p><img src=\"https://workplacex.org/Doc/Cli.png\" alt=\"https://workplacex.org/Doc/Cli.png\" /><p>(p)a(/p)</p>(/page)</section>" });
            list.Add(new Item { TextMd = "\r\n![](https://workplacex.org/Doc/Cli.png)\r\n", TextHtml = "<section>(page)<p>(p)(/p)</p><img src=\"https://workplacex.org/Doc/Cli.png\" alt=\"https://workplacex.org/Doc/Cli.png\" /><p>(p)(/p)</p>(/page)</section>" });
            list.Add(new Item { TextMd = "\r\n![](https://workplacex.org/Doc/Cli.png)\r\nT", TextHtml = "<section>(page)<p>(p)(/p)</p><img src=\"https://workplacex.org/Doc/Cli.png\" alt=\"https://workplacex.org/Doc/Cli.png\" /><p>(p)T(/p)</p>(/page)</section>" });
            list.Add(new Item { TextMd = "**Bold**Text", TextHtml = "<section>(page)<p>(p)<strong>Bold</strong>Text(/p)</p>(/page)</section>" });
            list.Add(new Item { TextMd = "* One\r\n* Two", TextHtml = "<section>(page)<ul><li>One</li><li>Two</li></ul>(/page)</section>" });
            list.Add(new Item { TextMd = "\r\n* One\r\n* Two", TextHtml = "<section>(page)<p>(p)(/p)</p><ul><li>One</li><li>Two</li></ul>(/page)</section>" });
            list.Add(new Item { TextMd = "\r\n* One\r\n1\r\n* Two", TextHtml = "<section>(page)<p>(p)(/p)</p><ul><li>One1</li><li>Two</li></ul>(/page)</section>" });
            list.Add(new Item { TextMd = "\r\n* One\r\n1\r\n\r\n* Two", TextHtml = "<section>(page)<p>(p)(/p)</p><ul><li>One1</li></ul><p>(p)(/p)</p><ul><li>Two</li></ul>(/page)</section>" });
            list.Add(new Item { TextMd = "* A\r\nB", TextHtml = "<section>(page)<ul><li>AB</li></ul>(/page)</section>" });
            list.Add(new Item { TextMd = "# A\r\nB", TextHtml = "<section>(page)<h1>A</h1><p>(p)B(/p)</p>(/page)</section>" });
            list.Add(new Item { TextMd = "* A\r\nB", TextHtml = "<section>(page)<ul><li>AB</li></ul>(/page)</section>" });
            list.Add(new Item { TextMd = "* A\r\n\r\nB", TextHtml = "<section>(page)<ul><li>A</li></ul><p>(p)B(/p)</p>(/page)</section>" });
            list.Add(new Item { TextMd = "\r\n\r\nB\r\n\r\n# T\r\nD", TextHtml = "<section>(page)<p>(p)B(/p)</p><p>(p)(/p)</p><h1>T</h1><p>(p)D(/p)</p>(/page)</section>" });
            list.Add(new Item { TextMd = "\r\nB\r\n\r\n# T\r\nD", TextHtml = "<section>(page)<p>(p)B(/p)</p><p>(p)(/p)</p><h1>T</h1><p>(p)D(/p)</p>(/page)</section>" });
            list.Add(new Item { TextMd = "B\r\n\r\n# T\r\nD", TextHtml = "<section>(page)<p>(p)B(/p)</p><p>(p)(/p)</p><h1>T</h1><p>(p)D(/p)</p>(/page)</section>" });
            list.Add(new Item { TextMd = "\r\n\r\n```cmd\r\ncd\r\n```\r\n", TextHtml = "<section>(page)<p>(p)(/p)</p><pre><code class=\"language-cmd\">\r\ncd\r\n</code></pre><p>(p)(/p)</p>(/page)</section>" });
            list.Add(new Item { TextMd = "# T\n(Note)\nHello\n(Note)", TextHtml = "<section>(page)<h1>T</h1><p>(p)(/p)</p><article class=\"message is-info\"><div class=\"message-body\"><p>(p)Hello(/p)</p></div></article>(/page)</section>" });
            list.Add(new Item { TextMd = "\r\n\r\n(Note)\r\nHello\r\n(Note)", TextHtml = "<section>(page)<p>(p)(/p)</p><article class=\"message is-info\"><div class=\"message-body\"><p>(p)Hello(/p)</p></div></article>(/page)</section>" });
            list.Add(new Item { TextMd = "(Note)\r\nD\r\n\r\nE\r\n(Note)", TextHtml = "<section>(page)<article class=\"message is-info\"><div class=\"message-body\"><p>(p)D(/p)</p><p>(p)E(/p)</p></div></article>(/page)</section>" });
            list.Add(new Item { TextMd = "Hello<!-- Comment -->World", TextHtml = "<section>(page)<p>(p)Hello<!-- Comment -->World(/p)</p>(/page)</section>" });
            list.Add(new Item { TextMd = "* Hello<!-- Comment -->World", TextHtml = "<section>(page)<ul><li>Hello<!-- Comment -->World</li></ul>(/page)</section>" });
            list.Add(new Item { TextMd = "(Note)Hello(Note)", TextHtml = "<section>(page)<p>(p)(Note)Hello(Note)(/p)</p>(/page)</section>" });
            list.Add(new Item { TextMd = "(Note)Hello\r\n(Note)", TextHtml = "<section>(page)<article class=\"message is-info\"><div class=\"message-body\"><p>(p)Hello(/p)</p></div></article>(/page)</section>" });
            list.Add(new Item { TextMd = "(Note)Hello\r\n\r\nWorld\r\n(Note)", TextHtml = "<section>(page)<article class=\"message is-info\"><div class=\"message-body\"><p>(p)Hello(/p)</p><p>(p)World(/p)</p></div></article>(/page)</section>" });
            list.Add(new Item { TextMd = "Hello<!-- Comment -->", TextHtml = "<section>(page)<p>(p)Hello<!-- Comment -->(/p)</p>(/page)</section>" });
            list.Add(new Item { TextMd = "<!-- Comment -->", TextHtml = "<section>(page)<!-- Comment -->(/page)</section>" });
            list.Add(new Item { TextMd = "(Note)\r\nHello**Bold**\r\n(Note)", TextHtml = "<section>(page)<article class=\"message is-info\"><div class=\"message-body\"><p>(p)Hello<strong>Bold</strong>(/p)</p></div></article>(/page)</section>" });
            list.Add(new Item { TextMd = "(Note)\r\n**Note:**\r\n(Note)", TextHtml = "<section>(page)<article class=\"message is-info\"><div class=\"message-body\"><p>(p)<strong>Note:</strong>(/p)</p></div></article>(/page)</section>" });
            list.Add(new Item { TextMd = "(Note)\r\n\r\nA\r\n(Note)", TextHtml = "<section>(page)<article class=\"message is-info\"><div class=\"message-body\"><p>(p)A(/p)</p></div></article>(/page)</section>" });
            list.Add(new Item { TextMd = "(Note)\r\n\r\nHello\r\n\r\n# A\r\n# B\r\n(Note)", TextHtml = "<section>(page)<article class=\"message is-info\"><div class=\"message-body\"><p>(p)Hello(/p)</p><p>(p)(/p)</p></div></article><h1>A</h1><p>(p)(/p)</p><h1>B</h1><p>(p)(/p)</p>(/page)</section>" });
            list.Add(new Item { TextMd = "(Note)\r\n\r\n* A\r\n(Note)", TextHtml = "<section>(page)<article class=\"message is-info\"><div class=\"message-body\"><p>(p)(/p)</p></div></article><ul><li>A</li></ul>(/page)</section>" });
            list.Add(new Item { TextMd = "(Note)\r\n# X5\r\n**Bold**\r\n(Note)", TextHtml = "<section>(page)<article class=\"message is-info\"><div class=\"message-body\"></div></article><h1>X5</h1><p>(p)<strong>Bold</strong>(/p)</p>(/page)</section>" });
            list.Add(new Item { TextMd = "Hello\r\n(Page)World", TextHtml = "<section>(page)<p>(p)Hello(/p)</p>(/page)</section><section>(page)<p>(p)World(/p)</p>(/page)</section>" });
            list.Add(new Item { TextMd = "# Title\r\n(Page)", TextHtml = "<section>(page)<h1>Title</h1><p>(p)(/p)</p>(/page)</section><section>(page)(/page)</section>" });
            list.Add(new Item { TextMd = "*(*(", TextHtml = "<section>(page)<p>(p)<i>(</i>((/p)</p>(/page)</section>" });
            list.Add(new Item { TextMd = "# T\r\n(Note)\r\nHello\r\n(Note)\r\nWorld", TextHtml = "<section>(page)<h1>T</h1><p>(p)(/p)</p><article class=\"message is-info\"><div class=\"message-body\"><p>(p)Hello(/p)</p></div></article><p>(p)World(/p)</p>(/page)</section>" });
            list.Add(new Item { TextMd = "\r\n\r\nT", TextHtml = "<section>(page)<p>(p)T(/p)</p>(/page)</section>" });
            list.Add(new Item { TextMd = "(Note)**A**\r\n(Note)", TextHtml = "<section>(page)<article class=\"message is-info\"><div class=\"message-body\"><p>(p)<strong>A</strong>(/p)</p></div></article>(/page)</section>" });
            list.Add(new Item { TextMd = "https://my.my", TextHtml = "<section>(page)<p>(p)<a href=\"https://my.my\">https://my.my</a>(/p)</p>(/page)</section>" });
            list.Add(new Item { TextMd = "https://https://my.my", TextHtml = "<section>(page)<p>(p)<a href=\"https://\">https://</a><a href=\"https://my.my\">https://my.my</a>(/p)</p>(/page)</section>" });
            list.Add(new Item { TextMd = "https://www.youtube.com/watch?v=bYJTl5axgUY", TextHtml = "<section>(page)<p>(p)<a href=\"https://www.youtube.com/watch?v=bYJTl5axgUY\">https://www.youtube.com/watch?v=bYJTl5axgUY</a>(/p)</p>(/page)</section>" });
            list.Add(new Item { TextMd = "# T\r\nHello\r\n(Youtube Link=\"https://www.youtube.com/embed/bYJTl5axgUY\")\r\nWorld\r\n", TextHtml = "<section>(page)<h1>T</h1><p>(p)Hello(/p)</p><iframe src=\"https://www.youtube.com/embed/bYJTl5axgUY\"></iframe><p>(p)World(/p)</p>(/page)</section>" }); list.Add(new Item { TextMd = "[](Page)", TextHtml = "<section>(page)<p>(p)<a href=\"Page\">Page</a>(/p)</p>(/page)</section>" });
            list.Add(new Item { TextMd = "(Page)\r\n# T", TextHtml = "<section>(page)<p>(p)(/p)</p><h1>T</h1>(/page)</section>" });
            list.Add(new Item { TextMd = "(Page)\r\n# Title", TextHtml = "<section>(page)<p>(p)(/p)</p><h1>Title</h1>(/page)</section>" });
            list.Add(new Item { TextMd = "(Page Path=\"/\" Title=\"Hello\")\r\n# Title", TextHtml = "<section>(page)<p>(p)(/p)</p><h1>Title</h1>(/page)</section>" });
            list.Add(new Item { TextMd = "GitHub: **[ApplicationDemo](https://github.com/WorkplaceX/ApplicationDemo)**", TextHtml = "<section>(page)<p>(p)GitHub: <strong><a href=\"https://github.com/WorkplaceX/ApplicationDemo\">ApplicationDemo</a></strong>(/p)</p>(/page)</section>" });
            list.Add(new Item { TextMd = "# T1\r\n## T2", TextHtml = "<section>(page)<h1>T1</h1><p>(p)(/p)</p><h2>T2</h2>(/page)</section>" });

            var i = 0;
            foreach (var item in list)
            {
                var appDoc = new AppDoc();
                appDoc.Data.Registry.IsDebug = true;
                new MdPage(appDoc.MdDoc, item.TextMd);
                appDoc.Parse();
                var textHtml = appDoc.HtmlDoc.Render();

                UtilFramework.Assert(textHtml == item.TextHtml);

                i += 1;
            }

            // Page path
            {
                var textMd = "Hello\r\n(Page Path=\"/contact/\")\r\nWorld";

                var appDoc = new AppDoc();
                appDoc.Data.Registry.IsDebug = true;
                new MdPage(appDoc.MdDoc, textMd);
                appDoc.Parse();

                var path = ((SyntaxPage)((HtmlPage)appDoc.HtmlDoc.List[1]).Syntax).PagePath;
                UtilFramework.Assert(path == "/contact/");
            }
        }

        public static void RunRandom()
        {
            List<string> list = new List<string>();
            list.Add("\r\n");
            list.Add("\r\n\r\n");
            list.Add(" ");
            list.Add(" ");
            list.Add("A");
            list.Add("Text");
            list.Add("![]");
            list.Add("[]()");
            list.Add("[");
            list.Add("]");
            list.Add("(");
            list.Add(")");
            list.Add("=");
            list.Add(",");
            list.Add("#");
            list.Add("*");
            list.Add("(Note)");
            list.Add("```");
            list.Add("*");
            list.Add("**");
            list.Add("(Page)");
            list.Add("Page");
            list.Add("(Youtube)");
            list.Add("Youtube");
            list.Add("Link=\"");

            var random = new Random(546);

            for (int i = 0; i < 10000; i++)
            {
                var textMd = new StringBuilder();

                for (int c = 0; c < 50; c++)
                {
                    textMd.Append(list[random.Next(list.Count - 1)]);
                }

                var appDoc = new AppDoc();
                appDoc.Data.Registry.IsDebug = true;
                new MdPage(appDoc.MdDoc, textMd.ToString());
                appDoc.Parse();
                var textHtml = appDoc.HtmlDoc.Render();
            }
        }

        public class Item
        {
            public string TextMd { get; set; }

            public string TextHtml { get; set; }
        }
    }

    public class MyApp : ComponentJson
    {
        public MyApp() 
            : base(null, nameof(MyApp))
        {

        }

        public Div Div;

        public BootstrapCol Col;
        
        public DivContainer Row;

        public MyCell MyCell;

        public int PropertyReadOnly => 9;
    }

    public class MyCell
    {
        public string MyText;

        [Serialize(SerializeEnum.Client)]
        public MyGrid MyGrid;

        public MyGrid MyGrid2;

        [Serialize(SerializeEnum.Both)]
        public MyGrid MyGridBoth;
    }

    public class MyGrid : ComponentJson
    {
        public MyGrid(ComponentJson owner) 
            : base(owner, nameof(MyGrid))
        {

        }

        public string Text;
    }

    public class My
    {
        public List<MyComponent> List = new List<MyComponent>();

        public MyComponent MyComponent;
    }

    public class MyComponent : ComponentJson
    {
        public MyComponent(ComponentJson owner) 
            : base(owner, nameof(MyComponent))
        {

        }

        public Html Html;

        public Html HtmlAbc;

        [Serialize(SerializeEnum.Session)]
        public string MyTextSession;

        [Serialize(SerializeEnum.Client)]
        public string MyTextClient;

        [Serialize(SerializeEnum.None)]
        public string MyIgnore;

        public Dto Dto;

        public int? Index;

        public List<Html> HtmlList;

        public MyRow MyRow;

        public List<Row> MyRowList;

        public object V;

        public MyComponent Component;
    }

    public class Dto
    {
        public string Css;

        public Html Html;
    }

    public class MyRow : Row
    {
        public string Text { get; set; }

        public DateTime DateTime { get; set; }
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

        public FrameworkDeployDb Row;
    }

    public class AppMain : AppJson
    {
        public AppMain()
        {
            this.Row = new BootstrapRow(this);
            this.Col = new BootstrapCol(Row);
        }

        public BootstrapRow Row;

        public BootstrapCol Col;
    }

    public class MyHideComponent : ComponentJson
    {
        public MyHideComponent(ComponentJson owner) 
            : base(owner, nameof(MyHideComponent))
        {

        }

        public string Text;

        public List<MyHideDto> DtoList;

        public MyHideDto Dto;

        public ComponentJson Ref;
    }

    public class MyHideDto : IHide
    {
        public string Text;

        public bool IsHide { get; set; }
    }
}