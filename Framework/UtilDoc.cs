namespace Framework.Doc
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Component serialization, deserialization mapping.
    /// </summary>
    internal enum DataEnum
    {
        None,

        AppDoc,

        MdDoc,

        MdPage,

        MdSpace,

        MdParagraph,

        MdNewLine,

        MdComment,

        MdTitle,

        MdBullet,

        MdCode,

        MdImage,

        MdBracket,

        MdFont,

        MdLink,

        MdContent,

        SyntaxDoc,

        SyntaxPage,

        SyntaxComment,

        SyntaxTitle,

        SyntaxBullet,

        SyntaxCode,

        SyntaxFont,

        SyntaxLink,

        SyntaxImage,

        SyntaxPageBreak,

        SyntaxParagraph,

        SyntaxNewLine,

        SyntaxContent,

        SyntaxIgnore,

        HtmlDoc,

        HtmlPage,

        HtmlComment,

        HtmlTitle,
        
        HtmlParagraph,

        HtmlBulletList,

        HtmlBulletItem,

        HtmlFont,

        HtmlLink,

        HtmlImage,

        HtmlContent,

        // HtmlIgnore
    }

    /// <summary>
    /// Component data.
    /// </summary>
    internal sealed class DataDoc
    {
        public Registry Registry;

        public int Id { get; set; }

        public DataEnum DataEnum { get; set; }

        /// <summary>
        /// Gets Owner. Not serialized.
        /// </summary>
        public DataDoc Owner;

        /// <summary>
        /// Gets Index. This is the index of this object in Owner.List. Not serialized.
        /// </summary>
        public int Index;

        /// <summary>
        /// Gets List. Can be null if no children. See also method ListGet();
        /// </summary>
        public List<DataDoc> List { get; set; }

        public List<DataDoc> ListGet()
        {
            if (List == null)
            {
                return new List<DataDoc>();
            }
            else
            {
                return List;
            }
        }

        public int ListCount()
        {
            return List != null ? List.Count : 0;
        }

        public DataDoc Last(bool isOrDefault = false, int offset = 0)
        {
            DataDoc result = null;
            int index = ListCount() - 1 - offset;
            bool isOutOfRange = index < 0 || index > List.Count;
            if (isOrDefault == false)
            {
                result = List[index];
            }
            else
            {
                if (!isOutOfRange)
                {
                    result = List[index];
                }
            }

            return result;
        }

        private Component componentCache;

        public Component Component()
        {
            var result = componentCache;
            if (result == null)
            {
                result = Registry.Deserialize(this);
                componentCache = result;
            }
            return result;
        }

        public string Text { get; set; }

        public int IndexBegin { get; set; }

        public int IndexEnd { get; set; }

        public bool IsCommentEnd { get; set; }

        public int TitleLevel { get; set; }

        public MdBracketEnum BracketEnum { get; set; }

        public bool IsBracketEnd { get; set; }

        public MdFontEnum FontEnum { get; set; }

        public int TokenIdBegin { get; set; }

        public int TokenIdEnd { get; set; }

        public int? MdDocId { get; set; }

        public int? SyntaxDocOneId { get; set; }

        public int? SyntaxDocTwoId { get; set; }

        public int? HtmlDocId { get; set; }

        public int? SyntaxId { get; set; }

        public string Link { get; set; }

        public string LinkText { get; set; }

        public string CodeLanguage { get; set; }
    }

    /// <summary>
    /// Component registry.
    /// </summary>
    internal class Registry
    {
        public Registry()
        {
            // Doc
            Add(typeof(AppDoc));

            // Token
            Add(typeof(MdDoc));
            Add(typeof(MdPage));
            Add(typeof(MdSpace));
            Add(typeof(MdParagraph));
            Add(typeof(MdNewLine));
            Add(typeof(MdComment));
            Add(typeof(MdTitle));
            Add(typeof(MdBullet));
            Add(typeof(MdCode));
            Add(typeof(MdImage));
            Add(typeof(MdBracket));
            Add(typeof(MdFont));
            Add(typeof(MdLink));
            Add(typeof(MdContent));

            // Syntax
            Add(typeof(SyntaxDoc));
            Add(typeof(SyntaxPage));
            Add(typeof(SyntaxComment));
            Add(typeof(SyntaxTitle));
            Add(typeof(SyntaxCode));
            Add(typeof(SyntaxBullet));
            Add(typeof(SyntaxFont));
            Add(typeof(SyntaxLink));
            Add(typeof(SyntaxImage));
            Add(typeof(SyntaxPageBreak));
            Add(typeof(SyntaxParagraph));
            Add(typeof(SyntaxNewLine));
            Add(typeof(SyntaxContent));
            Add(typeof(SyntaxIgnore)); // This should be the last SyntaX

            // Html
            Add(typeof(HtmlDoc));
            Add(typeof(HtmlPage));
            Add(typeof(HtmlComment));
            Add(typeof(HtmlTitle));
            Add(typeof(HtmlParagraph));
            Add(typeof(HtmlBulletList));
            Add(typeof(HtmlBulletItem));
            Add(typeof(HtmlFont));
            Add(typeof(HtmlLink));
            Add(typeof(HtmlImage));
            Add(typeof(HtmlContent));
        }

        private void Add(Type type)
        {
            if (!Enum.TryParse<DataEnum>(type.Name, out var dataEnum))
            {
                throw new Exception(string.Format("Type not registered in enum! ({0}, {1})", nameof(DataEnum), type.Name));
            }
            List.Add(type);
            TypeList.Add(type, dataEnum);
            DataEnumList.Add(dataEnum, type);
        }

        /// <summary>
        /// (Type) Keeps sequence.
        /// </summary>
        public List<Type> List = new List<Type>();

        /// <summary>
        /// (Type, DataEnum)
        /// </summary>
        public Dictionary<Type, DataEnum> TypeList = new Dictionary<Type, DataEnum>();

        /// <summary>
        /// (DataEnum, Type)
        /// </summary>
        public Dictionary<DataEnum, Type> DataEnumList = new Dictionary<DataEnum, Type>();

        public int IdCount;

        /// <summary>
        /// (Id, Data)
        /// </summary>
        public Dictionary<int, DataDoc> IdList = new Dictionary<int, DataDoc>();

        /// <summary>
        /// Use in getter for component reference.
        /// </summary>
        public T ReferenceGet<T>(int? id) where T : Component
        {
            T result = null;
            if (id != null)
            {
                result = (T)IdList[id.Value].Component();
            }
            return result;
        }

        /// <summary>
        /// Use in setter for component reference.
        /// </summary>
        public static int? ReferenceSet(Component value)
        {
            return value?.Data.Id;
        }

        public Component Deserialize(DataDoc data)
        {
            var type = DataEnumList[data.DataEnum];
            var result = (Component)FormatterServices.GetUninitializedObject(type);
            result.Data = data;
            return result;
        }
    }

    /// <summary>
    /// Register new Component in Registry constructor.
    /// </summary>
    internal class Component
    {
        internal DataDoc Data;

        public Component(Component owner)
        {
            if (owner == null)
            {
                var registry = new Registry();
                var dataEnum = registry.TypeList[GetType()];
                Data = new DataDoc { Registry = registry, Id = registry.IdCount += 1, DataEnum = dataEnum };
                registry.IdList.Add(Data.Id, Data);
            }
            else
            {
                var registry = owner.Data.Registry;
                var dataEnum = registry.TypeList[GetType()]; // See also constructor Registry and enum DataEnum.
                Data = new DataDoc { Registry = registry, Id = registry.IdCount += 1, DataEnum = dataEnum, Owner = owner.Data };
                registry.IdList.Add(Data.Id, Data);
                if (owner.Data.List == null)
                {
                    owner.Data.List = new List<DataDoc>();
                }
                owner.Data.List.Add(Data);
                Data.Index = owner.Data.List.Count - 1;
            }
        }

        public Component Owner => Data.Owner?.Component();

        public IReadOnlyList<Component> List
        {
            get
            {
                var result = new List<Component>();
                if (Data.List != null)
                {
                    foreach (var item in Data.List)
                    {
                        result.Add(item.Component());
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// Returns next or previous component.
        /// </summary>
        /// <param name="componentBeginEnd">Can be null for no range check.</param>
        /// <param name="offset">For example 1 (next) or -1 (previous)</param>
        private Component Next(Component componentBeginEnd, int offset)
        {
            Component result = null;
            if (Data.Owner != null) // Not root
            {
                UtilDoc.Assert(Data.Owner.List[Data.Index] == Data); // Index check
                if (this != componentBeginEnd) // Reached not yet begin or end
                {
                    if (Data.Index + offset >= 0 && Data.Index + offset < Data.Owner.List.Count) // There is a next component
                    {
                        result = Data.Owner.List[Data.Index + offset].Component(); // Move next
                    }
                }
            }
            return result;
        }

        public T Next<T>(T componentEnd) where T : Component
        {
            var result = Next(componentEnd, offset: 1);
            return (T)result;
        }

        /// <summary>
        /// Returns true, if next component.
        /// </summary>
        public static bool Next<T>(ref T component, T componentEnd) where T : Component
        {
            var result = component?.Next(componentEnd);
            if (result != null)
            {
                component = result;
            }
            return result != null;
        }

        /// <summary>
        /// Returns previous component.
        /// </summary>
        /// <param name="componentBegin">Can be null for no range check.</param>
        public T Previous<T>(T componentBegin) where T : Component
        {
            var result = Next(componentBegin, offset: -1);
            return (T)result;
        }

        public void Serialize(out string json)
        {
            var option = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault }; // Do not serialize default values.
            json = JsonSerializer.Serialize(Data, option);
        }

        private static void Deserialize(Registry registry, DataDoc owner, int index, DataDoc data)
        {
            data.Registry = registry;
            data.Owner = owner;
            data.Index = index;
            registry.IdList.Add(data.Id, data);
            if (data.List != null)
            {
                for (int i = 0; i < data.List.Count; i++)
                {
                    Deserialize(registry, data, i, data.List[i]);
                }
            }
        }

        public static T Deserialize<T>(string json) where T : Component
        {
            var data = JsonSerializer.Deserialize<DataDoc>(json);
            var registry = new Registry();
            Deserialize(registry, null, -1, data);
            var result = registry.Deserialize(data);
            return (T)result;
        }
    }

    /// <summary>
    /// Store and parse (*.md) pages.
    /// </summary>
    internal class AppDoc : Component
    {
        public AppDoc()
            : base(null)
        {
            MdDoc = new MdDoc(this);
            SyntaxDocOne = new SyntaxDoc(this);
            SyntaxDocTwo = new SyntaxDoc(this);
            HtmlDoc = new HtmlDoc(this);
        }

        /// <summary>
        /// Parse md pages.
        /// </summary>
        public void Parse()
        {
            // Init registries
            var mdRegistry = new MdRegistry(Data.Registry);
            var syntaxRegistry = new SyntaxRegistry(Data.Registry);

            // Lexer
            foreach (MdPage page in MdDoc.List)
            {
                // Clear token list
                if (page.Data.ListCount() > 0)
                {
                    page.Data.List = new List<DataDoc>();
                }

                // Lexer
                mdRegistry.Parse(page);
            }

            syntaxRegistry.Parse(MdDoc, SyntaxDocOne, SyntaxDocTwo, HtmlDoc);
        }

        public MdDoc MdDoc
        {
            get => Data.Registry.ReferenceGet<MdDoc>(Data.MdDocId);
            set => Data.MdDocId = Registry.ReferenceSet(value);
        }

        public SyntaxDoc SyntaxDocOne
        {
            get => Data.Registry.ReferenceGet<SyntaxDoc>(Data.SyntaxDocOneId);
            set => Data.SyntaxDocOneId = Registry.ReferenceSet(value);
        }

        public SyntaxDoc SyntaxDocTwo
        {
            get => Data.Registry.ReferenceGet<SyntaxDoc>(Data.SyntaxDocTwoId);
            set => Data.SyntaxDocTwoId = Registry.ReferenceSet(value);
        }

        public HtmlDoc HtmlDoc
        {
            get => Data.Registry.ReferenceGet<HtmlDoc>(Data.HtmlDocId);
            set => Data.HtmlDocId = Registry.ReferenceSet(value);
        }
    }

    /// <summary>
    /// Span extensions.
    /// </summary>
    internal static class TextExtension
    {
        /// <summary>
        /// Returns char at index or null if out of range.
        /// </summary>
        public static char? Char(this ReadOnlySpan<char> text, int index)
        {
            char? result = null;
            if (index >= 0 && index < text.Length)
            {
                result = text.Slice(index, 1)[0];
            }
            return result;
        }

        public static bool StartsWith(this ReadOnlySpan<char> text, int index, string textFind)
        {
            return text.Slice(index).StartsWith(textFind, StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// Subset of Registry.
    /// </summary>
    internal class MdRegistry
    {
        public MdRegistry(Registry registry)
        {
            foreach (var type in registry.List)
            {
                if (type.IsSubclassOf(typeof(MdTokenBase)))
                {
                    var token = (MdTokenBase)Activator.CreateInstance(type);
                    List.Add(token);
                }
            }
        }

        /// <summary>
        /// (Token). Registry, factory mode.
        /// </summary>
        public List<MdTokenBase> List = new List<MdTokenBase>();

        public void Parse(MdPage owner)
        {
            var text = owner.Text.AsSpan();

            while (MdTokenBase.Parse(this, owner, text)) ;
        }
    }

    /// <summary>
    /// Md tree root. Containes (*.md) pages.
    /// </summary>
    internal class MdDoc : Component
    {
        public MdDoc(Component owner)
            : base(owner)
        {

        }
    }

    internal class MdPage : Component
    {
        public MdPage(MdDoc owner, string text)
            : base(owner)
        {
            Data.Text = text;
        }

        public string Text => Data.Text;
    }

    /// <summary>
    /// Base class for token.
    /// </summary>
    internal abstract class MdTokenBase : Component
    {
        /// <summary>
        /// Constructor registry, factory mode.
        /// </summary>
        public MdTokenBase()
            : base(null)
        {

        }

        /// <summary>
        /// Constructor instance token.
        /// </summary>
        public MdTokenBase(MdPage owner, int length)
            : base(owner)
        {
            UtilDoc.Assert(length > 0);

            // IndexBegin is IndexEnd + 1 of previous
            var indexBegin = 0;
            if (Owner.List.Count > 1)
            {
                var previous = (MdTokenBase)Owner.Data.Last(offset: 1).Component();
                indexBegin = previous.IndexEnd + 1;
            }

            Data.IndexBegin = indexBegin;
            Data.IndexEnd = Data.IndexBegin + length - 1;

            UtilDoc.Assert(Data.IndexEnd <= owner.Text.Length);
        }

        public new MdPage Owner => (MdPage)base.Owner;

        public int IndexBegin => Data.IndexBegin;

        public int IndexEnd => Data.IndexEnd;

        public int Length => IndexEnd - IndexBegin + 1;

        internal void IndexEndSet(int index)
        {
            var isLast = Owner.Data.Last() == Data;
            UtilDoc.Assert(isLast, "Can only set IndexEnd of last token!");

            UtilDoc.Assert(index >= 0 && index < Owner.Text.Length, "Index out of range!");

            Data.IndexEnd = index;
        }

        /// <summary>
        /// Gets Text. This is the text between IndexBegin and IndexEnd.
        /// </summary>
        public string Text
        {
            get
            {
                return Owner.Text.Substring(Data.IndexBegin, Data.IndexEnd - Data.IndexBegin + 1);
            }
        }

        /// <summary>
        /// Returns current text parse index.
        /// </summary>
        public static int ParseIndex(MdPage owner)
        {
            var result = 0;

            MdTokenBase token = (MdTokenBase)owner.Data.Last(isOrDefault: true)?.Component();
            if (token != null)
            {
                result = token.IndexEnd + 1;
            }
            return result;
        }

        internal static bool Parse(MdRegistry registry, MdPage owner, ReadOnlySpan<char> text, bool isExcludeContent = false)
        {
            var result = false;

            var index = ParseIndex(owner);

            foreach (var token in registry.List)
            {
                if (isExcludeContent)
                {
                    if (token.GetType() == typeof(MdContent))
                    {
                        break;
                    }
                }

                // Parse
                var countBefore = owner.Data.ListCount();
                token.Parse(registry, owner, text, index);
                var countAfter = owner.Data.ListCount();

                UtilDoc.Assert(countBefore <= countAfter);

                // A token has been created
                if (countBefore < countAfter)
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        internal virtual void Parse(MdRegistry registry, MdPage owner, ReadOnlySpan<char> text, int index)
        {

        }
    }

    internal class MdParagraph : MdTokenBase
    {
        /// <summary>
        /// Constructor registry, factory mode.
        /// </summary>
        public MdParagraph()
            : base()
        {

        }

        /// <summary>
        /// Constructor instance token.
        /// </summary>
        public MdParagraph(MdPage owner, int length)
            : base(owner, length)
        {

        }

        internal override void Parse(MdRegistry registry, MdPage owner, ReadOnlySpan<char> text, int index)
        {
            int next = index;
            int count = 0;
            while (MdNewLine.Parse(text, next, out int length))
            {
                next += length;
                count += 1;
            }
            if (count >= 2)
            {
                new MdParagraph(owner, next - index);
            }
        }
    }

    internal class MdNewLine : MdTokenBase
    {
        /// <summary>
        /// Constructor registry, factory mode.
        /// </summary>
        public MdNewLine()
            : base()
        {

        }

        /// <summary>
        /// Constructor instance token.
        /// </summary>
        public MdNewLine(MdPage owner, int length)
            : base(owner, length)
        {

        }

        /// <summary>
        /// Returns true, if NewLine.
        /// </summary>
        internal static bool Parse(ReadOnlySpan<char> text, int index, out int length)
        {
            var result = false;
            length = 0;

            var textFindList = new List<string>
            {
                "\r\n",
                "\r",
                "\n"
            };

            foreach (var textFind in textFindList)
            {
                if (text.StartsWith(index, textFind))
                {
                    result = true;
                    length = textFind.Length;
                    break;
                }
            }
            return result;
        }

        internal override void Parse(MdRegistry registry, MdPage owner, ReadOnlySpan<char> text, int index)
        {
            if (Parse(text, index, out int length))
            {
                new MdNewLine(owner, length);
            }
        }
    }

    internal class MdSpace : MdTokenBase
    {
        /// <summary>
        /// Constructor registry, factory mode.
        /// </summary>
        public MdSpace()
            : base()
        {

        }

        /// <summary>
        /// Constructor instance token.
        /// </summary>
        public MdSpace(MdPage owner, int length)
            : base(owner, length)
        {

        }

        internal override void Parse(MdRegistry registry, MdPage owner, ReadOnlySpan<char> text, int index)
        {
            var length = 0;
            while (text.StartsWith(index + length, " "))
            {
                length += 1;
            }

            if (length > 0)
            {
                new MdSpace(owner, length);
            }
        }
    }

    internal class MdComment : MdTokenBase
    {
        /// <summary>
        /// Constructor registry, factory mode.
        /// </summary>
        public MdComment()
            : base()
        {

        }

        /// <summary>
        /// Constructor instance token.
        /// </summary>
        public MdComment(MdPage owner, int length, bool isCommentEnd)
            : base(owner, length)
        {
            Data.IsCommentEnd = isCommentEnd;
        }

        public bool IsCommentEnd => Data.IsCommentEnd;

        internal override void Parse(MdRegistry registry, MdPage owner, ReadOnlySpan<char> text, int index)
        {
            var commentBegin = "<!--";
            var commentEnd = "-->";
            if (text.StartsWith(index, commentBegin))
            {
                new MdComment(owner, commentBegin.Length, isCommentEnd: false);
            }
            if (text.StartsWith(index, commentEnd))
            {
                new MdComment(owner, commentEnd.Length, isCommentEnd: true);
            }
        }
    }

    internal class MdTitle : MdTokenBase
    {
        /// <summary>
        /// Constructor registry, factory mode.
        /// </summary>
        public MdTitle()
            : base()
        {

        }

        /// <summary>
        /// Constructor instance token.
        /// </summary>
        public MdTitle(MdPage owner, int length, int titleLevel)
            : base(owner, length)
        {
            Data.TitleLevel = titleLevel;
        }

        public int TitleLevel => Data.TitleLevel;

        internal override void Parse(MdRegistry registry, MdPage owner, ReadOnlySpan<char> text, int index)
        {
            if (!text.StartsWith(index, "####"))
            {
                if (text.StartsWith(index, "### "))
                {
                    new MdTitle(owner, length: 3, titleLevel: 3);
                }
                if (text.StartsWith(index, "## "))
                {
                    new MdTitle(owner, length: 2, titleLevel: 2);
                }
                if (text.StartsWith(index, "# "))
                {
                    new MdTitle(owner, length: 1, titleLevel: 1);
                }
            }
        }
    }

    internal class MdBullet : MdTokenBase
    {
        /// <summary>
        /// Constructor registry, factory mode.
        /// </summary>
        public MdBullet()
        {

        }

        /// <summary>
        /// Constructor instance token.
        /// </summary>
        public MdBullet(MdPage owner, int length)
            : base(owner, length)
        {

        }

        internal override void Parse(MdRegistry registry, MdPage owner, ReadOnlySpan<char> text, int index)
        {
            if (text.StartsWith(index, "* "))
            {
                new MdBullet(owner, 1);
            }
        }
    }

    internal class MdImage : MdTokenBase
    {
        /// <summary>
        /// Constructor registry, factory mode.
        /// </summary>
        public MdImage()
            : base()
        {

        }

        /// <summary>
        /// Constructor instance token.
        /// </summary>
        public MdImage(MdPage owner, int length)
            : base(owner, length)
        {

        }

        internal override void Parse(MdRegistry registry, MdPage owner, ReadOnlySpan<char> text, int index)
        {
            if (text.StartsWith(index, "!["))
            {
                new MdImage(owner, 2);
            }
        }
    }

    internal enum MdBracketEnum
    {
        None,

        Round,

        Square,
    }

    internal class MdBracket : MdTokenBase
    {
        /// <summary>
        /// Constructor registry, factory mode.
        /// </summary>
        public MdBracket()
            : base()
        {

        }

        /// <summary>
        /// Constructor instance token.
        /// </summary>
        public MdBracket(MdPage owner, int length, MdBracketEnum bracketEnum, bool isBracketEnd)
            : base(owner, length)
        {
            Data.BracketEnum = bracketEnum;
            Data.IsBracketEnd = isBracketEnd;
        }

        public MdBracketEnum BracketEnum => Data.BracketEnum;

        public bool IsBracketEnd => Data.IsBracketEnd;

        public string TextBracket
        {
            get
            {
                if (BracketEnum == MdBracketEnum.Round)
                {
                    if (IsBracketEnd == false)
                    {
                        return "(";
                    }
                    else
                    {
                        return ")";
                    }
                }
                else
                {
                    if (BracketEnum == MdBracketEnum.Square)
                    {
                        if (IsBracketEnd == false)
                        {
                            return "[";
                        }
                        else
                        {
                            return "]";
                        }
                    }
                    else
                    {
                        throw new Exception("Type unknown!");
                    }
                }
            }
        }

        internal override void Parse(MdRegistry registry, MdPage owner, ReadOnlySpan<char> text, int index)
        {
            var textChar = text.Char(index);
            switch (textChar)
            {
                case '(':
                    new MdBracket(owner, 1, MdBracketEnum.Round, isBracketEnd: false);
                    break;
                case ')':
                    new MdBracket(owner, 1, MdBracketEnum.Round, isBracketEnd: true);
                    break;
                case '[':
                    new MdBracket(owner, 1, MdBracketEnum.Square, isBracketEnd: false);
                    break;
                case ']':
                    new MdBracket(owner, 1, MdBracketEnum.Square, isBracketEnd: true);
                    break;
            }
        }
    }

    internal class MdCode : MdTokenBase
    {
        /// <summary>
        /// Constructor registry, factory mode.
        /// </summary>
        public MdCode()
        {

        }

        /// <summary>
        /// Constructor instance token.
        /// </summary>
        public MdCode(MdPage owner, int length)
            : base(owner, length)
        {

        }

        internal override void Parse(MdRegistry registry, MdPage owner, ReadOnlySpan<char> text, int index)
        {
            if (text.StartsWith(index, "```") && text.Char(index + 3) != '`')
            {
                new MdCode(owner, 3);
            }
        }
    }

    internal enum MdFontEnum
    {
        None,

        Bold,

        Italic,
    }

    internal class MdFont : MdTokenBase
    {
        /// <summary>
        /// Constructor registry, factory mode.
        /// </summary>
        public MdFont()
        {

        }

        /// <summary>
        /// Constructor instance token.
        /// </summary>
        public MdFont(MdPage owner, int length, MdFontEnum fontEnum)
            : base(owner, length)
        {
            Data.FontEnum = fontEnum;
        }

        public MdFontEnum FontEnum => Data.FontEnum;

        internal override void Parse(MdRegistry registry, MdPage owner, ReadOnlySpan<char> text, int index)
        {
            if (text.StartsWith(index, "**") && text.Char(index + 2) != '*')
            {
                new MdFont(owner, 2, MdFontEnum.Bold);
            }
            else
            {
                if (text.StartsWith(index, "*") && text.Char(index + 1) != '*')
                {
                    new MdFont(owner, 1, MdFontEnum.Italic);
                }
            }
        }
    }

    internal class MdLink : MdTokenBase
    {
        /// <summary>
        /// Constructor registry, factory mode.
        /// </summary>
        public MdLink()
        {
            textList = new List<string>
            {
                "http://",
                "https://"
            };
        }

        /// <summary>
        /// Constructor instance token.
        /// </summary>
        public MdLink(MdPage owner, int length)
            : base(owner, length)
        {

        }

        private readonly List<string> textList;

        internal override void Parse(MdRegistry registry, MdPage owner, ReadOnlySpan<char> text, int index)
        {
            foreach (var item in textList)
            {
                if (text.StartsWith(index, item))
                {
                    new MdLink(owner, item.Length);
                    break;
                }
            }
        }
    }

    internal class MdContent : MdTokenBase
    {
        /// <summary>
        /// Constructor registry, factory mode.
        /// </summary>
        public MdContent()
            : base()
        {

        }

        /// <summary>
        /// Constructor instance token.
        /// </summary>
        public MdContent(MdPage owner, int length)
            : base(owner, length)
        {

        }

        internal override void Parse(MdRegistry registry, MdPage owner, ReadOnlySpan<char> text, int index)
        {
            MdContent token = null;
            while (ParseIndex(owner) < text.Length)
            {
                if (Parse(registry, owner, text, isExcludeContent: true) == false)
                {
                    if (token == null)
                    {
                        token = new MdContent(owner, 1);
                    }
                    else
                    {
                        token.IndexEndSet(token.IndexEnd + 1);
                    }
                }
                else
                {
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Subset of Registry.
    /// </summary>
    internal class SyntaxRegistry
    {
        public SyntaxRegistry(Registry registry)
        {
            foreach (var type in registry.List)
            {
                if (type.IsSubclassOf(typeof(SyntaxBase)))
                {
                    // Create instance for registry, factory mode.
                    var syntax = (SyntaxBase)Activator.CreateInstance(type);
                    List.Add(syntax);
                }
            }
        }

        /// <summary>
        /// (Syntax)
        /// </summary>
        public List<SyntaxBase> List = new List<SyntaxBase>();

        public void Parse(MdDoc mdDoc, SyntaxDoc syntaxDocOne, SyntaxDoc syntaxDocTwo, HtmlDoc htmlDoc)
        {
            // ParseOne
            foreach (MdPage page in mdDoc.List)
            {
                if (page.Data.ListCount() > 0)
                {
                    var tokenBegin = (MdTokenBase)page.Data.List[0].Component();
                    var tokenEnd = (MdTokenBase)page.Data.List[page.Data.List.Count - 1].Component();
                    var syntaxPage = new SyntaxPage(syntaxDocOne, tokenBegin, tokenEnd);
                    SyntaxBase.ParseOneMain(this, syntaxPage);
                }
            }

            // ParseTwo
            SyntaxBase.ParseTwoMain(syntaxDocTwo, syntaxDocOne);

            // ParseHtml
            syntaxDocTwo.ParseHtml(htmlDoc);
        }
    }

    /// <summary>
    /// Base class for md syntax tree.
    /// </summary>
    internal abstract class SyntaxBase : Component
    {
        /// <summary>
        /// Constructor registry, factory mode.
        /// </summary>
        public SyntaxBase()
            : base(null)
        {

        }

        /// <summary>
        /// Constructor for Doc.
        /// </summary>
        public SyntaxBase(Component owner)
            : base(owner)
        {
            Data.TokenIdBegin = -1;
            Data.TokenIdEnd = -1;
        }

        /// <summary>
        /// Constructor ParseOne.
        /// </summary>
        public SyntaxBase(SyntaxBase owner, MdTokenBase tokenBegin, MdTokenBase tokenEnd)
            : base(owner)
        {
            Data.TokenIdBegin = tokenBegin.Data.Id;
            Data.TokenIdEnd = tokenEnd.Data.Id;

            CreateValidate();
        }

        /// <summary>
        /// Constructor ParseTwo.
        /// </summary>
        public SyntaxBase(SyntaxBase owner, SyntaxBase syntax)
            : base(owner)
        {
            Data.TokenIdBegin = syntax.TokenBegin.Data.Id;
            Data.TokenIdEnd = syntax.TokenEnd.Data.Id;

            if (owner is not SyntaxDoc)
            {
                bool isShorten = Index(syntax.Data.TokenIdEnd) <= Index(owner.Data.TokenIdEnd);
                bool isExtend = Index(syntax.Data.TokenIdBegin) == Index(owner.Data.TokenIdEnd) + 1;
                UtilDoc.Assert(isShorten ^ isExtend);
                var next = owner.Data;
                while (next.DataEnum != DataEnum.SyntaxDoc)
                {
                    UtilDoc.Assert(next.Owner.List.Last() == next); // Modify TokenIdEnd only on last item
                    next.TokenIdEnd = syntax.Data.TokenIdEnd; // Modify TokenIdEnd
                    next = next.Owner;
                }
                owner.Data.TokenIdEnd = syntax.Data.TokenIdEnd;
                UtilDoc.Assert(Index(owner.Data.TokenIdBegin) <= Index(owner.Data.TokenIdEnd));
            }

            CreateValidate();
        }

        /// <summary>
        /// Validate index ranges after create.
        /// </summary>
        private void CreateValidate()
        {
            var typeOwner = Data.Registry.DataEnumList[Data.Owner.DataEnum];
            if (!UtilDoc.IsSubclassOf(typeOwner, typeof(SyntaxDoc)))
            {
                UtilDoc.Assert(Index(Data.TokenIdBegin) <= Index(Data.TokenIdEnd));
                if (UtilDoc.IsSubclassOf(typeOwner, typeof(SyntaxBase)))
                {
                    UtilDoc.Assert(Index(Data.Owner.TokenIdBegin) <= (Data.TokenIdBegin));
                    if (Data.Index == 0)
                    {
                        UtilDoc.Assert(Index(Data.TokenIdBegin) == Index(Data.Owner.TokenIdBegin));
                    }
                    else
                    {
                        var indexPrevious = Index(Owner.Data.List[Data.Index - 1].TokenIdEnd);
                        var index = Index(Data.TokenIdBegin);

                        UtilDoc.Assert(indexPrevious + 1 == index);
                    }
                }
            }
        }

        private int Index(int id)
        {
            return Data.Registry.IdList[id].Index;
        }

        public MdTokenBase TokenBegin => (MdTokenBase)Data.Registry.IdList[Data.TokenIdBegin].Component();

        public MdTokenBase TokenEnd => (MdTokenBase)Data.Registry.IdList[Data.TokenIdEnd].Component();

        /// <summary>
        /// Gets Text. This is the text between TokenBegin and TokenEnd.
        /// </summary>
        public string Text
        {
            get
            {
                var result = new StringBuilder();
                if (this is not SyntaxDoc)
                {
                    var tokenEnd = TokenEnd;
                    Component component = TokenBegin;
                    do
                    {
                        var token = (MdTokenBase)component;
                        result.Append(token.Text);
                    } while (Next(ref component, tokenEnd));
                }
                return result.ToString();
            }
        }

        /// <summary>
        /// Returns token of currently parsed syntax.
        /// </summary>
        internal static MdTokenBase ParseOneToken(SyntaxBase syntax)
        {
            var result = syntax.TokenBegin;
            var last = syntax.Data.Last(isOrDefault: true);
            if (last != null)
            {
                var syntaxLast = (SyntaxBase)syntax.Data.Registry.IdList[last.Id].Component();
                result = syntaxLast.TokenEnd;
                result = (MdTokenBase)result.Next(syntax.TokenEnd);
            }
            return result;
        }

        /// <summary>
        /// Returns true, if tokenBegin is a new line starting with token T.
        /// </summary>
        /// <typeparam name="T">Token to search.</typeparam>
        /// <param name="tokenSpace">Leading space.</param>
        /// <param name="token">Found token.</param>
        internal static bool ParseOneIsNewLine<T>(MdTokenBase tokenBegin, MdTokenBase tokenEnd, out MdSpace tokenSpace, out T token) where T : MdTokenBase
        {
            var result = false;

            bool isStart;
            tokenSpace = null;
            token = null;

            Component component = tokenBegin;

            // Leading start or NewLine or Paragraph
            var previous = tokenBegin.Previous((MdTokenBase)null);
            isStart = previous == null || previous is MdNewLine || previous is MdParagraph;
            // Leading Space
            if (component is MdSpace space)
            {
                tokenSpace = space;
                component = component.Next(tokenEnd);
            }
            // Token
            if (component is T)
            {
                token = (T)component;
            }

            if (isStart && token != null)
            {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Returns true, if tokenBegin is Link.
        /// </summary>
        /// <param name="link">Detected Link.</param>
        internal static bool ParseOneIsLink(MdTokenBase tokenBegin, MdTokenBase tokenEnd, out MdTokenBase linkEnd, out string link)
        {
            bool result = false;
            linkEnd = tokenBegin;
            MdTokenBase next = tokenBegin;
            link = null;
            do
            {
                if (next is MdLink)
                {
                    if (next != tokenBegin)
                    {
                        break;
                    }
                }
                if (!(next is MdContent || next is MdLink))
                {
                    break;
                }
                linkEnd = next;
                link += next.Text;
                result = true;
            } while (Next(ref next, tokenEnd));
            return result;
        }

        /// <summary>
        /// Returns true, if tokenBegin contains LinkText.
        /// </summary>
        /// <param name="linkText">Detected LinkText.</param>
        internal static bool ParseOneIsLinkText(MdTokenBase tokenBegin, MdTokenBase tokenEnd, out MdTokenBase linkTextEnd, out string linkText)
        {
            var result = false;
            linkTextEnd = tokenBegin;
            MdTokenBase next = tokenBegin;
            linkText = null;
            do
            {
                if (!(next is MdContent || next is MdSpace || next is MdLink))
                {
                    break;
                }
                linkTextEnd = next;
                linkText += next.Text;
                result = true;
            } while (Next(ref next, tokenEnd));
            return result;
        }

        /// <summary>
        /// Main entry for ParseOne.
        /// </summary>
        internal static void ParseOneMain(SyntaxRegistry registry, SyntaxBase owner)
        {
            var tokenEnd = owner.TokenEnd;
            MdTokenBase tokenBegin;
            while ((tokenBegin = ParseOneToken(owner)) != null)
            {
                bool isFind = false;
                foreach (var syntaxParser in registry.List)
                {
                    var countBefore = owner.Data.ListCount();
                    syntaxParser.ParseOne(registry, owner, tokenBegin, tokenEnd);
                    var countAfter = owner.Data.ListCount();

                    UtilDoc.Assert(countBefore <= countAfter);

                    if (countBefore < countAfter)
                    {
                        isFind = true;
                        break;
                    }
                }
                UtilDoc.Assert(isFind, "No syntax parser found!");
            }
        }

        internal static bool ParseTwoIsText(SyntaxBase syntax, bool isAllowLink, bool isAllowNewLine)
        {
            var result = syntax is SyntaxContent || syntax is SyntaxFont || syntax is SyntaxIgnore;
            if (isAllowLink)
            {
                result = result || syntax is SyntaxLink;
            }
            if (isAllowNewLine)
            {
                result = result || syntax is SyntaxNewLine;
            }
            return result;
        }

        /// <summary>
        /// Main entry for ParseTwo.
        /// </summary>
        internal static void ParseTwoMain(SyntaxBase owner, SyntaxBase syntax)
        {
            foreach (SyntaxBase item in syntax.List)
            {
                item.ParseTwo(owner);
            }
        }

        /// <summary>
        /// Main entry for ParseTwo with break.
        /// </summary>
        /// <param name="isOwnerNewChild">Returns true, if item is a child of ownerNew. If false, it is a child of owner.</param>
        internal static void ParseTwoMainBreak(SyntaxBase owner, SyntaxBase ownerNew, SyntaxBase syntax, Func<SyntaxBase, bool> isOwnerNewChild)
        {
            UtilDoc.Assert(ownerNew.Owner.Data == owner.Data);

            bool isOwnerNewChildLocal = true;
            foreach (DataDoc data in syntax.Data.ListGet())
            {
                SyntaxBase item = (SyntaxBase)data.Component(); 
                if (isOwnerNewChildLocal && isOwnerNewChild(item) == false)
                {
                    isOwnerNewChildLocal = false;
                }
                if (isOwnerNewChildLocal == false)
                {
                    // item is not child
                    item.ParseTwo(owner);
                }
                else
                {
                    // item is child
                    item.ParseTwo(ownerNew);
                }
            }
        }

        /// <summary>
        /// Returns paragraph.
        /// </summary>
        internal static SyntaxBase ParseTwoParagraph(SyntaxBase owner, SyntaxBase syntax)
        {
            var result = owner;
            if (owner is SyntaxPage page)
            {
                if (owner.Data.ListCount() > 0 && owner.Data.List.Last().DataEnum == DataEnum.SyntaxParagraph)
                {
                    result = (SyntaxBase)owner.Data.List.Last().Component();
                }
                else
                {
                    result = new SyntaxParagraph(page, syntax);
                }
            }
            return result;
        }

        /// <summary>
        /// Parse md token between tokenBegin and tokenEnd.
        /// </summary>
        internal virtual void ParseOne(SyntaxRegistry registry, SyntaxBase owner, MdTokenBase tokenBegin, MdTokenBase tokenEnd)
        {

        }

        /// <summary>
        /// Override this method to custom transform syntax tree ParseOne into ParseTwo.
        /// </summary>
        internal virtual void ParseTwo(SyntaxBase owner)
        {
            ParseTwoMain(owner, this);
        }

        /// <summary>
        /// Override this method to custom transform syntax tree ParseTwo into html.
        /// </summary>
        internal virtual void ParseHtml(HtmlBase owner)
        {
            foreach (SyntaxBase item in List)
            {
                item.ParseHtml(owner);
            }
        }
    }

    /// <summary>
    /// Syntax tree root.
    /// </summary>
    internal class SyntaxDoc : SyntaxBase
    {
        /// <summary>
        /// Constructor registry, factory mode.
        /// </summary>
        public SyntaxDoc()
            : base(null)
        {

        }

        public SyntaxDoc(Component owner)
            : base(owner)
        {

        }
    }

    internal class SyntaxPage : SyntaxBase
    {
        /// <summary>
        /// Constructor registry, factory mode.
        /// </summary>
        public SyntaxPage()
            : base()
        {

        }

        /// <summary>
        /// Constructor ParseOne.
        /// </summary>
        public SyntaxPage(SyntaxBase owner, MdTokenBase tokenBegin, MdTokenBase tokenEnd)
            : base(owner, tokenBegin, tokenEnd)
        {

        }

        /// <summary>
        /// Constructor ParseTwo.
        /// </summary>
        public SyntaxPage(SyntaxBase owner, SyntaxBase syntax)
            : base(owner, syntax)
        {

        }

        internal override void ParseTwo(SyntaxBase owner)
        {
            var page = new SyntaxPage(owner, this);

            ParseTwoMain(page, this);
        }

        internal override void ParseHtml(HtmlBase owner)
        {
            var page = new HtmlPage(owner, this);

            base.ParseHtml(page);
        }
    }

    internal class SyntaxComment : SyntaxBase
    {
        /// <summary>
        /// Constructor registry, factory mode.
        /// </summary>
        public SyntaxComment()
            : base()
        {

        }

        /// <summary>
        /// Constructor ParseOne.
        /// </summary>
        public SyntaxComment(SyntaxBase owner, MdTokenBase tokenBegin, MdTokenBase tokenEnd)
            : base(owner, tokenBegin, tokenEnd)
        {

        }

        /// <summary>
        /// Constructor ParseTwo.
        /// </summary>
        public SyntaxComment(SyntaxBase owner, SyntaxBase syntax)
            : base(owner, syntax)
        {

        }

        internal override void ParseOne(SyntaxRegistry registry, SyntaxBase owner, MdTokenBase tokenBegin, MdTokenBase tokenEnd)
        {
            if (tokenBegin is MdComment commentBegin && !commentBegin.IsCommentEnd)
            {
                Component component = tokenBegin;
                while (Next(ref component, tokenEnd))
                {
                    if (component is MdComment commentEnd && commentEnd.IsCommentEnd)
                    {
                        new SyntaxComment(owner, commentBegin, commentEnd);
                        break;
                    }
                }
            }
        }

        internal override void ParseTwo(SyntaxBase owner)
        {
            new SyntaxComment(owner, this);
        }

        internal override void ParseHtml(HtmlBase owner)
        {
            new HtmlComment(owner, this);
        }
    }

    internal class SyntaxTitle : SyntaxBase
    {
        /// <summary>
        /// Constructor registry, factory mode.
        /// </summary>
        public SyntaxTitle()
            : base()
        {

        }

        /// <summary>
        /// Constructor ParseOne.
        /// </summary>
        public SyntaxTitle(SyntaxBase owner, MdTokenBase tokenBegin, MdTokenBase tokenEnd)
            : base(owner, tokenBegin, tokenEnd)
        {

        }

        /// <summary>
        /// Constructor ParseTwo.
        /// </summary>
        public SyntaxTitle(SyntaxPage owner, SyntaxBase syntax)
            : base(owner, syntax)
        {

        }

        internal override void ParseOne(SyntaxRegistry registry, SyntaxBase owner, MdTokenBase tokenBegin, MdTokenBase tokenEnd)
        {
            if (ParseOneIsNewLine<MdTitle>(tokenBegin, tokenEnd, out MdSpace tokenSpace, out var token))
            {
                // Ignore leading space
                if (tokenSpace != null)
                {
                    new SyntaxIgnore(owner, tokenSpace);
                }

                var title = new SyntaxTitle(owner, token, tokenEnd);
                new SyntaxIgnore(title, token);

                // Ignore space after title
                if (token.Next(tokenEnd) is MdSpace space)
                {
                    new SyntaxIgnore(title, space);
                }

                ParseOneMain(registry, title);
            }
        }

        internal override void ParseTwo(SyntaxBase owner)
        {
            var title = new SyntaxTitle((SyntaxPage)owner, this);
            ParseTwoMainBreak(owner, title, this, (syntax) => ParseTwoIsText(syntax, isAllowLink: false, isAllowNewLine: false)); // No link in title
        }

        internal override void ParseHtml(HtmlBase owner)
        {
            var title = new HtmlTitle(owner, this);
            base.ParseHtml(title);
        }
    }

    internal class SyntaxBullet : SyntaxBase
    {
        /// <summary>
        /// Constructor registry, factory mode.
        /// </summary>
        public SyntaxBullet()
            : base()
        {

        }

        /// <summary>
        /// Constructor ParseOne.
        /// </summary>
        public SyntaxBullet(SyntaxBase owner, MdTokenBase tokenBegin, MdTokenBase tokenEnd)
            : base(owner, tokenBegin, tokenEnd)
        {

        }

        /// <summary>
        /// Constructor ParseTwo.
        /// </summary>
        public SyntaxBullet(SyntaxPage owner, SyntaxBase syntax)
            : base(owner, syntax)
        {

        }

        internal override void ParseOne(SyntaxRegistry registry, SyntaxBase owner, MdTokenBase tokenBegin, MdTokenBase tokenEnd)
        {
            if (ParseOneIsNewLine<MdBullet>(tokenBegin, tokenEnd, out MdSpace tokenSpace, out var token))
            {
                // Space after star
                if (token.Next(tokenEnd) is MdSpace)
                {
                    // Ignore leading space
                    if (tokenSpace != null)
                    {
                        new SyntaxIgnore(owner, tokenSpace);
                    }

                    var bullet = new SyntaxBullet(owner, token, tokenEnd);
                    new SyntaxIgnore(bullet, token);

                    // Ignor space after bullet
                    if (token.Next(tokenEnd) is MdSpace space)
                    {
                        new SyntaxIgnore(bullet, space);
                    }

                    ParseOneMain(registry, bullet);
                }
            }
        }

        internal override void ParseTwo(SyntaxBase owner)
        {
            var bullet = new SyntaxBullet((SyntaxPage)owner, this);
            ParseTwoMainBreak(owner, bullet, this, (syntax) => ParseTwoIsText(syntax, isAllowLink: true, isAllowNewLine: true)); // Link in bullet item.
        }

        internal override void ParseHtml(HtmlBase owner)
        {
            var bulletList = owner.Data.List?.Last().Component() as HtmlBulletList;
            if (bulletList == null)
            {
                bulletList = new HtmlBulletList(owner);
            }
            var bulletItem = new HtmlBulletItem(bulletList, this);
            base.ParseHtml(bulletItem);
        }
    }

    internal class SyntaxCode : SyntaxBase
    {
        /// <summary>
        /// Constructor registry, factory mode.
        /// </summary>
        public SyntaxCode()
            : base()
        {

        }

        /// <summary>
        /// Constructor ParseOne.
        /// </summary>
        public SyntaxCode(SyntaxBase owner, MdTokenBase tokenBegin, MdTokenBase tokenEnd, string codeLanguage)
            : base(owner, tokenBegin, tokenEnd)
        {
            Data.CodeLanguage = codeLanguage;
        }

        /// <summary>
        /// Constructor ParseTwo.
        /// </summary>
        public SyntaxCode(SyntaxBase owner, SyntaxBase syntax)
            : base(owner, syntax)
        {

        }

        public string CodeLanguage => Data.CodeLanguage;

        internal override void ParseOne(SyntaxRegistry registry, SyntaxBase owner, MdTokenBase tokenBegin, MdTokenBase tokenEnd)
        {
            if (ParseOneIsNewLine<MdCode>(tokenBegin, tokenEnd, out MdSpace tokenSpace, out var codeBegin))
            {
                MdCode codeEnd = null;
                MdTokenBase next = codeBegin;
                while (Next(ref next, tokenEnd))
                {
                    if (next is MdCode tokenCode)
                    {
                        codeEnd = tokenCode;
                        break;
                    }
                }

                if (codeEnd != null)
                {
                    // Code language
                    MdContent codeLanguage = null;
                    if (codeBegin.Next(tokenEnd) is MdContent content)
                    {
                        codeLanguage = content;
                    }

                    // Next
                    next = codeLanguage != null ? codeLanguage : codeBegin;

                    // Code content
                    MdTokenBase codeContentBegin = null;
                    MdTokenBase codeContentEnd = null;
                    while (Next(ref next, tokenEnd))
                    {
                        if (next == codeEnd)
                        {
                            break;
                        }
                        if (codeContentBegin == null)
                        {
                            codeContentBegin = next;
                            codeContentEnd = next;
                        }
                        else
                        {
                            codeContentEnd = next;
                        }
                    }

                    // Code content exists
                    if (codeContentBegin != null)
                    {
                        // Ignore leading space
                        if (tokenSpace != null)
                        {
                            new SyntaxIgnore(owner, tokenSpace);
                        }

                        // Create code syntax
                        var code = new SyntaxCode(owner, codeBegin, codeEnd, codeLanguage?.Text);

                        // Ignore code language token
                        new SyntaxIgnore(code, codeBegin);
                        if (codeLanguage != null)
                        {
                            new SyntaxIgnore(code, codeLanguage);
                        }

                        // Code content
                        new SyntaxContent(code, codeContentBegin, codeContentEnd);

                        // Ignore code end token
                        new SyntaxIgnore(code, codeEnd);
                    }
                }
            }
        }

        internal override void ParseTwo(SyntaxBase owner)
        {
            new SyntaxCode(owner, this);
        }
    }

    internal class SyntaxFont : SyntaxBase
    {
        /// <summary>
        /// Constructor registry, factory mode.
        /// </summary>
        public SyntaxFont()
            : base()
        {

        }

        /// <summary>
        /// Constructor ParseOne.
        /// </summary>
        public SyntaxFont(SyntaxBase owner, MdTokenBase tokenBegin, MdTokenBase tokenEnd)
            : base(owner, tokenBegin, tokenEnd)
        {

        }

        /// <summary>
        /// Constructor ParseTwo.
        /// </summary>
        public SyntaxFont(SyntaxBase owner, SyntaxBase syntax)
            : base(owner, syntax)
        {

        }

        public MdFontEnum FontEnum => ((MdFont)TokenBegin).FontEnum;

        internal override void ParseOne(SyntaxRegistry registry, SyntaxBase owner, MdTokenBase tokenBegin, MdTokenBase tokenEnd)
        {
            if (tokenBegin is MdFont tokenFontBegin)
            {
                var next = tokenBegin;
                while (Next(ref next, tokenEnd))
                {
                    if (next is MdFont || next is MdNewLine)
                    {
                        break;
                    }
                }
                if (next != tokenBegin && next is MdFont tokenFontEnd && tokenFontEnd.FontEnum == tokenFontBegin.FontEnum)
                {
                    var syntaxFont = new SyntaxFont(owner, tokenBegin, next);
                    new SyntaxIgnore(syntaxFont, tokenBegin);
                    ParseOneMain(registry, syntaxFont);
                }
                else
                {
                    if (owner is SyntaxFont syntaxFont)
                    {
                        new SyntaxIgnore(syntaxFont, tokenBegin);
                    }
                    else
                    {
                        new SyntaxContent(owner, tokenBegin, tokenBegin);
                    }
                }
            }
        }

        internal override void ParseTwo(SyntaxBase owner)
        {
            var ownerFont = owner;
            bool isBegin = !(owner is SyntaxFont);

            if (isBegin)
            {
                ownerFont = new SyntaxFont(owner, this);
            }

            ParseTwoMain(ownerFont, this);
        }

        internal override void ParseHtml(HtmlBase owner)
        {
            var font = owner;
            bool isBegin = !(Owner is SyntaxFont);

            if (isBegin)
            {
                font = new HtmlFont(owner, this);
            }

            base.ParseHtml(font);
        }
    }

    internal class SyntaxLink : SyntaxBase
    {
        /// <summary>
        /// Constructor registry, factory mode.
        /// </summary>
        public SyntaxLink()
        {

        }

        /// <summary>
        /// Constructor ParseOne.
        /// </summary>
        public SyntaxLink(SyntaxBase owner, MdTokenBase tokenBegin, MdTokenBase tokenEnd, string link, string linkText)
            : base(owner, tokenBegin, tokenEnd)
        {
            Data.Link = link;
            Data.LinkText = linkText;
        }

        /// <summary>
        /// Constructor ParseTwo.
        /// </summary>
        public SyntaxLink(SyntaxBase owner, SyntaxLink syntax)
            : base(owner, syntax)
        {
            Data.Link = syntax.Link;
            Data.LinkText = syntax.LinkText;
        }

        public string Link => Data.Link;

        public string LinkText => Data.LinkText;

        internal override void ParseOne(SyntaxRegistry registry, SyntaxBase owner, MdTokenBase tokenBegin, MdTokenBase tokenEnd)
        {
            // For example http://
            if (tokenBegin is MdLink)
            {
                if (ParseOneIsLink(tokenBegin, tokenEnd, out var linkEnd, out string link))
                {
                    new SyntaxLink(owner, tokenBegin, linkEnd, link, link);
                }
            }

            // For example []()
            if (tokenBegin is MdBracket bracketSquare && bracketSquare.TextBracket == "[")
            {
                var next = tokenBegin.Next(tokenEnd);
                ParseOneIsLinkText(next, tokenEnd, out var linkTextEnd, out string linkText);
                if (linkText != null)
                {
                    next = linkTextEnd.Next(tokenEnd);
                }
                if (next is MdBracket bracketSquareEnd && bracketSquareEnd.TextBracket == "]")
                {
                    next = next.Next(tokenEnd);
                    if (next is MdBracket bracketRound && bracketRound.TextBracket == "(")
                    {
                        next = next.Next(tokenEnd);
                        if (ParseOneIsLinkText(next, tokenEnd, out var linkEnd, out string link))
                        {
                            next = linkEnd.Next(tokenEnd);
                            if (next is MdBracket bracketRoundEnd && bracketRoundEnd.TextBracket == ")")
                            {
                                if (linkText == null)
                                {
                                    linkText = link;
                                }
                                new SyntaxLink(owner, tokenBegin, next, link, linkText);
                            }
                        }
                    }
                }
            }
        }

        internal override void ParseTwo(SyntaxBase owner)
        {
            new SyntaxLink(ParseTwoParagraph(owner, this), this);
        }

        internal override void ParseHtml(HtmlBase owner)
        {
            new HtmlLink(owner, this);
        }
    }

    internal class SyntaxImage : SyntaxBase
    {
        /// <summary>
        /// Constructor registry, factory mode.
        /// </summary>
        public SyntaxImage()
        {

        }

        /// <summary>
        /// Constructor ParseOne.
        /// </summary>
        public SyntaxImage(SyntaxBase owner, MdTokenBase tokenBegin, MdTokenBase tokenEnd, string link, string linkText)
            : base(owner, tokenBegin, tokenEnd)
        {
            Data.Link = link;
            Data.LinkText = linkText;
        }

        /// <summary>
        /// Constructor ParseTwo.
        /// </summary>
        public SyntaxImage(SyntaxPage owner, SyntaxImage syntax)
            : base(owner, syntax)
        {
            Data.Link = syntax.Link;
            Data.LinkText = syntax.LinkText;
        }

        public string Link => Data.Link;

        public string LinkText => Data.LinkText;

        internal override void ParseOne(SyntaxRegistry registry, SyntaxBase owner, MdTokenBase tokenBegin, MdTokenBase tokenEnd)
        {
            // For example ![]()
            if (tokenBegin is MdImage)
            {
                MdTokenBase next = tokenBegin.Next(tokenEnd);
                ParseOneIsLinkText(next, tokenEnd, out var linkTextEnd, out string linkText);
                next = linkTextEnd.Next(tokenEnd);
                if (next is MdBracket bracketSquareEnd && bracketSquareEnd.TextBracket == "]")
                {
                    next = next.Next(tokenEnd);
                    if (next is MdBracket bracketRound && bracketRound.TextBracket == "(")
                    {
                        next = next.Next(tokenEnd);
                        if (ParseOneIsLink(next, tokenEnd, out var linkEnd, out var link))
                        {
                            if (linkEnd.Next(tokenEnd) is MdBracket bracketRoundEnd && bracketRoundEnd.Text == ")")
                            {
                                new SyntaxImage(owner, tokenBegin, bracketRoundEnd, link, linkText);
                            }
                        }
                    }
                }
            }
        }

        internal override void ParseTwo(SyntaxBase owner)
        {
            new SyntaxImage((SyntaxPage)owner, this);
        }

        internal override void ParseHtml(HtmlBase owner)
        {
            new HtmlImage(owner, this);
        }
    }

    internal class SyntaxContent : SyntaxBase
    {
        /// <summary>
        /// Constructor registry, factory mode.
        /// </summary>
        public SyntaxContent()
            : base()
        {

        }

        /// <summary>
        /// Constructor ParseOne.
        /// </summary>
        public SyntaxContent(SyntaxBase owner, MdTokenBase tokenBegin, MdTokenBase tokenEnd)
            : base(owner, tokenBegin, tokenEnd)
        {

        }

        /// <summary>
        /// Constructor ParseTwo.
        /// </summary>
        public SyntaxContent(SyntaxBase owner, SyntaxBase syntax)
            : base(owner, syntax)
        {

        }

        internal override void ParseOne(SyntaxRegistry registry, SyntaxBase owner, MdTokenBase tokenBegin, MdTokenBase tokenEnd)
        {
            bool isFind = false;
            if (tokenBegin is MdContent)
            {
                var contentEnd = tokenBegin;

                if (tokenBegin.Next(tokenEnd) is MdSpace)
                {
                    var next = tokenBegin.Next(tokenEnd).Next(tokenEnd) as MdContent;
                    if (next != null)
                    {
                        contentEnd = next;
                    }
                }

                new SyntaxContent(owner, tokenBegin, contentEnd);
                isFind = true;
            }

            if (tokenBegin is MdSpace)
            {
                var next = tokenBegin.Next(tokenEnd);
                if (next is not MdNewLine && next is not MdComment)
                {
                    new SyntaxContent(owner, tokenBegin, tokenBegin);
                    isFind = true;
                }
            }

            if (!isFind)
            {
                new SyntaxContent(owner, tokenBegin, tokenBegin);
            }
        }

        internal override void ParseTwo(SyntaxBase owner)
        {
            new SyntaxContent(ParseTwoParagraph(owner, this), this);
        }

        internal override void ParseHtml(HtmlBase owner)
        {
            new HtmlContent(owner, this);
        }
    }

    internal class SyntaxParagraph : SyntaxBase
    {
        /// <summary>
        /// Constructor registry, factory mode.
        /// </summary>
        public SyntaxParagraph()
            : base()
        {

        }

        /// <summary>
        /// Constructor ParseOne.
        /// </summary>
        public SyntaxParagraph(SyntaxBase owner, MdTokenBase tokenBegin, MdTokenBase tokenEnd)
            : base(owner, tokenBegin, tokenEnd)
        {

        }

        /// <summary>
        /// Constructor ParseTwo.
        /// </summary>
        public SyntaxParagraph(SyntaxPage owner, SyntaxBase syntax)
            : base(owner, syntax)
        {

        }

        internal override void ParseOne(SyntaxRegistry registry, SyntaxBase owner, MdTokenBase tokenBegin, MdTokenBase tokenEnd)
        {
            if (tokenBegin is MdParagraph)
            {
                var paragraph = new SyntaxParagraph(owner, tokenBegin, tokenEnd);
                // Ignore paragrpah token
                new SyntaxIgnore(paragraph, tokenBegin);
                ParseOneMain(registry, paragraph);
            }
        }

        private static bool ParseTwoIsChild(SyntaxBase syntax)
        {
            // Not a child of a paragraph
            bool result = !(
                syntax is SyntaxCode ||
                syntax is SyntaxTitle ||
                syntax is SyntaxBullet ||
                syntax is SyntaxPageBreak ||
                syntax is SyntaxImage ||
                syntax is SyntaxParagraph);
            return result;
        }

        internal override void ParseTwo(SyntaxBase owner)
        {
            if (owner is SyntaxPage page)
            {
                var paragraph = new SyntaxParagraph(page, this);
                ParseTwoMainBreak(page, paragraph, this, (syntax) => ParseTwoIsChild(syntax));
            }
            else
            {
                ParseTwoMain(owner, this);
            }
        }

        internal override void ParseHtml(HtmlBase owner)
        {
            var paragraph = new HtmlParagraph(owner, this);

            base.ParseHtml(paragraph);
        }
    }

    internal class SyntaxNewLine : SyntaxBase
    {
        /// <summary>
        /// Constructor registry, factory mode.
        /// </summary>
        public SyntaxNewLine()
            : base()
        {

        }

        /// <summary>
        /// Constructor ParseOne.
        /// </summary>
        public SyntaxNewLine(SyntaxBase owner, MdTokenBase token)
            : base(owner, token, token)
        {

        }

        /// <summary>
        /// Constructor ParseTwo.
        /// </summary>
        public SyntaxNewLine(SyntaxBase owner, SyntaxBase syntax)
            : base(owner, syntax)
        {

        }

        internal override void ParseOne(SyntaxRegistry registry, SyntaxBase owner, MdTokenBase tokenBegin, MdTokenBase tokenEnd)
        {
            if (tokenBegin is MdNewLine)
            {
                UtilDoc.Assert(tokenBegin.Next(tokenEnd) is not MdNewLine); // Detected by SyntaxParagraph.
                new SyntaxNewLine(owner, tokenBegin);
            }
        }

        internal override void ParseTwo(SyntaxBase owner)
        {
            new SyntaxNewLine(ParseTwoParagraph(owner, this), this);
        }

        internal override void ParseHtml(HtmlBase owner)
        {
            // No html for NewLine token.
        }
    }

    internal class SyntaxIgnore : SyntaxBase
    {
        /// <summary>
        /// Constructor registry, factory mode.
        /// </summary>
        public SyntaxIgnore()
            : base()
        {

        }

        /// <summary>
        /// Constructor ParseOne.
        /// </summary>
        public SyntaxIgnore(SyntaxBase owner, MdTokenBase token)
            : base(owner, token, token)
        {

        }

        /// <summary>
        /// Constructor ParseTwo.
        /// </summary>
        public SyntaxIgnore(SyntaxBase owner, SyntaxBase syntax)
            : base(owner, syntax)
        {

        }

        internal override void ParseOne(SyntaxRegistry registry, SyntaxBase owner, MdTokenBase tokenBegin, MdTokenBase tokenEnd)
        {
            // No parser found for tokenBegin!
            throw new Exception("Syntax unknown!");
        }

        internal override void ParseTwo(SyntaxBase owner)
        {
            new SyntaxIgnore(owner, this);
        }
    }

    /// <summary>
    /// Custom syntax for page break.
    /// </summary>
    internal class SyntaxPageBreak : SyntaxBase
    {
        /// <summary>
        /// Constructor registry, factory mode.
        /// </summary>
        public SyntaxPageBreak()
            : base()
        {

        }

        /// <summary>
        /// Constructor ParseOne.
        /// </summary>
        public SyntaxPageBreak(SyntaxBase owner, MdTokenBase tokenBegin, MdTokenBase tokenEnd)
            : base(owner, tokenBegin, tokenEnd)
        {

        }

        /// <summary>
        /// Constructor ParseTwo.
        /// </summary>
        public SyntaxPageBreak(SyntaxBase owner, SyntaxBase syntax)
            : base(owner, syntax)
        {

        }

        internal override void ParseOne(SyntaxRegistry registry, SyntaxBase owner, MdTokenBase tokenBegin, MdTokenBase tokenEnd)
        {
            // Detect (Page)
            if (ParseOneIsNewLine<MdBracket>(tokenBegin, tokenEnd, out var tokenSpace, out var token))
            {
                if (token.TextBracket == "(")
                {
                    if (token.Next(tokenEnd) is MdContent content && token.Next(tokenEnd)?.Next(tokenEnd) is MdBracket bracketEnd)
                    {
                        if (bracketEnd.TextBracket == ")")
                        {
                            if (content.Text == "Page")
                            {
                                // Ignore leading space
                                if (tokenSpace != null)
                                {
                                    new SyntaxIgnore(owner, tokenSpace);
                                }
                                var pageBreak = new SyntaxPageBreak(owner, token, tokenEnd);
                                new SyntaxIgnore(pageBreak, token);
                                new SyntaxIgnore(pageBreak, content);
                                new SyntaxIgnore(pageBreak, bracketEnd);
                                ParseOneMain(registry, pageBreak);
                            }
                        }
                    }
                }
            }
        }

        internal override void ParseTwo(SyntaxBase owner)
        {
            var page = new SyntaxPageBreak(owner, this);
            ParseTwoMain(page, this);
        }

        internal override void ParseHtml(HtmlBase owner)
        {
            var page = new HtmlPage((HtmlBase)owner.Owner, this);

            base.ParseHtml(page);
        }
    }

    /// <summary>
    /// Base class for html syntax tree.
    /// </summary>
    internal abstract class HtmlBase : Component
    {
        /// <summary>
        /// Constructor for Doc.
        /// </summary>
        public HtmlBase(Component owner)
            : base(owner)
        {

        }

        /// <summary>
        /// Constructor parse html.
        /// </summary>
        public HtmlBase(HtmlBase owner, SyntaxBase syntax)
            : base(owner)
        {
            Data.SyntaxId = Registry.ReferenceSet(syntax);
        }

        public SyntaxBase Syntax => Data.Registry.ReferenceGet<SyntaxBase>(Data.SyntaxId);

        internal string Render()
        {
            var result = new StringBuilder();
            Render(result);
            return result.ToString();
        }

        internal void Render(StringBuilder result)
        {
            RenderBegin(result);
            RenderContent(result);
            RenderEnd(result);
        }

        internal virtual void RenderBegin(StringBuilder result)
        {

        }

        internal virtual void RenderContent(StringBuilder result)
        {
            foreach (HtmlBase item in List)
            {
                item.Render(result);
            }
        }

        internal virtual void RenderEnd(StringBuilder result)
        {

        }
    }

    /// <summary>
    /// Html tree root.
    /// </summary>
    internal class HtmlDoc : HtmlBase
    {
        public HtmlDoc(Component owner)
            : base(owner)
        {

        }
    }

    internal class HtmlPage : HtmlBase
    {
        /// <summary>
        /// Constructor parse html.
        /// </summary>
        public HtmlPage(HtmlBase owner, SyntaxBase syntax)
            : base(owner, syntax)
        {

        }

        internal override void RenderBegin(StringBuilder result)
        {
            // result.Append("<html><head></head><body>");
        }

        internal override void RenderEnd(StringBuilder result)
        {
            // result.Append("</body></html>");
        }
    }

    internal class HtmlComment : HtmlBase
    {
        /// <summary>
        /// Constructor parse html.
        /// </summary>
        public HtmlComment(HtmlBase owner, SyntaxComment syntax)
            : base(owner, syntax)
        {

        }

        internal override void RenderContent(StringBuilder result)
        {
            result.Append(Syntax.Text);
        }
    }

    internal class HtmlTitle : HtmlBase
    {
        /// <summary>
        /// Constructor parse html.
        /// </summary>
        public HtmlTitle(HtmlBase owner, SyntaxTitle syntax)
            : base(owner, syntax)
        {

        }

        internal override void RenderBegin(StringBuilder result)
        {
            result.Append("<h1>");
        }

        internal override void RenderEnd(StringBuilder result)
        {
            result.Append("</h1>");
        }
    }

    internal class HtmlParagraph : HtmlBase
    {
        /// <summary>
        /// Constructor parse html.
        /// </summary>
        public HtmlParagraph(HtmlBase owner, SyntaxParagraph syntax)
            : base(owner, syntax)
        {

        }

        internal override void RenderBegin(StringBuilder result)
        {
            result.Append("<p>(p)");
        }

        internal override void RenderEnd(StringBuilder result)
        {
            result.Append("(/p)</p>");
        }
    }

    internal class HtmlBulletList : HtmlBase
    {
        /// <summary>
        /// Constructor parse html.
        /// </summary>
        public HtmlBulletList(HtmlBase owner)
            : base(owner, null)
        {

        }

        internal override void RenderBegin(StringBuilder result)
        {
            result.Append("<ul>");
        }

        internal override void RenderEnd(StringBuilder result)
        {
            result.Append("</ul>");
        }
    }

    internal class HtmlBulletItem : HtmlBase
    {
        /// <summary>
        /// Constructor parse html.
        /// </summary>
        public HtmlBulletItem(HtmlBulletList owner, SyntaxBullet syntax)
            : base(owner, syntax)
        {

        }

        internal override void RenderBegin(StringBuilder result)
        {
            result.Append("<li>");
        }

        internal override void RenderEnd(StringBuilder result)
        {
            result.Append("</li>");
        }
    }

    internal class HtmlFont : HtmlBase
    {
        /// <summary>
        /// Constructor parse html.
        /// </summary>
        public HtmlFont(HtmlBase owner, SyntaxFont syntax)
            : base(owner, syntax)
        {

        }

        public new SyntaxFont Syntax => (SyntaxFont)base.Syntax;

        internal override void RenderBegin(StringBuilder result)
        {
            switch (Syntax.FontEnum)
            {
                case MdFontEnum.Bold:
                    result.Append("<strong>");
                    break;
                case MdFontEnum.Italic:
                    result.Append("<i>");
                    break;
                default:
                    throw new Exception("Enum unknown!");
            }
        }

        internal override void RenderEnd(StringBuilder result)
        {
            switch (Syntax.FontEnum)
            {
                case MdFontEnum.Bold:
                    result.Append("</strong>");
                    break;
                case MdFontEnum.Italic:
                    result.Append("</i>");
                    break;
                default:
                    throw new Exception("Enum unknown!");
            }
        }
    }

    internal class HtmlLink : HtmlBase
    {
        /// <summary>
        /// Constructor parse html.
        /// </summary>
        public HtmlLink(HtmlBase owner, SyntaxLink syntax)
            : base(owner, syntax)
        {

        }

        public new SyntaxLink Syntax => (SyntaxLink)base.Syntax;

        internal override void RenderContent(StringBuilder result)
        {
            result.Append($"<a href=\"{ Syntax.Link }\">{ Syntax.LinkText }</a>");
        }
    }

    internal class HtmlImage : HtmlBase
    {
        /// <summary>
        /// Constructor parse html.
        /// </summary>
        public HtmlImage(HtmlBase owner, SyntaxImage syntax)
            : base(owner, syntax)
        {

        }

        public new SyntaxImage Syntax => (SyntaxImage)base.Syntax;

        internal override void RenderContent(StringBuilder result)
        {
            if (Syntax.LinkText == null)
            {
                result.Append($"<a src=\"{ Syntax.Link }\" />{ Syntax.LinkText }</a>");
            }
            else
            {
                result.Append($"<img src=\"{ Syntax.Link }\" alt=\"{ Syntax.LinkText }\" />");
            }
        }
    }

    internal class HtmlContent : HtmlBase
    {
        /// <summary>
        /// Constructor parse html.
        /// </summary>
        public HtmlContent(HtmlBase owner, SyntaxBase syntax)
            : base(owner, syntax)
        {

        }

        internal override void RenderContent(StringBuilder result)
        {
            result.Append(Syntax.Text);
        }
    }

    internal static class UtilDoc
    {
        public static void Debug()
        {
            string text =
@"
# Hello
World
* One 
* Two 
Click: [workplacex.org](https://workplacex.org)
* Three

![My Cli](https://workplacex.org/Doc/Cli.png)
";

            text =
@"
My

# F Hello [](https://workplacex.org)

ppp
";

            text = "abc def ghi";

            // Doc
            var appDoc = new AppDoc();
            var mdPage = new MdPage(appDoc.MdDoc, text);
            // var mdPage = new MdPage(appDoc, " #Title Hello\r\nWorld\nThis     is the <!-- --> \r ## Title \r\n![Image](a.png)");

            // mdPage.Parse(appDoc.SyntaxDocOne, appDoc.SyntaxDocTwo, appDoc.HtmlDoc);
            appDoc.Parse();

            appDoc.Serialize(out string json);
            var appDoc2 = Component.Deserialize<AppDoc>(json);

            var textDebug = TextDebug(appDoc);

            var textHtml = appDoc.HtmlDoc.Render();

            textDebug += "\r\n\r\n" + textHtml;

            File.WriteAllText(@"C:\Temp\Debug.txt", textDebug);
            File.WriteAllText(@"C:\Temp\Debug.html", textHtml);
            // Console.WriteLine(textDebug);
        }

        private static string TextDebug(string text)
        {
            return text?.Replace("\r", "\\r").Replace("\n", "\\n");
        }

        private static void TextDebug(Component component, int level, StringBuilder result)
        {
            for (int i = 0; i < level; i++)
            {
                result.Append("  ");
            }
            result.Append("-(" + component.GetType().Name + ")");

            // Token
            if (component is MdTokenBase token)
            {
                result.Append(" Text=\"" + TextDebug(token.Text) + "\";");
            }
            // Syntax
            if (component is SyntaxBase syntax)
            {
                result.Append(" Text=\"" + TextDebug(syntax.Text) + "\";");
            }
            // Html
            if (component is HtmlBase syntaxHtml)
            {
                result.Append(" Text=\"" + TextDebug(syntaxHtml.Syntax?.Text) + "\";");
            }

            result.AppendLine();
            foreach (var item in component.List)
            {
                TextDebug(item, level + 1, result);
            }
        }

        public static string TextDebug(Component component)
        {
            StringBuilder result = new StringBuilder();
            TextDebug(component, 0, result);
            return result.ToString();
        }

        public static void Assert(bool isAssert, string exceptionText)
        {
            if (!isAssert)
            {
                throw new Exception(exceptionText);
            }
        }

        public static void Assert(bool isAssert)
        {
            Assert(isAssert, "Assert!");
        }

        public static bool IsSubclassOf(Type type, Type typeBase)
        {
            return type == typeBase || type.IsSubclassOf(typeBase);
        }
    }
}
