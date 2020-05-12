namespace Framework.Util
{
    using System.Collections.Generic;
    using System.IO;

    public class TextParseComponent
    {
        public TextParseComponent(TextParseComponent owner)
        {
            Owner = owner;
            if (Owner != null)
            {
                Owner.list.Add(this);
            }
        }

        public readonly TextParseComponent Owner;

        public TextParseDocument Document
        {
            get
            {
                var result = this;
                while (result.Owner != null)
                {
                    result = result.Owner;
                }
                return (TextParseDocument)result;
            }
        }

        private List<TextParseComponent> list = new List<TextParseComponent>();

        public bool IsLine
        {
            get
            {
                return Document.LineList.Count > Document.LineIndex;
            }
        }

        public void LineNext()
        {
            Document.LineIndex += 1;
            Document.LineCharIndex = 0;
        }

        public string Line
        {
            get
            {
                return Document.LineList[Document.LineIndex];
            }
        }

        public bool IsLineChar
        {
            get
            {
                return IsLine && Line.Length > Document.LineCharIndex;
            }
        }

        public char LineChar
        {
            get
            {
                return Document.LineList[Document.LineIndex][Document.LineCharIndex];
            }
        }

        public bool LineCharGet(int lineOffsetIndex, out char lineChar)
        {
            var result = false;
            lineChar = (char)0;
            int lineIndex = Document.LineCharIndex + lineOffsetIndex;
            if (lineIndex >= 0 && lineIndex < Line.Length)
            {
                lineChar = Line[lineIndex];
            }
            return result;
        }

        public void LineCharNext()
        {
            if (Document.LineCharIndex < Line.Length)
            {
                Document.LineCharIndex += 1;
            }
        }

        protected virtual bool Parse()
        {
            return false;
        }

        public void ParseExecute()
        {
            foreach (var item in list)
            {
                var lineIndexLocal = Document.LineIndex;
                var lineCharIndexLocal = Document.LineCharIndex;
                if (item.Parse() == false)
                {
                    Document.LineIndex = lineIndexLocal;
                    Document.LineCharIndex = lineCharIndexLocal;
                }
            }
        }
    }

    public class TextParseDocument : TextParseComponent
    {
        public TextParseDocument(string text)
            : base(null)
        {
            this.Text = text;
            StringReader reader = new StringReader(text);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrEmpty(line))
                {
                    line = null;
                }
                LineList.Add(line);
            }
        }

        public readonly string Text;

        internal List<string> LineList;

        internal int LineIndex;

        internal int LineCharIndex;
    }

    public class TextParseError
    {
        public int LineIndex;

        public int ColIndex;

        public string ErrorText;
    }
}
