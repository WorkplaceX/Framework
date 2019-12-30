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
            this.DivModal = new Div(this) { CssClass = "modal" };
            this.DivModalDialog = new Div(DivModal) { CssClass = "modal-dialog" };
            this.DivModalContent = new Div(DivModalDialog) { CssClass = "modal-content" };
            if (isHeader)
            {
                this.DivHeader = new Div(DivModalContent) { CssClass = "modal-header" };
                ButtonClose = new Button(DivHeader) { CssClass = "close", TextHtml = "<span>&times;</span>" };
            }
            this.DivBody = new Div(DivModalContent) { CssClass = "modal-body" };
            if (isFooter)
            {
                this.DivFooter = new Div(DivModalContent) { CssClass = "modal-footer" };
            }
            if (isLarge)
            {
                DivModalDialog.CssClass += " modal-lg";
            }
        }

        internal Div DivModal;

        internal Div DivModalDialog;

        internal Div DivModalContent;

        /// <summary>
        /// Gets DivHeader. Place for example title into this div.
        /// </summary>
        public Div DivHeader { get; internal set; }

        /// <summary>
        /// Gets DivBody. Place content into this div.
        /// </summary>
        public Div DivBody { get; internal set; }

        /// <summary>
        /// Gets DivFooter. Place for example close button into this div.
        /// </summary>
        public Div DivFooter { get; internal set; }

        /// <summary>
        /// Gets or sets ButtonClose. If header exists. See also method Init();
        /// </summary>
        internal Button ButtonClose;

        /// <summary>
        /// Add shadow covering application behind modal window.
        /// </summary>
        internal static void DivModalBackdropCreate(AppJson appJson)
        {
            Div result = new Div(appJson);
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

        protected internal override Task ProcessAsync()
        {
            if (ButtonClose.IsClick)
            {
                this.ComponentRemove();
            }
            return base.ProcessAsync();
        }
    }
}
