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

        MdQuotation,

        MdSymbol,

        MdSpecial,

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

        SyntaxCustomNote,
        
        SyntaxCustomYoutube,

        SyntaxCustomPage,

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

        HtmlCode,

        HtmlCustomNote,
        
        HtmlCustomYoutube,

        HtmlContent,
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

        public Type ComponentType()
        {
            return Registry.DataEnumList[DataEnum];
        }

        public string Text { get; set; }

        public bool IsCommentEnd { get; set; }

        public int TitleLevel { get; set; }

        public string TitleId { get; set; }

        public MdBracketEnum BracketEnum { get; set; }

        public bool IsBracketEnd { get; set; }

        public MdQuotationEnum QuotationEnum { get; set; }

        public MdSymbolEnum SymbolEnum { get; set; }

        public MdFontEnum FontEnum { get; set; }

        public int TokenIdBegin { get; set; }

        public int TokenIdEnd { get; set; }

        public int TokenIndexBegin { get; set; }

        public int TokenIndexEnd { get; set; }

        public int? MdDocId { get; set; }

        public int? SyntaxDocOneId { get; set; }

        public int? SyntaxDocTwoId { get; set; }

        public int? SyntaxDocThreeId { get; set; }

        public int? SyntaxDocFourId { get; set; }

        public int? SyntaxDocFiveId { get; set; }

        public int? HtmlDocId { get; set; }

        public int? SyntaxId { get; set; }

        public string Link { get; set; }

        public string LinkText { get; set; }
        
        public string PagePath { get; set; }

        public string PageTitleHtml { get; set; }

        public string CodeLanguage { get; set; }

        public string CodeText { get; set; }

        public bool IsCreateNew { get; set; }

        public bool IsEndExceptional { get; set; }
    }

    /// <summary>
    /// Parse steps.
    /// </summary>
    public enum ParseEnum
    {
        None = 0,

        /// <summary>
        /// Parse stage one (SyntaxToken). Build syntax token from md token. Not hierarchical like md token.
        /// </summary>
        ParseOne = 1,

        /// <summary>
        /// Parse stage two (Block). Build blocks. Detect for example the end of bold font and build a block. Content goes inside.
        /// </summary>
        ParseTwo = 2,

        /// <summary>
        /// Parse stage three (Fold). Add following syntax components inside as long as it is valid child. Then break. For example title add following content inside.
        /// </summary>
        ParseThree = 3,

        /// <summary>
        /// Parse stage four (Owner Insert). For example insert paragraph (owner) for content directly on page.
        /// </summary>
        ParseFour = 4,

        /// <summary>
        /// Parse stage five (Owner Merge). For example merge two inserted paragraphs into one.
        /// </summary>
        ParseFive = 5,

        /// <summary>
        /// Parse stage html. Convert syntax component to html component.
        /// </summary>
        ParseHtml = 6,
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
            Add(typeof(MdQuotation));
            Add(typeof(MdSymbol));
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
            Add(typeof(SyntaxCustomNote));
            Add(typeof(SyntaxCustomYoutube));
            Add(typeof(SyntaxCustomPage));
            Add(typeof(SyntaxParagraph));
            Add(typeof(SyntaxNewLine));
            Add(typeof(SyntaxContent));
            Add(typeof(SyntaxIgnore)); // Needs to be last Syntax

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
            Add(typeof(HtmlCode));
            Add(typeof(HtmlCustomNote));
            Add(typeof(HtmlCustomYoutube));
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
        /// Gets SyntaxRegistry. Available if one SyntaxRegistry has been created out of this Registry.
        /// </summary>
        public SyntaxRegistry SyntaxRegistry;

        /// <summary>
        /// Gets or sets IsDebug. If true, for example html is rendered with additional debug information.
        /// </summary>
        public bool IsDebug;

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

        /// <summary>
        /// Gets or sets ParseEnum. This is the current parse step.
        /// </summary>
        public ParseEnum ParseEnum { get; set; }

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

        private Component(Component owner, Registry registry)
        {
            if (owner == null)
            {
                if (registry == null)
                {
                    registry = new Registry();
                }
                var dataEnum = registry.TypeList[GetType()];
                Data = new DataDoc { Registry = registry, Id = registry.IdCount += 1, DataEnum = dataEnum };
                registry.IdList.Add(Data.Id, Data);
            }
            else
            {
                UtilDoc.Assert(registry == null);
                registry = owner.Data.Registry;
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

        /// <summary>
        /// Constructor root.
        /// </summary>
        public Component()
            : this(null, null)
        {

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public Component(Component owner)
            : this(owner, null)
        {

        }

        /// <summary>
        /// Constructor registry, factory mode. Root with existing registry.
        /// </summary>
        public Component(Registry registry)
            : this(null, registry)
        {

        }

        /// <summary>
        /// Gets Id. Unique Id in component tree.
        /// </summary>
        public int Id => Data.Id;

        /// <summary>
        /// Gets Owner. This is the components owner in tree.
        /// </summary>
        public Component Owner => Data.Owner?.Component();

        public T OwnerFind<T>() where T : Component
        {
            T result = null;
            var dataEnum = Data.Registry.TypeList[typeof(T)];
            var next = Data;
            do
            {
                if (next.DataEnum == dataEnum)
                {
                    result = (T)next.Component();
                    break;
                }
                next = next.Owner;
            } while (next != null);
            return result;
        }

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

        private static void ListAll(Component component, List<Component> result)
        {
            result.Add(component);
            foreach (var item in component.List)
            {
                ListAll(item, result);
            }
        }

        /// <summary>
        /// Returns list of all child components recursive including this.
        /// </summary>
        public IReadOnlyList<Component> ListAll()
        {
            var result = new List<Component>();
            ListAll(this, result);
            return result;
        }

        /// <summary>
        /// Remove component from owner. Needs to be last component.
        /// </summary>
        public void Remove()
        {
            UtilDoc.Assert(Data.Owner != null);
            UtilDoc.Assert(Data.Owner.List.Last() == Data);
            Data.Owner.List.Remove(this.Data);
            Data.Owner = null;
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

        public T Next<T>() where T : Component
        {
            var result = Next(null, offset: 1);
            return (T)result;
        }

        public Component Next()
        {
            var result = Next(null, offset: 1);
            return result;
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
        public T Previous<T>() where T : Component
        {
            var result = Next(null, offset: -1);
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
            : base()
        {
            MdDoc = new MdDoc(this);
            SyntaxDocOne = new SyntaxDoc(this);
            SyntaxDocTwo = new SyntaxDoc(this);
            SyntaxDocThree = new SyntaxDoc(this);
            SyntaxDocFour = new SyntaxDoc(this);
            SyntaxDocFive = new SyntaxDoc(this);
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

            syntaxRegistry.Parse(this);
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

        public SyntaxDoc SyntaxDocThree
        {
            get => Data.Registry.ReferenceGet<SyntaxDoc>(Data.SyntaxDocThreeId);
            set => Data.SyntaxDocThreeId = Registry.ReferenceSet(value);
        }

        public SyntaxDoc SyntaxDocFour
        {
            get => Data.Registry.ReferenceGet<SyntaxDoc>(Data.SyntaxDocFourId);
            set => Data.SyntaxDocFourId = Registry.ReferenceSet(value);
        }

        public SyntaxDoc SyntaxDocFive
        {
            get => Data.Registry.ReferenceGet<SyntaxDoc>(Data.SyntaxDocFiveId);
            set => Data.SyntaxDocFiveId = Registry.ReferenceSet(value);
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

            while (MdTokenBase.ParseMain(this, owner, text)) ;
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
            : base()
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

            Data.TokenIndexBegin = indexBegin;
            Data.TokenIndexEnd = Data.TokenIndexBegin + length - 1;

            UtilDoc.Assert(Data.TokenIndexEnd <= owner.Text.Length);
        }

        public new MdPage Owner => (MdPage)base.Owner;

        public int IndexBegin => Data.TokenIndexBegin;

        public int IndexEnd => Data.TokenIndexEnd;

        public int Length => IndexEnd - IndexBegin + 1;

        internal void IndexEndSet(int index)
        {
            var isLast = Owner.Data.Last() == Data;
            UtilDoc.Assert(isLast, "Can only set IndexEnd of last token!");

            UtilDoc.Assert(index >= 0 && index < Owner.Text.Length, "Index out of range!");

            Data.TokenIndexEnd = index;
        }

        /// <summary>
        /// Gets Text. This is the text between IndexBegin and IndexEnd.
        /// </summary>
        public string Text
        {
            get
            {
                return Owner.Text.Substring(Data.TokenIndexBegin, Data.TokenIndexEnd - Data.TokenIndexBegin + 1);
            }
        }

        /// <summary>
        /// Main entry for parse md.
        /// </summary>
        internal static bool ParseMain(MdRegistry registry, MdPage owner, ReadOnlySpan<char> text, bool isExcludeContent = false)
        {
            var result = false;

            var index = UtilParse.ParseIndex(owner);

            foreach (var tokenParser in registry.List)
            {
                if (isExcludeContent)
                {
                    if (tokenParser.GetType() == typeof(MdContent))
                    {
                        break;
                    }
                }

                // Parse
                var countBefore = owner.Data.ListCount();
                tokenParser.Parse(registry, owner, text, index);
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
                switch (BracketEnum)
                {
                    case MdBracketEnum.Round:
                        if (IsBracketEnd == false)
                        {
                            return "(";
                        }
                        else
                        {
                            return ")";
                        }
                    case MdBracketEnum.Square:
                        if (IsBracketEnd == false)
                        {
                            return "[";
                        }
                        else
                        {
                            return "]";
                        }
                    default:
                        throw new Exception("Enum unknown!");
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

    internal enum MdQuotationEnum
    {
        None,

        Single,

        Double,
    }

    internal class MdQuotation : MdTokenBase
    {
        /// <summary>
        /// Constructor registry, factory mode.
        /// </summary>
        public MdQuotation()
            : base()
        {

        }

        /// <summary>
        /// Constructor instance token.
        /// </summary>
        public MdQuotation(MdPage owner, int length, MdQuotationEnum quotationEnum)
            : base(owner, length)
        {
            Data.QuotationEnum = quotationEnum;
        }

        public MdQuotationEnum QuotationEnum => Data.QuotationEnum;

        public string TextQuotation
        {
            get
            {
                switch (QuotationEnum)
                {
                    case MdQuotationEnum.Single:
                        return "'";
                    case MdQuotationEnum.Double:
                        return "\"";
                    default:
                        throw new Exception("Enum unknown!");
                }
            }
        }

        internal override void Parse(MdRegistry registry, MdPage owner, ReadOnlySpan<char> text, int index)
        {
            var textChar = text.Char(index);
            switch (textChar)
            {
                case '\'':
                    new MdQuotation(owner, 1, MdQuotationEnum.Single);
                    break;
                case '"':
                    new MdQuotation(owner, 1, MdQuotationEnum.Double);
                    break;
            }
        }
    }

    internal enum MdSymbolEnum
    {
        None,

        Equal,
    }

    internal class MdSymbol : MdTokenBase
    {
        /// <summary>
        /// Constructor registry, factory mode.
        /// </summary>
        public MdSymbol()
            : base()
        {

        }

        /// <summary>
        /// Constructor instance token.
        /// </summary>
        public MdSymbol(MdPage owner, int length, MdSymbolEnum symbolEnum)
            : base(owner, length)
        {
            Data.SymbolEnum = symbolEnum;
        }

        public MdSymbolEnum SymbolEnum => Data.SymbolEnum;

        public string TextSymbol
        {
            get
            {
                switch (SymbolEnum)
                {
                    case MdSymbolEnum.Equal:
                        return "=";
                    default:
                        throw new Exception("Enum unknown!");
                }
            }
        }

        internal override void Parse(MdRegistry registry, MdPage owner, ReadOnlySpan<char> text, int index)
        {
            var textChar = text.Char(index);
            switch (textChar)
            {
                case '=':
                    new MdSymbol(owner, 1, MdSymbolEnum.Equal);
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
            while (UtilParse.ParseIndex(owner) < text.Length)
            {
                if (ParseMain(registry, owner, text, isExcludeContent: true) == false)
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
            UtilDoc.Assert(registry.SyntaxRegistry == null);
            registry.SyntaxRegistry = this;

            // Init SchemaTypeList
            foreach (var type in registry.List)
            {
                SchemaTypeList[type] = new List<Type>();
            }

            SchemaTypeIsBlockList = new Dictionary<Type, bool>();

            foreach (var type in registry.List)
            {
                if (type.IsSubclassOf(typeof(SyntaxBase)))
                {
                    // Create instance for registry, factory mode.

                    // Call new SyntaxBase(registry);
                    var syntaxParser = (SyntaxBase)type.GetConstructor(new Type[] { typeof(Registry) }).Invoke(new object[] { registry }); // Activator
                    List.Add(syntaxParser);
                    TypeList.Add(type, syntaxParser);

                    // SchemaTypeList
                    var result = new SyntaxBase.RegistrySchemaResult();
                    syntaxParser.RegistrySchema(result);
                    SchemaOwnerTypeList.Add(type, result.List.Where(item => item.IsChild == true).Select(item => item.OwnerType).ToList());
                    foreach (var item in result.List.Select(item => item.OwnerType).ToList())
                    {
                        SchemaTypeList[item].Add(type);
                    }

                    SchemaTypeIsBlockList[type] = result.IsBlock;
                }
            }
        }

        /// <summary>
        /// (Syntax). Syntax parser list.
        /// </summary>
        public List<SyntaxBase> List = new List<SyntaxBase>();

        /// <summary>
        /// (Type, Syntax). Syntax parser list.
        /// </summary>
        public Dictionary<Type, SyntaxBase> TypeList = new Dictionary<Type, SyntaxBase>();

        /// <summary>
        /// (OwnerType, Type). Owner type, child type. Contains all possible (and not valid) owner. See also property IsChildDirect.
        /// </summary>
        public Dictionary<Type, List<Type>> SchemaTypeList = new Dictionary<Type, List<Type>>();

        /// <summary>
        /// (Type, OwnerType). Child type, owner type. Contains all possible (and valid) owner. See also property IsChildDirect.
        /// </summary>
        public Dictionary<Type, List<Type>> SchemaOwnerTypeList = new Dictionary<Type, List<Type>>();

        /// <summary>
        /// (Type, IsBlock). For example true for font and comment. False for title.
        /// </summary>
        public Dictionary<Type, bool> SchemaTypeIsBlockList = new Dictionary<Type, bool>();

        public void Parse(AppDoc appDoc)
        {
            var mdDoc = appDoc.MdDoc;
            var syntaxDocOne = appDoc.SyntaxDocOne;
            var syntaxDocTwo = appDoc.SyntaxDocTwo;
            var syntaxDocThree = appDoc.SyntaxDocThree;
            var syntaxDocFour = appDoc.SyntaxDocFour;
            var syntaxDocFive = appDoc.SyntaxDocFive;
            var htmlDoc = appDoc.HtmlDoc;

            // ParseOne
            appDoc.Data.Registry.ParseEnum = ParseEnum.ParseOne;
            foreach (MdPage page in mdDoc.List)
            {
                if (page.Data.ListCount() > 0)
                {
                    var tokenBegin = (MdTokenBase)page.Data.List[0].Component();
                    var tokenEnd = (MdTokenBase)page.Data.List[page.Data.List.Count - 1].Component();
                    var syntaxPage = new SyntaxPage(syntaxDocOne, tokenBegin, tokenEnd);
                    SyntaxBase.ParseOneMain(syntaxPage);
                }
            }

            // ParseTwo
            appDoc.Data.Registry.ParseEnum = ParseEnum.ParseTwo;
            SyntaxBase.ParseMain(syntaxDocTwo, syntaxDocOne);

            // ParseThree
            appDoc.Data.Registry.ParseEnum = ParseEnum.ParseThree;
            SyntaxBase.ParseMain(syntaxDocThree, syntaxDocTwo);

            // ParseFour
            appDoc.Data.Registry.ParseEnum = ParseEnum.ParseFour;
            SyntaxBase.ParseMain(syntaxDocFour, syntaxDocThree);

            // ParseFive
            appDoc.Data.Registry.ParseEnum = ParseEnum.ParseFive;
            SyntaxBase.ParseMain(syntaxDocFive, syntaxDocFour);

            // ParseHtml
            appDoc.Data.Registry.ParseEnum = ParseEnum.ParseHtml;
            syntaxDocFive.ParseHtml(htmlDoc);
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
        public SyntaxBase(Registry registry)
            : base(registry)
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
        /// Create instance of this object in registry, factory mode.
        /// </summary>
        protected internal virtual SyntaxBase Create(SyntaxBase owner, SyntaxBase syntax)
        {
            throw new Exception("Not implemented!");
        }

        /// <summary>
        /// Override this method to register possible owner types.
        /// </summary>
        internal virtual void RegistrySchema(RegistrySchemaResult result)
        {

        }

        internal class RegistrySchemaResult
        {
            public List<RegistrySchemaResultItem> List = new List<RegistrySchemaResultItem>();

            internal class RegistrySchemaResultItem
            {
                public Type OwnerType;

                public bool IsChild;
            }

            /// <summary>
            /// Gets or sets IsBlock. True, if syntax has a corresponding end syntax. For example true for font and comment. False for title.
            /// </summary>
            public bool IsBlock;

            /// <summary>
            /// Register possible owner.
            /// </summary>
            /// <typeparam name="T">This syntax can be a child of owner.</typeparam>
            /// <param name="isChildDirect">This syntax can be a direct child of owner. Otherwise first owner in list is created in between in ParseFour (Owner Insert).</param>
            public void AddOwner<T>(bool isChildDirect = true) where T : SyntaxBase
            {
                List.Add(new RegistrySchemaResultItem { OwnerType = typeof(T), IsChild = isChildDirect });
            }
        }

        /// <summary>
        /// Constructor ParseOne.
        /// </summary>
        public SyntaxBase(SyntaxBase owner, MdTokenBase tokenBegin, MdTokenBase tokenEnd)
            : base(owner)
        {
            UtilDoc.Assert(owner.Data.Registry.ParseEnum == ParseEnum.ParseOne);

            Data.TokenIdBegin = tokenBegin.Data.Id;
            Data.TokenIdEnd = tokenEnd.Data.Id;

            Data.TokenIndexBegin = tokenBegin.Data.TokenIndexBegin;
            Data.TokenIndexEnd = tokenEnd.Data.TokenIndexEnd;

            CreateValidate();
        }

        /// <summary>
        /// Constructor ParseTwo, ParseThree, ParseFour and ParseFive.
        /// </summary>
        public SyntaxBase(SyntaxBase owner, SyntaxBase syntaxBegin, SyntaxBase syntaxEnd)
            : base(owner)
        {
            var parseEnum = owner.Data.Registry.ParseEnum;
            UtilDoc.Assert(parseEnum == ParseEnum.ParseTwo || parseEnum == ParseEnum.ParseThree || parseEnum == ParseEnum.ParseFour || parseEnum == ParseEnum.ParseFive);

            Data.SyntaxId = syntaxBegin.Data.Id;
            Data.TokenIdBegin = syntaxBegin.TokenBegin.Data.Id;
            Data.TokenIdEnd = syntaxEnd.TokenEnd.Data.Id;

            Data.TokenIndexBegin = syntaxBegin.TokenBegin.Data.TokenIndexBegin;
            Data.TokenIndexEnd = syntaxEnd.TokenEnd.Data.TokenIndexEnd;

            // Enable, disable (for debug only) ParseThree validate.
            if (parseEnum == ParseEnum.ParseThree)
            {
                // Uncomment return to disable ParseThree validate.
                // No more exceptions but wrong sequence of components in output!

                // return;
            }

            if (owner is not SyntaxDoc)
            {
                var indexBegin = Index(syntaxBegin.Data.TokenIdBegin);
                var indexEnd = Index(syntaxEnd.Data.TokenIdEnd);
                var indexEndOwner = Index(owner.Data.TokenIdEnd);
                bool isShorten = indexEnd <= indexEndOwner;
                bool isExtend = indexBegin == indexEndOwner + 1;
                UtilDoc.Assert(isShorten ^ isExtend); // If exception in ParseThree, disable (for debug only) ParseThree validate above. Caused by wrong break in method ParseTwo calling method ParseTwoMainBreak.
                var next = owner.Data;
                while (next.DataEnum != DataEnum.SyntaxDoc)
                {
                    UtilDoc.Assert(next.Owner.List.Last() == next); // Modify TokenIdEnd only on last item
                    next.TokenIdEnd = syntaxEnd.Data.TokenIdEnd; // Modify owner TokenIdEnd
                    next.TokenIndexEnd = syntaxEnd.TokenEnd.Data.TokenIndexEnd;
                    next = next.Owner;
                }
                UtilDoc.Assert(Index(owner.Data.TokenIdBegin) <= Index(owner.Data.TokenIdEnd));
            }

            CreateValidate();
        }

        /// <summary>
        /// Constructor ParseTwo, ParseThree, ParseFour and ParseFive.
        /// </summary>
        public SyntaxBase(SyntaxBase owner, SyntaxBase syntax)
            : this(owner, syntax, syntax)
        {

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
                    UtilDoc.Assert(Index(Data.Owner.TokenIdBegin) <= Index(Data.TokenIdBegin));
                    UtilDoc.Assert(Index(Data.Owner.TokenIdEnd) >= Index(Data.TokenIdEnd));
                    if (Data.Index == 0)
                    {
                        UtilDoc.Assert(Index(Data.Owner.TokenIdBegin) == Index(Data.TokenIdBegin));
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
                return UtilDoc.StringNull(result.ToString());
            }
        }

        /// <summary>
        /// Gets IsCreateNew. If true, syntax has been inserted by ParseThree.
        /// </summary>
        public bool IsCreateNew => Data.IsCreateNew;

        /// <summary>
        /// Main entry for ParseOne.
        /// </summary>
        internal static void ParseOneMain(SyntaxPage owner)
        {
            var registry = owner.Data.Registry.SyntaxRegistry;

            MdTokenBase token;
            while ((token = UtilParse.ParseOneToken(owner)) != null)
            {
                bool isFind = false;
                foreach (var syntaxParser in registry.List)
                {
                    var countBefore = owner.Data.ListCount();
                    syntaxParser.ParseOne(owner, token);
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

        /// <summary>
        /// Main entry for ParseTwo, ParseThree, ParseFour, ParseFive.
        /// </summary>
        internal static void ParseMain(SyntaxBase owner, IReadOnlyList<Component> list)
        {
            bool isFirst = owner.Data.ListCount() == 0;
            foreach (SyntaxBase item in list)
            {
                if (item.TokenEnd.IndexEnd <= owner.Data.List?.Last().TokenIndexEnd)
                {
                    continue;
                }

                var countBefore = owner.Data.ListCount();
                var indexEndBefore = owner.Data.TokenIndexEnd;
                var countOwnerBefore = owner.Data.Owner?.ListCount();
                switch (owner.Data.Registry.ParseEnum)
                {
                    case ParseEnum.ParseTwo:
                        item.ParseTwo(owner);
                        break;
                    case ParseEnum.ParseThree:
                        item.ParseThree(owner);
                        break;
                    case ParseEnum.ParseFour:
                        item.ParseFour(owner);
                        break;
                    case ParseEnum.ParseFive:
                        item.ParseFive(owner);
                        break;
                    default:
                        throw new Exception("Enum unknown!");
                }
                var countAfter = owner.Data.ListCount();
                var indexEndAfter = owner.Data.TokenIndexEnd;
                var countOwnerAfter = owner.Data.Owner?.ListCount();

                // After first child component has been created IndexEnd is smaller.
                UtilDoc.Assert((isFirst && indexEndBefore >= indexEndAfter) || (!isFirst && indexEndBefore <= indexEndAfter));
                bool isFind = false;
                if ((isFirst && indexEndBefore > indexEndAfter) || (!isFirst && indexEndBefore < indexEndAfter))
                {
                    isFind = true;
                }

                // Component has been created and parse is completed.
                if (indexEndBefore == indexEndAfter && countBefore < countAfter)
                {
                    isFind = true;
                }

                // Component has been created and added to owner.Owner.
                if (countOwnerBefore < countOwnerAfter)
                {
                    isFind = true;
                }

                if (!isFind)
                {
                    var registry = owner.Data.Registry.SyntaxRegistry;
                    var ownerLocal = UtilParse.Create(registry, owner, item);
                    ParseMain(ownerLocal, item);
                }

                isFirst = false;
            }
        }

        internal static void ParseMain(SyntaxBase owner, SyntaxBase syntax)
        {
            if (syntax.Data.ListCount() > 0)
            {
                ParseMain(owner, syntax.List);
            }
        }

        /// <summary>
        /// Main entry for ParseHtml.
        /// </summary>
        internal static void ParseHtmlMain(HtmlBase owner, SyntaxBase syntax)
        {
            foreach (SyntaxBase item in syntax.List)
            {
                item.ParseHtml(owner);
            }
        }

        /// <summary>
        /// Parse md token between tokenBegin and tokenEnd.
        /// </summary>
        internal virtual void ParseOne(SyntaxBase owner, MdTokenBase token)
        {

        }

        /// <summary>
        /// Transform this syntax source to owner dest.
        /// </summary>
        /// <param name="owner">Owner dest.</param>
        internal virtual void ParseTwo(SyntaxBase owner)
        {

        }

        /// <summary>
        /// Transform this syntax source to owner dest.
        /// </summary>
        /// <param name="owner">Owner dest.</param>
        internal virtual void ParseThree(SyntaxBase owner)
        {
            var registry = Data.Registry.SyntaxRegistry;
            var ownerLocal = UtilParse.Create(registry, owner, this);
            ParseMain(ownerLocal, this);
            if (!registry.SchemaTypeIsBlockList[GetType()])
            {
                var next = Next<SyntaxBase>();
                bool isIgnore = false;
                while (next != null && registry.SchemaTypeList[GetType()].Contains(next.GetType()))
                {
                    if (!isIgnore)
                    {
                        isIgnore = true;
                        new SyntaxIgnore(ownerLocal, this);
                    }
                    var ownerLocalLocal = UtilParse.Create(registry, ownerLocal, next);
                    ParseMain(ownerLocalLocal, next);
                    next = next.Next<SyntaxBase>();
                }
            }
        }

        /// <summary>
        /// Transform this syntax source to owner dest.
        /// </summary>
        /// <param name="owner">Owner dest.</param>
        internal void ParseFour(SyntaxBase owner)
        {
            var registry = Data.Registry.SyntaxRegistry;
            SyntaxBase ownerLocal = owner;

            // Owner insert if owner can not be direct owner.
            var isOwnerInsert = false;
            if (!registry.SchemaOwnerTypeList[GetType()].Contains(owner.GetType()))
            {
                // Owner insert
                var ownerType = registry.SchemaOwnerTypeList[GetType()].First();
                ownerLocal = UtilParse.Create(registry, owner, this, ownerType);
                ownerLocal.Data.IsCreateNew = true;
                isOwnerInsert = true;
            }

            // Insert for example paragraph to comment in order to merge later.
            if (!isOwnerInsert)
            {
                var ownerLast = owner.Data.List?.Last();
                if (ownerLast?.IsCreateNew == true)
                {
                    var ownerLastType = ownerLast.ComponentType();
                    if (registry.SchemaOwnerTypeList[GetType()].Contains(ownerLastType))
                    {
                        // Owner insert
                        ownerLocal = UtilParse.Create(registry, owner, this, ownerLastType);
                        ownerLocal.Data.IsCreateNew = true;
                    }
                }
            }

            // Syntax create
            var syntax = UtilParse.Create(registry, ownerLocal, this);
            if (Data.ListCount() > 0)
            {
                foreach (SyntaxBase item in List)
                {
                    item.ParseFour(syntax);
                }
            }
        }

        internal void ParseFive(SyntaxBase owner)
        {
            var registry = Data.Registry.SyntaxRegistry;

            SyntaxBase ownerLocal;

            var previous = Previous<SyntaxBase>();

            if (IsCreateNew && previous?.GetType() == GetType())
            {
                // Merge with previous
                ownerLocal = (SyntaxBase)owner.Data.List.Last().Component();
                if (ownerLocal.Data.ListCount() == 0)
                {
                    new SyntaxIgnore(ownerLocal, previous);
                }
            }
            else
            {
                // Syntax create
                ownerLocal = UtilParse.Create(registry, owner, this);
                ownerLocal.Data.IsCreateNew = IsCreateNew;
            }

            if (Data.ListCount() > 0)
            {
                foreach (SyntaxBase item in List)
                {
                    item.ParseFive(ownerLocal);
                }
            }
        }

        /// <summary>
        /// Override this method to custom transform syntax tree ParseTwo into html.
        /// </summary>
        internal virtual void ParseHtml(HtmlBase owner)
        {
            ParseHtmlMain(owner, this);
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
        public SyntaxDoc(Registry registry)
            : base(registry)
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
        public SyntaxPage(Registry registry)
            : base(registry)
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
        /// Constructor ParseTwo, ParseThree, ParseFour and ParseFive.
        /// </summary>
        public SyntaxPage(SyntaxBase owner, SyntaxBase syntax)
            : base(owner, syntax)
        {
            UtilDoc.Assert(owner is SyntaxDoc);
            if (syntax is SyntaxCustomPage)
            {
                Data.PagePath = ((SyntaxCustomPage)syntax).PagePath;
                Data.PageTitleHtml = ((SyntaxCustomPage)syntax).PageTitleHtml;
            }
            else
            {
                Data.PagePath = ((SyntaxPage)syntax).PagePath;
                Data.PageTitleHtml = ((SyntaxPage)syntax).PageTitleHtml;
            }
        }

        /// <summary>
        /// Gets PagePath. Custom parameter of class SyntaxCustomPage.
        /// </summary>
        public string PagePath => Data.PagePath;

        /// <summary>
        /// Gets PageTitleHtml. Custom parameter of class SyntaxCustomPage.
        /// </summary>
        public string PageTitleHtml => Data.PageTitleHtml;

        internal override void RegistrySchema(RegistrySchemaResult result)
        {
            result.AddOwner<SyntaxDoc>();
        }

        protected internal override SyntaxBase Create(SyntaxBase owner, SyntaxBase syntax)
        {
            return new SyntaxPage(owner, syntax);
        }

        internal override void ParseHtml(HtmlBase owner)
        {
            var page = new HtmlPage(owner, this);

            ParseHtmlMain(page, this);
        }
    }

    internal class SyntaxComment : SyntaxBase
    {
        /// <summary>
        /// Constructor registry, factory mode.
        /// </summary>
        public SyntaxComment(Registry registry)
            : base(registry)
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
        /// Constructor ParseTwo, ParseThree, ParseFour and ParseFive.
        /// </summary>
        public SyntaxComment(SyntaxBase owner, SyntaxBase syntax)
            : base(owner, syntax)
        {

        }

        /// <summary>
        /// Constructor ParseTwo, ParseThree, ParseFour and ParseFive.
        /// </summary>
        public SyntaxComment(SyntaxBase owner, SyntaxBase syntaxBegin, SyntaxBase syntaxEnd)
            : base(owner, syntaxBegin, syntaxEnd)
        {

        }

        internal override void RegistrySchema(RegistrySchemaResult result)
        {
            result.IsBlock = true;
            result.AddOwner<SyntaxPage>();
            result.AddOwner<SyntaxParagraph>();
            result.AddOwner<SyntaxBullet>();
        }

        protected internal override SyntaxBase Create(SyntaxBase owner, SyntaxBase syntax)
        {
            return new SyntaxComment(owner, syntax);
        }

        internal override void ParseOne(SyntaxBase owner, MdTokenBase token)
        {
            if (token is MdComment)
            {
                new SyntaxComment(owner, token, token);
            }
        }

        internal override void ParseTwo(SyntaxBase owner)
        {
            SyntaxBase next = this;
            do
            {
                next = next.Next<SyntaxBase>(null);
                if (next is SyntaxComment comment && comment.Text == "-->")
                {
                    new SyntaxComment(owner, this, comment);
                    break;
                }
            } while (next != null);
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
        public SyntaxTitle(Registry registry)
            : base(registry)
        {

        }

        /// <summary>
        /// Constructor ParseOne.
        /// </summary>
        public SyntaxTitle(SyntaxBase owner, MdTokenBase tokenBegin, MdTokenBase tokenEnd, int titleLevel)
            : base(owner, tokenBegin, tokenEnd)
        {
            Data.TitleLevel = titleLevel;
        }

        /// <summary>
        /// Constructor ParseTwo, ParseThree, ParseFour and ParseFive.
        /// </summary>
        public SyntaxTitle(SyntaxBase owner, SyntaxBase syntax)
            : base(owner, syntax)
        {
            UtilDoc.Assert(owner is SyntaxPage || owner is SyntaxCustomPage);
            Data.TitleLevel = ((SyntaxTitle)syntax).TitleLevel;
            Data.TitleId = ((SyntaxTitle)syntax).TitleId;
            if (Data.Registry.IsDebug == false)
            {
                if (TitleId == null)
                {
                    var contentList = syntax.List.OfType<SyntaxContent>().ToList();
                    if (contentList.Count > 0)
                    {
                        foreach (var item in contentList)
                        {
                            Data.TitleId += item.Text.ToLower().Replace(" ", "-").Replace("\"", "");
                        }
                        // Title contains html. For example: <i class="fas fa-info-circle"></i>
                        var index = Data.TitleId.IndexOf("<");
                        if (index != -1)
                        {
                            Data.TitleId = Data.TitleId.Substring(0, index);
                        }
                        while (Data.TitleId.EndsWith("-"))
                        {
                            Data.TitleId = Data.TitleId.Substring(0, Data.TitleId.Length - 1);
                        }
                    }
                }
            }
        }

        public int TitleLevel => Data.TitleLevel;

        /// <summary>
        /// Gets TitleId. Used for html named anchor.
        /// </summary>
        public string TitleId => Data.TitleId;

        protected internal override SyntaxBase Create(SyntaxBase owner, SyntaxBase syntax)
        {
            return new SyntaxTitle(owner, syntax);
        }

        internal override void RegistrySchema(RegistrySchemaResult result)
        {
            result.AddOwner<SyntaxPage>();
        }

        internal override void ParseOne(SyntaxBase owner, MdTokenBase token)
        {
            if (UtilParse.ParseOneIsNewLine<MdTitle>(token, out var tokenEnd))
            {
                new SyntaxTitle(owner, token, tokenEnd, tokenEnd.TitleLevel);
                if (tokenEnd.Next() is MdSpace tokenSpace)
                {
                    // Trailing space
                    new SyntaxIgnore(owner, tokenSpace);
                }
            }
        }

        internal override void ParseHtml(HtmlBase owner)
        {
            var title = new HtmlTitle(owner, this);

            ParseHtmlMain(title, this);
        }
    }

    internal class SyntaxBullet : SyntaxBase
    {
        /// <summary>
        /// Constructor registry, factory mode.
        /// </summary>
        public SyntaxBullet(Registry registry)
            : base(registry)
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
        /// Constructor ParseTwo, ParseThree, ParseFour and ParseFive.
        /// </summary>
        public SyntaxBullet(SyntaxBase owner, SyntaxBase syntax)
            : base(owner, syntax)
        {

        }

        protected internal override SyntaxBase Create(SyntaxBase owner, SyntaxBase syntax)
        {
            return new SyntaxBullet(owner, syntax);
        }

        internal override void RegistrySchema(RegistrySchemaResult result)
        {
            result.AddOwner<SyntaxPage>();
        }

        internal override void ParseOne(SyntaxBase owner, MdTokenBase token)
        {
            if (UtilParse.ParseOneIsNewLine<MdBullet>(token, out var tokenEnd))
            {
                new SyntaxBullet(owner, token, tokenEnd);
                if (tokenEnd.Next() is MdSpace tokenSpace)
                {
                    // Trailing space
                    new SyntaxIgnore(owner, tokenSpace);
                }
            }
        }

        internal override void ParseHtml(HtmlBase owner)
        {
            var bulletList = owner.Data.List?.Last().Component() as HtmlBulletList;
            if (bulletList == null)
            {
                bulletList = new HtmlBulletList(owner);
            }
            var bulletItem = new HtmlBulletItem(bulletList, this);

            ParseHtmlMain(bulletItem, this);
        }
    }

    internal class SyntaxCode : SyntaxBase
    {
        /// <summary>
        /// Constructor registry, factory mode.
        /// </summary>
        public SyntaxCode(Registry registry)
            : base(registry)
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
        /// Constructor ParseTwo, ParseThree, ParseFour and ParseFive.
        /// </summary>
        public SyntaxCode(SyntaxBase owner, SyntaxBase syntax)
            : base(owner, syntax)
        {
            Data.CodeLanguage = ((SyntaxCode)syntax).CodeLanguage;
            Data.CodeText = ((SyntaxCode)syntax).CodeText;
        }

        protected internal override SyntaxBase Create(SyntaxBase owner, SyntaxBase syntax)
        {
            return new SyntaxCode(owner, syntax);
        }

        internal override void RegistrySchema(RegistrySchemaResult result)
        {
            result.IsBlock = true;
            result.AddOwner<SyntaxPage>();
        }

        public string CodeLanguage => Data.CodeLanguage;

        public string CodeText
        {
            get => Data.CodeText;
            set => Data.CodeText = value;
        }

        internal override void ParseOne(SyntaxBase owner, MdTokenBase token)
        {
            if (UtilParse.ParseOneIsNewLine<MdCode>(token, out var tokenEnd))
            {
                var next = tokenEnd.Next<MdTokenBase>();

                // Code language
                string codeLanguage = null;
                if (next is MdContent content)
                {
                    codeLanguage = content.Text;
                    next = next.Next<MdTokenBase>();
                }

                List<MdTokenBase> contentList = new List<MdTokenBase>();

                bool isFind = false;
                while (next != null)
                {
                    if (next is MdCode)
                    {
                        isFind = true;
                        break;
                    }
                    contentList.Add(next);
                    next = next.Next<MdTokenBase>();
                }

                var codeText = new StringBuilder();
                foreach (var item in contentList)
                {
                    codeText.Append(item.Text);
                }

                if (isFind)
                {
                    new SyntaxCode(owner, token, next, codeLanguage) { CodeText = codeText.ToString() };
                }
            }
        }

        internal override void ParseHtml(HtmlBase owner)
        {
            new HtmlCode(owner, this);
        }
    }

    internal class SyntaxFont : SyntaxBase
    {
        /// <summary>
        /// Constructor registry, factory mode.
        /// </summary>
        public SyntaxFont(Registry registry)
            : base(registry)
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
        /// Constructor ParseTwo, ParseThree, ParseFour and ParseFive.
        /// </summary>
        public SyntaxFont(SyntaxBase owner, SyntaxBase syntax)
            : base(owner, syntax)
        {

        }

        /// <summary>
        /// Constructor ParseTwo, ParseThree, ParseFour and ParseFive.
        /// </summary>
        public SyntaxFont(SyntaxBase owner, SyntaxBase syntaxBegin, SyntaxBase syntaxEnd)
            : base(owner, syntaxBegin, syntaxEnd)
        {

        }

        protected internal override SyntaxBase Create(SyntaxBase owner, SyntaxBase syntax)
        {
            return new SyntaxFont(owner, syntax);
        }

        internal override void RegistrySchema(RegistrySchemaResult result)
        {
            result.IsBlock = true;
            result.AddOwner<SyntaxParagraph>();
            result.AddOwner<SyntaxTitle>();
            result.AddOwner<SyntaxBullet>();
            result.AddOwner<SyntaxCustomNote>(false);
        }

        public MdFontEnum FontEnum => ((MdFont)TokenBegin).FontEnum;

        internal override void ParseOne(SyntaxBase owner, MdTokenBase token)
        {
            if (token is MdFont)
            {
                new SyntaxFont(owner, token, token);
            }
        }

        internal override void ParseTwo(SyntaxBase owner)
        {
            var registry = Data.Registry.SyntaxRegistry;

            var next = Next<SyntaxBase>();
            List<SyntaxBase> contentList = new List<SyntaxBase>();
            bool isFind = false;
            while (next != null)
            {
                if (next is SyntaxFont fontSource && FontEnum == fontSource.FontEnum)
                {
                    var fontDest = new SyntaxFont(owner, this, fontSource);
                    new SyntaxIgnore(fontDest, this);
                    foreach (var item in contentList)
                    {
                        UtilParse.Create(registry, fontDest, item);
                    }
                    new SyntaxIgnore(fontDest, fontSource);

                    isFind = true;
                    break;
                }
                if (!registry.SchemaOwnerTypeList[next.GetType()].Contains(this.GetType()))
                {
                    break;
                }
                contentList.Add(next);
                next = next.Next<SyntaxBase>();
            }

            if (!isFind)
            {
                new SyntaxContent(owner, this);
            }
        }

        internal override void ParseHtml(HtmlBase owner)
        {
            var font = owner;
            bool isBegin = !(Owner is SyntaxFont);

            if (isBegin)
            {
                font = new HtmlFont(owner, this);
            }

            ParseHtmlMain(font, this);
        }
    }

    internal class SyntaxLink : SyntaxBase
    {
        /// <summary>
        /// Constructor registry, factory mode.
        /// </summary>
        public SyntaxLink(Registry registry)
            : base(registry)
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
        /// Constructor ParseTwo, ParseThree, ParseFour and ParseFive.
        /// </summary>
        public SyntaxLink(SyntaxBase owner, SyntaxBase syntax)
            : base(owner, syntax)
        {
            Data.Link = ((SyntaxLink)syntax).Link;
            Data.LinkText = ((SyntaxLink)syntax).LinkText;
        }

        public string Link => Data.Link;

        public string LinkText => Data.LinkText;

        protected internal override SyntaxBase Create(SyntaxBase owner, SyntaxBase syntax)
        {
            return new SyntaxLink(owner, syntax);
        }

        internal override void RegistrySchema(RegistrySchemaResult result)
        {
            result.IsBlock = true;
            result.AddOwner<SyntaxParagraph>();
            // result.AddOwner<SyntaxTitle>(); // No link in title
            result.AddOwner<SyntaxBullet>();
            result.AddOwner<SyntaxFont>(); // For example bold link
        }

        internal override void ParseOne(SyntaxBase owner, MdTokenBase token)
        {
            // For example http://	
            if (token is MdLink)
            {
                if (UtilParse.ParseOneIsLink(token, null, out var linkEnd, out string link))
                {
                    new SyntaxLink(owner, token, linkEnd, link, link);
                }
            }

            // For example []()	
            if (token is MdBracket bracketSquare && bracketSquare.TextBracket == "[")
            {
                var next = token.Next<MdTokenBase>();
                UtilParse.ParseOneIsLinkText(next, null, out var linkTextEnd, out string linkText);
                if (linkText != null)
                {
                    next = linkTextEnd.Next<MdTokenBase>();
                }
                if (next is MdBracket bracketSquareEnd && bracketSquareEnd.TextBracket == "]")
                {
                    next = next.Next<MdTokenBase>();
                    if (next is MdBracket bracketRound && bracketRound.TextBracket == "(")
                    {
                        next = next.Next<MdTokenBase>();
                        if (UtilParse.ParseOneIsLinkText(next, null, out var linkEnd, out string link))
                        {
                            next = linkEnd.Next<MdTokenBase>();
                            if (next is MdBracket bracketRoundEnd && bracketRoundEnd.TextBracket == ")")
                            {
                                if (linkText == null)
                                {
                                    linkText = link;
                                }
                                new SyntaxLink(owner, token, next, link, linkText);
                            }
                        }
                    }
                }
            }
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
        public SyntaxImage(Registry registry)
            : base(registry)
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
        /// Constructor ParseTwo, ParseThree, ParseFour and ParseFive.
        /// </summary>
        public SyntaxImage(SyntaxBase owner, SyntaxBase syntax)
            : base(owner, syntax)
        {
            Data.Link = ((SyntaxImage)syntax).Link;
            Data.LinkText = ((SyntaxImage)syntax).LinkText;
        }

        protected internal override SyntaxBase Create(SyntaxBase owner, SyntaxBase syntax)
        {
            return new SyntaxImage(owner, syntax);
        }

        internal override void RegistrySchema(RegistrySchemaResult result)
        {
            result.IsBlock = true;
            result.AddOwner<SyntaxPage>();
        }

        public string Link => Data.Link;

        public string LinkText => Data.LinkText;

        internal override void ParseOne(SyntaxBase owner, MdTokenBase token)
        {
            // For example ![]()
            if (token is MdImage)
            {
                MdTokenBase next = token.Next<MdTokenBase>();
                UtilParse.ParseOneIsLinkText(next, token, out var linkTextEnd, out string linkText);
                if (linkText != null)
                {
                    next = linkTextEnd.Next<MdTokenBase>();
                }
                if (next is MdBracket bracketSquareEnd && bracketSquareEnd.TextBracket == "]")
                {
                    next = next.Next<MdTokenBase>();
                    if (next is MdBracket bracketRound && bracketRound.TextBracket == "(")
                    {
                        next = next.Next<MdTokenBase>();
                        if (UtilParse.ParseOneIsLink(next, token, out var linkEnd, out var link))
                        {
                            if (linkEnd.Next() is MdBracket bracketRoundEnd && bracketRoundEnd.Text == ")")
                            {
                                new SyntaxImage(owner, token, bracketRoundEnd, link, linkText != null ? linkText : link);
                            }
                        }
                    }
                }
            }
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
        public SyntaxContent(Registry registry)
            : base(registry)
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
        /// Constructor ParseTwo, ParseThree, ParseFour and ParseFive.
        /// </summary>
        public SyntaxContent(SyntaxBase owner, SyntaxBase syntax)
            : base(owner, syntax)
        {

        }

        protected internal override SyntaxBase Create(SyntaxBase owner, SyntaxBase syntax)
        {
            return new SyntaxContent(owner, syntax);
        }

        internal override void RegistrySchema(RegistrySchemaResult result)
        {
            result.AddOwner<SyntaxParagraph>();
            result.AddOwner<SyntaxTitle>();
            result.AddOwner<SyntaxBullet>();
            result.AddOwner<SyntaxFont>();
            result.AddOwner<SyntaxCustomNote>(false);
        }

        internal override void ParseOne(SyntaxBase owner, MdTokenBase token)
        {
            new SyntaxContent(owner, token, token);
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
        public SyntaxParagraph(Registry registry)
            : base(registry)
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
        /// Constructor ParseTwo, ParseThree, ParseFour and ParseFive.
        /// </summary>
        public SyntaxParagraph(SyntaxBase owner, SyntaxBase syntax)
            : base(owner, syntax)
        {
            UtilDoc.Assert(owner is SyntaxPage || owner is SyntaxCustomNote || owner is SyntaxCustomPage);
        }

        protected internal override SyntaxBase Create(SyntaxBase owner, SyntaxBase syntax)
        {
            return new SyntaxParagraph(owner, syntax);
        }

        internal override void RegistrySchema(RegistrySchemaResult result)
        {
            result.AddOwner<SyntaxPage>();
            result.AddOwner<SyntaxCustomNote>();
        }

        internal override void ParseOne(SyntaxBase owner, MdTokenBase token)
        {
            if (token is MdParagraph)
            {
                new SyntaxParagraph(owner, token, token);
            }
        }

        internal override void ParseHtml(HtmlBase owner)
        {
            var paragraph = new HtmlParagraph(owner, this);

            ParseHtmlMain(paragraph, this);
        }
    }

    internal class SyntaxNewLine : SyntaxBase
    {
        /// <summary>
        /// Constructor registry, factory mode.
        /// </summary>
        public SyntaxNewLine(Registry registry)
            : base(registry)
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
        /// Constructor ParseTwo, ParseThree, ParseFour and ParseFive.
        /// </summary>
        public SyntaxNewLine(SyntaxBase owner, SyntaxBase syntax)
            : base(owner, syntax)
        {

        }

        protected internal override SyntaxBase Create(SyntaxBase owner, SyntaxBase syntax)
        {
            return new SyntaxNewLine(owner, syntax);
        }

        internal override void RegistrySchema(RegistrySchemaResult result)
        {
            result.AddOwner<SyntaxParagraph>();
            // result.AddOwner<SyntaxTitle>(); // New line in title is the end of title.
            result.AddOwner<SyntaxBullet>();
            result.AddOwner<SyntaxCustomNote>(false);
        }

        internal override void ParseOne(SyntaxBase owner, MdTokenBase token)
        {
            if (token is MdNewLine)
            {
                new SyntaxNewLine(owner, token);
            }
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
        public SyntaxIgnore(Registry registry)
            : base(registry)
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
        /// Constructor ParseTwo, ParseThree, ParseFour and ParseFive.
        /// </summary>
        public SyntaxIgnore(SyntaxBase owner, SyntaxBase syntax)
            : base(owner, syntax)
        {

        }

        protected internal override SyntaxBase Create(SyntaxBase owner, SyntaxBase syntax)
        {
            return new SyntaxIgnore(owner, syntax);
        }

        internal override void RegistrySchema(RegistrySchemaResult result)
        {
            result.AddOwner<SyntaxParagraph>();
            result.AddOwner<SyntaxTitle>();
            result.AddOwner<SyntaxBullet>();
            result.AddOwner<SyntaxFont>();
            result.AddOwner<SyntaxCustomNote>();
            result.AddOwner<SyntaxCode>();
            result.AddOwner<SyntaxPage>();
        }

        internal override void ParseHtml(HtmlBase owner)
        {
            // No html
        }
    }

    /// <summary>
    /// Base class for custom syntax. For example (Message Type="Warning")Do not delete!(Message)
    /// </summary>
    internal abstract class SyntaxCustomBase : SyntaxBase
    {
        /// <summary>
        /// Constructor registry, factory mode.
        /// </summary>
        public SyntaxCustomBase(Registry registry)
            : base(registry)
        {

        }

        /// <summary>
        /// Constructor ParseOne.
        /// </summary>
        public SyntaxCustomBase(SyntaxBase owner, MdTokenBase tokenBegin, MdTokenBase tokenEnd)
            : base(owner, tokenBegin, tokenEnd)
        {

        }

        /// <summary>
        /// Constructor ParseTwo, ParseThree, ParseFour and ParseFive.
        /// </summary>
        public SyntaxCustomBase(SyntaxBase owner, SyntaxBase syntax)
            : base(owner, syntax)
        {

        }

        /// <summary>
        /// Constructor ParseTwo, ParseThree, ParseFour and ParseFive.
        /// </summary>
        public SyntaxCustomBase(SyntaxBase owner, SyntaxBase syntaxBegin, SyntaxBase syntaxEnd)
            : base(owner, syntaxBegin, syntaxEnd)
        {

        }
    }

    internal class SyntaxCustomNote : SyntaxCustomBase
    {
        /// <summary>
        /// Constructor registry, factory mode.
        /// </summary>
        public SyntaxCustomNote(Registry registry)
            : base(registry)
        {

        }

        /// <summary>
        /// Constructor ParseOne.
        /// </summary>
        public SyntaxCustomNote(SyntaxBase owner, MdTokenBase tokenBegin, MdTokenBase tokenEnd)
            : base(owner, tokenBegin, tokenEnd)
        {

        }

        /// <summary>
        /// Constructor ParseTwo, ParseThree, ParseFour and ParseFive.
        /// </summary>
        public SyntaxCustomNote(SyntaxBase owner, SyntaxBase syntax)
            : base(owner, syntax)
        {

        }

        /// <summary>
        /// Constructor ParseTwo, ParseThree, ParseFour and ParseFive.
        /// </summary>
        public SyntaxCustomNote(SyntaxBase owner, SyntaxBase syntaxBegin, SyntaxBase syntaxEnd)
            : base(owner, syntaxBegin, syntaxEnd)
        {

        }

        protected internal override SyntaxBase Create(SyntaxBase owner, SyntaxBase syntax)
        {
            return new SyntaxCustomNote(owner, syntax);
        }

        internal override void RegistrySchema(RegistrySchemaResult result)
        {
            result.IsBlock = true;
            result.AddOwner<SyntaxPage>();
        }

        internal override void ParseOne(SyntaxBase owner, MdTokenBase token)
        {
            if (UtilParse.ParseOneIsCustom(token, "Note", out var tokenEnd, out _))
            {
                new SyntaxCustomNote(owner, token, tokenEnd);
                if (tokenEnd.Next() is MdNewLine newLine)
                {
                    new SyntaxIgnore(owner, newLine);
                }
            }
        }

        internal override void ParseTwo(SyntaxBase owner)
        {
            var registry = Data.Registry.SyntaxRegistry;

            var next = Next<SyntaxBase>();
            List<SyntaxBase> contentList = new List<SyntaxBase>();
            bool isFind = false; // Found closing (Note)
            bool isEndExceptional = false; // Ends without closing (Note). For example with title.
            while (next != null)
            {
                if (next is SyntaxCustomNote noteSource)
                {
                    var noteDest = new SyntaxCustomNote(owner, this, noteSource);
                    new SyntaxIgnore(noteDest, this);
                    ParseMain(noteDest, contentList);
                    if (!isEndExceptional)
                    {
                        new SyntaxIgnore(noteDest, noteSource);
                    }
                    else
                    {
                        noteSource.Data.IsEndExceptional = true;
                    }

                    isFind = true;
                    break;
                }

                if (!registry.SchemaTypeList[GetType()].Contains(next.GetType()))
                {
                    isEndExceptional = true;
                }
                if (!isEndExceptional)
                {
                    contentList.Add(next);
                }
                next = next.Next<SyntaxBase>();
            }

            if (!isFind)
            {
                if (!Data.IsEndExceptional)
                {
                    new SyntaxContent(owner, this);
                }
                else
                {
                    new SyntaxIgnore(owner, this);
                }
            }
        }

        internal override void ParseHtml(HtmlBase owner)
        {
            var note = new HtmlCustomNote(owner, this);

            ParseHtmlMain(note, this);
        }
    }

    /// <summary>
    /// For example (Youtube) https://www.youtube.com/embed/bYJTl5axgUY
    /// </summary>
    internal class SyntaxCustomYoutube : SyntaxCustomBase
    {
        /// <summary>
        /// Constructor registry, factory mode.
        /// </summary>
        public SyntaxCustomYoutube(Registry registry)
            : base(registry)
        {

        }

        /// <summary>
        /// Constructor ParseOne.
        /// </summary>
        public SyntaxCustomYoutube(SyntaxBase owner, MdTokenBase tokenBegin, MdTokenBase tokenEnd, string link)
            : base(owner, tokenBegin, tokenEnd)
        {
            Data.Link = link;
        }

        /// <summary>
        /// Constructor ParseTwo, ParseThree, ParseFour and ParseFive.
        /// </summary>
        public SyntaxCustomYoutube(SyntaxBase owner, SyntaxBase syntax)
            : base(owner, syntax)
        {
            Data.Link = ((SyntaxCustomYoutube)syntax).Link;
        }

        public string Link => Data.Link;

        protected internal override SyntaxBase Create(SyntaxBase owner, SyntaxBase syntax)
        {
            return new SyntaxCustomYoutube(owner, syntax);
        }

        internal override void RegistrySchema(RegistrySchemaResult result)
        {
            result.AddOwner<SyntaxPage>();
        }

        internal override void ParseOne(SyntaxBase owner, MdTokenBase token)
        {
            if (UtilParse.ParseOneIsCustom(token, "Youtube", out var tokenEnd, out var paramList))
            {
                if (paramList.TryGetValue("Link", out var paramLink))
                {
                    if (UtilParse.ParseOneIsLink(paramLink.TokenBegin, paramLink.TokenEnd, out var linkEnd, out string link))
                    {
                        new SyntaxCustomYoutube(owner, token, tokenEnd, link);
                    }
                }
            }
        }

        internal override void ParseHtml(HtmlBase owner)
        {
            var note = new HtmlCustomYoutube(owner, this);

            ParseHtmlMain(note, this);
        }
    }

    /// <summary>
    /// Custom syntax for page break.
    /// </summary>
    internal class SyntaxCustomPage : SyntaxCustomBase
    {
        /// <summary>
        /// Constructor registry, factory mode.
        /// </summary>
        public SyntaxCustomPage(Registry registry)
            : base(registry)
        {

        }

        /// <summary>
        /// Constructor ParseOne.
        /// </summary>
        public SyntaxCustomPage(SyntaxBase owner, MdTokenBase tokenBegin, MdTokenBase tokenEnd, string pagePath, string pageTitleHtml)
            : base(owner, tokenBegin, tokenEnd)
        {
            Data.PagePath = pagePath;
            Data.PageTitleHtml = pageTitleHtml;
        }

        /// <summary>
        /// Constructor ParseTwo, ParseThree, ParseFour and ParseFive.
        /// </summary>
        public SyntaxCustomPage(SyntaxBase owner, SyntaxBase syntax)
            : base(owner, syntax)
        {
            Data.PagePath = ((SyntaxCustomPage)syntax).PagePath;
            Data.PageTitleHtml = ((SyntaxCustomPage)syntax).PageTitleHtml;
        }

        /// <summary>
        /// Gets PagePath. Custom parameter of class SyntaxCustomPage.
        /// </summary>
        public string PagePath => Data.PagePath;

        /// <summary>
        /// Gets PageTitleHtml. Custom parameter of class SyntaxCustomPage.
        /// </summary>
        public string PageTitleHtml => Data.PageTitleHtml;

        protected internal override SyntaxBase Create(SyntaxBase owner, SyntaxBase syntax)
        {
            return new SyntaxCustomPage(owner, syntax);
        }

        internal override void RegistrySchema(RegistrySchemaResult result)
        {
            result.AddOwner<SyntaxPage>();
            result.IsBlock = true;
        }

        internal override void ParseOne(SyntaxBase owner, MdTokenBase token)
        {
            if (UtilParse.ParseOneIsCustom(token, "Page", out var tokenEnd, out var paramList))
            {
                paramList.TryGetValue("Path", out var pagePath);
                paramList.TryGetValue("Title", out var titleHtml);
                new SyntaxCustomPage(owner, token, tokenEnd, pagePath?.Text, titleHtml?.Text);
            }
        }

        internal override void ParseTwo(SyntaxBase owner)
        {
            var next = Next<SyntaxBase>();
            List<SyntaxBase> contentList = new List<SyntaxBase>();
            while (true)
            {
                if (next is SyntaxCustomPage || next == null)
                {
                    var pageDest = new SyntaxCustomPage(owner, this);
                    new SyntaxIgnore(pageDest, this);
                    ParseMain(pageDest, contentList);
                    break;
                }
                contentList.Add(next);
                next = next.Next<SyntaxBase>();
            }
        }

        internal override void ParseThree(SyntaxBase owner)
        {
            SyntaxPage ownerLocal;
            if (owner.Data.ListCount() == 0)
            {
                // (Page) is first text. Do not create, split new page but use existing.
                ownerLocal = (SyntaxPage)owner;
            }
            else
            {
                // Create, split new page,
                ownerLocal = new SyntaxPage((SyntaxBase)owner.Owner, this);
            }
            ownerLocal.Data.PagePath = PagePath;
            ownerLocal.Data.PageTitleHtml = PageTitleHtml;
            ParseMain(ownerLocal, this);
        }

        internal override void ParseHtml(HtmlBase owner)
        {
            throw new Exception(); // Should never come here! Create, split new page.
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
            UtilDoc.Assert(owner.Data.Registry.ParseEnum == ParseEnum.ParseHtml);

            Data.SyntaxId = Registry.ReferenceSet(syntax);
        }

        public SyntaxBase Syntax => Data.Registry.ReferenceGet<SyntaxBase>(Data.SyntaxId);

        internal string Render()
        {
            var result = new StringBuilder();
            Render(result);
            return UtilDoc.StringNull(result.ToString());
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
            if (Data.Registry.IsDebug)
            {
                result.Append("<section>(page)");
            }
        }

        internal override void RenderEnd(StringBuilder result)
        {
            if (Data.Registry.IsDebug)
            {
                result.Append("(/page)</section>");
            }
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
        public new SyntaxTitle Syntax => (SyntaxTitle)base.Syntax;

        internal override void RenderBegin(StringBuilder result)
        {
            // For example <h1>
            result.Append("<h" + Syntax.TitleLevel + ">");
            if (Syntax.TitleId != null)
            {
                var anchor = string.Format("<a id=\"{0}\" class=\"anchor\" aria-hidden=\"true\" href=\"#{0}\"></a>", Syntax.TitleId);
                result.Append(anchor);
            }
        }

        internal override void RenderEnd(StringBuilder result)
        {
            // For example <h1>
            result.Append("</h" + Syntax.TitleLevel + ">");
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
            if (!Data.Registry.IsDebug && List.Count == 0)
            {
                // Do not render empty paragraph
                return;
            }

            result.Append("<p>");
            if (Data.Registry.IsDebug)
            {
                result.Append("(p)");
            }
        }

        internal override void RenderEnd(StringBuilder result)
        {
            if (!Data.Registry.IsDebug && List.Count == 0)
            {
                // Do not render empty paragraph
                return;
            }

            if (Data.Registry.IsDebug)
            {
                result.Append("(/p)");
            }
            result.Append("</p>");
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
                var fileNameExtension = UtilDoc.StringNull(Path.GetExtension(Syntax.Link));
                // Render html image tag only if src file name has an extension.
                // For example the image file name "/" would cause the session to navigate.
                if (fileNameExtension != null)
                {
                    result.Append($"<img src=\"{ Syntax.Link }\" alt=\"{ Syntax.LinkText }\" />");
                }
            }
        }
    }

    internal class HtmlCode : HtmlBase
    {
        /// <summary>
        /// Constructor parse html.
        /// </summary>
        public HtmlCode(HtmlBase owner, SyntaxCode syntax)
            : base(owner, syntax)
        {

        }

        public new SyntaxCode Syntax => (SyntaxCode)base.Syntax;

        internal override void RenderContent(StringBuilder result)
        {
            result.Append(string.Format("<pre><code class=\"{0}\">", "language-" + Syntax.CodeLanguage));
            var codeText = Syntax.CodeText;
            if (Data.Registry.IsDebug == false)
            {
                if (codeText.StartsWith("\r"))
                {
                    codeText = codeText.Substring(1);
                }
                if (codeText.StartsWith("\n"))
                {
                    codeText = codeText.Substring(1);
                }
                if (codeText.EndsWith("\n"))
                {
                    codeText = codeText.Substring(0, codeText.Length - 1);
                }
                if (codeText.EndsWith("\r"))
                {
                    codeText = codeText.Substring(0, codeText.Length - 1);
                }
            }
            // Escape html special chars.
            codeText = System.Security.SecurityElement.Escape(codeText);
            result.Append(codeText);
            result.Append("</code></pre>");
        }
    }

    internal class HtmlCustomNote : HtmlBase
    {
        /// <summary>
        /// Constructor parse html.
        /// </summary>
        public HtmlCustomNote(HtmlBase owner, SyntaxBase syntax)
            : base(owner, syntax)
        {

        }

        internal override void RenderBegin(StringBuilder result)
        {
            result.Append("<article class=\"message is-info\"><div class=\"message-body\">");
        }

        internal override void RenderEnd(StringBuilder result)
        {
            result.Append("</div></article>");
        }
    }

    internal class HtmlCustomYoutube : HtmlBase
    {
        /// <summary>
        /// Constructor parse html.
        /// </summary>
        public HtmlCustomYoutube(HtmlBase owner, SyntaxBase syntax) 
            : base(owner, syntax)
        {

        }

        internal override void RenderContent(StringBuilder result)
        {
            string link = ((SyntaxCustomYoutube)Syntax).Link;
            string html = $"<iframe src=\"{ link }\"></iframe>";
            result.Append(html);
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

    internal static class UtilParse
    {
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

        /// <summary>
        /// Create syntax component of type createType and add it to owner.
        /// </summary>
        /// <param name="owner">Tree to add new syntax component.</param>
        /// <param name="syntax">Syntax component to reference (copy).</param>
        /// <returns>Returns new syntax component.</returns>
        internal static SyntaxBase Create(SyntaxRegistry registry, SyntaxBase owner, SyntaxBase syntax, Type createType)
        {
            return registry.TypeList[createType].Create(owner, syntax);
        }

        /// <summary>
        /// Create new syntax component of type syntax and add it to owner.
        /// </summary>
        internal static SyntaxBase Create(SyntaxRegistry registry, SyntaxBase owner, SyntaxBase syntax)
        {
            return Create(registry, owner, syntax, syntax.GetType());
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
                result = result.Next(syntax.TokenEnd);
            }
            return result;
        }

        /// <summary>
        /// Returns true, if line starts with token T. Allows leading spaces.
        /// </summary>
        internal static bool ParseOneIsNewLine<T>(MdTokenBase token, out T tokenEnd) where T : MdTokenBase
        {
            var result = false;
            tokenEnd = null;

            bool isStart;
            Component component = token;

            // Leading start or NewLine or Paragraph
            var previous = token.Previous<MdTokenBase>();
            isStart = previous == null || previous is MdNewLine || previous is MdParagraph;
            // Leading Space
            if (component is MdSpace)
            {
                component = component.Next<MdTokenBase>(null);
            }
            // Token
            if (component is T)
            {
                tokenEnd = (T)component;
            }

            if (isStart && tokenEnd != null)
            {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Returns true, if tokenBegin is valid Link. For example https://workplacex.org
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
                if (!(next is MdContent || next is MdLink || next is MdSymbol))
                {
                    break;
                }
                linkEnd = next;
                link += next.Text;
                result = true;
            } while (Component.Next(ref next, tokenEnd));
            return result;
        }

        /// <summary>
        /// Returns true, if tokenBegin contains valid LinkText. For example link description.
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
            } while (Component.Next(ref next, tokenEnd));
            return result;
        }

        public class ParseOneIsCustomItem
        {
            public MdTokenBase TokenBegin;

            public MdTokenBase TokenEnd;

            public string Text;
        }

        private static bool ParseOneIsCustom(MdTokenBase token, out MdTokenBase tokenEnd, out Dictionary<string, ParseOneIsCustomItem> paramList)
        {
            var result = false;
            tokenEnd = token;
            paramList = null;
            var next = token;
            while (next is MdContent)
            {
                var paramName = next.Text;
                next = next.Next<MdTokenBase>();
                if (next is MdSymbol symbol && symbol.SymbolEnum == MdSymbolEnum.Equal)
                {
                    next = next.Next<MdTokenBase>();
                    if (next is MdQuotation quotation && quotation.QuotationEnum == MdQuotationEnum.Double)
                    {
                        next = next.Next<MdTokenBase>();
                        var paramTokenBegin = next;
                        var paramTokenEnd = next;
                        StringBuilder value = new StringBuilder();
                        while (next != null && !(next is MdQuotation quotationEnd && quotationEnd.QuotationEnum == MdQuotationEnum.Double))
                        {
                            value.Append(next.Text);
                            paramTokenEnd = next;
                            next = next.Next<MdTokenBase>();
                        }
                        if (next is MdQuotation)
                        {
                            if (paramList == null)
                            {
                                paramList = new Dictionary<string, ParseOneIsCustomItem>();
                            }
                            paramList[paramName] = new ParseOneIsCustomItem { Text = UtilDoc.StringNull(value.ToString() ), TokenBegin = paramTokenBegin, TokenEnd = paramTokenEnd };
                            tokenEnd = next;
                            result = true;
                        }
                    }
                    next = next?.Next<MdTokenBase>();
                    if (next is MdSpace)
                    {
                        next = next.Next<MdTokenBase>();
                    }
                }
            }
            return result;
        }

        internal static bool ParseOneIsCustom(MdTokenBase token, string commandName, out MdTokenBase tokenEnd, out Dictionary<string, ParseOneIsCustomItem> paramList)
        {
            var result = false;
            tokenEnd = null;
            paramList = null;
            if (ParseOneIsNewLine<MdBracket>(token, out var bracket))
            {
                if (bracket.TextBracket == "(")
                {
                    if (bracket.Next() is MdContent content)
                    {
                        if (content.Text == commandName)
                        {
                            MdTokenBase next = content.Next<MdTokenBase>();
                            bool isParamValid = true;
                            if (next is MdSpace)
                            {
                                next = next.Next<MdTokenBase>();
                                isParamValid = ParseOneIsCustom(next, out var paramEnd, out paramList);
                                next = paramEnd.Next<MdTokenBase>();
                            }
                            if (isParamValid && next is MdBracket bracketEnd)
                            {
                                if (bracketEnd.TextBracket == ")")
                                {
                                    tokenEnd = bracketEnd;
                                    if (paramList == null)
                                    {
                                        paramList = new Dictionary<string, ParseOneIsCustomItem>();
                                    }
                                    result = true;
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }
    }

    internal static class UtilDoc
    {
        public static void Debug()
        {
            var textMd = "(Note A=\"";

            // Doc
            var appDoc = new AppDoc();
            appDoc.Data.Registry.IsDebug = true;
            new MdPage(appDoc.MdDoc, textMd);
            string exceptionText = null;
            try
            {
                appDoc.Parse();
            }
            catch (Exception exception)
            {
                exceptionText = exception.Message;
            }

            // Serialize, deserialize
            appDoc.Serialize(out string json);
            Component.Deserialize<AppDoc>(json);

            // Write file Debug.txt
            TextDebugWriteToFile(appDoc, exceptionText);
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

            string syntaxIdText = null;
            if (component is SyntaxBase syntaxId && syntaxId.Data.SyntaxId != null)
            {
                syntaxIdText = "->" + string.Format("{0:000}", syntaxId.Data.SyntaxId);
            }

            result.Append("-(" + component.GetType().Name + " " + string.Format("{0:000}", component.Data.Id) + syntaxIdText + ")");

            // Token
            if (component is MdTokenBase token)
            {
                result.Append(" Text=\"" + TextDebug(token.Text) + "\";");
            }
            // Syntax
            if (component is SyntaxBase syntax)
            {
                if (syntax is SyntaxDoc doc)
                {
                    if (doc.OwnerFind<AppDoc>().SyntaxDocOne.Data == doc.Data)
                    {
                        result.Append(" ParseOne (SyntaxToken)");
                    }
                    if (doc.OwnerFind<AppDoc>().SyntaxDocTwo.Data == doc.Data)
                    {
                        result.Append(" ParseTwo (Block)");
                    }
                    if (doc.OwnerFind<AppDoc>().SyntaxDocThree.Data == doc.Data)
                    {
                        result.Append(" ParseThree (Fold)");
                    }
                    if (doc.OwnerFind<AppDoc>().SyntaxDocFour.Data == doc.Data)
                    {
                        result.Append(" ParseFour (Owner Insert)");
                    }
                    if (doc.OwnerFind<AppDoc>().SyntaxDocFive.Data == doc.Data)
                    {
                        result.Append(" ParseFive (Owner Merge)");
                    }
                }
                else
                {
                    result.Append(" Text=\"" + TextDebug(syntax.Text) + "\";");
                    if (syntax.IsCreateNew)
                    {
                        result.Append(" IsCreateNew;");
                    }
                }
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
            return StringNull(result.ToString());
        }

        public static void TextDebugWriteToFile(AppDoc appDoc, string exceptionText = null)
        {
            string textMd = ((MdPage)appDoc.MdDoc.List.First()).Text;
            string result = TextDebug(appDoc);
            string textHtml = appDoc.HtmlDoc.Render();

            if (exceptionText != null)
            {
                result += "\r\n\r\n" + exceptionText;
            }

            result += "\r\n\r\n" + "Md:\r\n";
            result += textMd;

            result += "\r\n\r\n" + "Html:\r\n";
            result += textHtml;

            string textMdEscape = textMd.Replace("\r", "\\r").Replace("\n", "\\n");
            textMdEscape = textMdEscape.Replace("\"", "\\\"");
            string textHtmlEscape = textHtml?.Replace("\"", @"\""");
            textHtmlEscape = textHtmlEscape?.Replace("\r", "\\r").Replace("\n", "\\n");
            string textCSharp = "list.Add(new Item { TextMd = \"" + textMdEscape + "\", TextHtml = \"" + textHtmlEscape + "\" });";

            result += "\r\n\r\n" + "CSharp:\r\n";
            result += textCSharp;

            File.WriteAllText(@"C:\Temp\Debug.txt", result);
            // File.WriteAllText(@"C:\Temp\Debug.html", textHtml);
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

        public static string StringNull(string text)
        {
            return text == "" ? null : text;
        }
    }
}
