namespace Framework.Json
{
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Bootstrap dialog window.
    /// </summary>
    public class BootstrapModal : Page
    {
        public BootstrapModal() : this(null) { }

        public BootstrapModal(ComponentJson owner) : base(owner) { }

        protected internal override Task InitAsync()
        {
            Init(true, false);
            return base.InitAsync();
        }

        protected void Init(bool isHeader, bool isFooter, bool isLarge = false)
        {
            if (isHeader)
            {
                DivModalContent().ComponentCreate<Div>("Header").CssClass = "modal-header";
                ButtonClose();
            }
            DivModalContent().ComponentCreate<Div>("Body").CssClass = "modal-body";
            if (isFooter)
            {
                DivModalContent().ComponentCreate<Div>("Footer").CssClass = "modal-footer";
            }
            if (isLarge)
            {
                DivModalDialog().CssClass += " modal-lg";
            }
        }

        private Div DivModal()
        {
            return this.ComponentGetOrCreate<Div>((div) =>
            {
                div.CssClass = "modal";
            });
        }

        private Div DivModalDialog()
        {
            return DivModal().ComponentGetOrCreate<Div>((div) =>
            {
                div.CssClass = "modal-dialog";
            });
        }

        private Div DivModalContent()
        {
            return DivModalDialog().ComponentGetOrCreate<Div>((div) =>
            {
                div.CssClass = "modal-content";
            });
        }

        /// <summary>
        /// Returns header. Place for example title into this div.
        /// </summary>
        public Div DivHeader()
        {
            return DivModalContent().ComponentGet<Div>("Header");
        }

        /// <summary>
        /// Returns body. Place content into this div.
        /// </summary>
        public Div DivBody()
        {
            return DivModalContent().ComponentGet<Div>("Body");
        }

        /// <summary>
        /// Returns footer. Place for example close button into this div.
        /// </summary>
        public Div DivFooter()
        {
            return DivModalContent().ComponentGet<Div>("Footer");
        }

        /// <summary>
        /// Returns close button, if header exists. See also method Init();
        /// </summary>
        public Button ButtonClose()
        {
            Button result = null;
            var header = DivHeader();
            if (header != null)
            {
                result = header.ComponentGetOrCreate<Button>("Close", (button) =>
                {
                    button.CssClass = "close";
                    button.TextHtml = "<span>&times;</span>";
                });
            }
            return result;
        }

        /// <summary>
        /// Add shadow covering application behind modal window.
        /// </summary>
        internal static void DivModalBackdropCreate(AppJson appJson)
        {
            Div result = appJson.ComponentCreate<Div>();
            result.CssClass = "modal-backdrop show";
        }

        /// <summary>
        /// Remove shadow covering application behind modal window.
        /// </summary>
        internal static void DivModalBackdropRemove(AppJson owner)
        {
            foreach (var item in owner.List.Where(item => item.CssClass == "modal-backdrop show").ToList())
            {
                item.ComponentRemove();
            }
        }

        protected internal override Task ButtonClickAsync(Button button)
        {
            if (button == ButtonClose())
            {
                this.ComponentRemove();
            }
            return base.ButtonClickAsync(button);
        }
    }
}
