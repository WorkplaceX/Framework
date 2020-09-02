namespace Framework.Json
{
    using Database.dbo;
    using Framework.App;
    using Framework.DataAccessLayer;
    using Framework.Server;
    using Framework.Session;
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Threading.Tasks;

    internal enum RequestCommand
    {
        None = 0,

        ButtonIsClick = 1,

        BootstrapNavbarButtonIsClick = 7,

        GridIsClickSort = 8,

        GridCellIsModify = 9,

        GridIsClickEnum = 10,

        GridIsClickRow = 11,

        GridIsClickConfig = 12,

        /// <summary>
        /// Inform server about text leave event.
        /// </summary>
        GridIsTextLeave = 13,

        /// <summary>
        /// Send css grid style property grid-template-columns to server after column resize.
        /// </summary>
        GridStyleColumn = 14,

        /// <summary>
        /// User clicked home button for example on navbar.
        /// </summary>
        HomeIsClick = 15,

        /// <summary>
        /// User clicked an internal link. For example: "/contact/". Instead of GET and download Angular again a POST command is sent to the server.
        /// </summary>
        NavigateLink = 16,

        /// <summary>
        /// User clicked backward, forward navigation history.
        /// </summary>
        NavigateLinkBackwardForward = 17,
    }

    /// <summary>
    /// Origin for request and command.
    /// </summary>
    internal enum RequestOrigin
    {
        None = 0,

        /// <summary>
        /// Request or command created by server.
        /// </summary>
        Server = 1,

        /// <summary>
        /// Request or command sent by browser.
        /// </summary>
        Browser = 2,
    }

    /// <summary>
    /// Command sent by browser. See also RequestJson.
    /// </summary>
    internal sealed class CommandJson
    {
        public RequestCommand Command { get; set; }

        /// <summary>
        /// Gets or sets Origin. Command sent by browser or created by server.
        /// </summary>
        public RequestOrigin Origin { get; set; }

        public int GridCellId { get; set; }

        public string GridCellText { get; set; }

        /// <summary>
        /// Gets or sets GridCellTextBase64. Contains from user uploaded file data.
        /// </summary>
        public string GridCellTextBase64 { get; set; }

        /// <summary>
        /// Gets or sets GridCellTextBase64. Contains file name from user uploaded file.
        /// </summary>
        public string GridCellTextBase64FileName { get; set; }

        /// <summary>
        /// Send visible column width list to server.
        /// </summary>
        public string[] GridStyleColumnList { get; set; }

        /// <summary>
        /// Gets or sets Id. This is ComponentJson.Id.
        /// </summary>
        public int ComponentId { get; set; }

        public GridIsClickEnum GridIsClickEnum { get; set; }

        /// <summary>
        /// Gets GridCellTextIsInternal. If true, text has been set internally by grid lookup select row.
        /// </summary>
        public bool GridCellTextIsLookup; // TODO Command Queue

        public int BootstrapNavbarButtonId { get; set; }

        /// <summary>
        /// Gets or sets NavigateLinkPath. For internal link. For example: "/contact/".
        /// </summary>
        public string NavigateLinkPath { get; set; }

        /// <summary>
        /// Gets or sets NavigateLinkPathIsAddHistory. If true, NavigateLinkPath is added to browser history. Used by server command. Not used by client command.
        /// </summary>
        public bool NavigateLinkPathIsAddHistory { get; set; }
    }

    /// <summary>
    /// Request sent by Angular client.
    /// </summary>
    internal sealed class RequestJson
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public RequestJson() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public RequestJson(CommandJson command)
        {
            this.Origin = RequestOrigin.Server;
            this.CommandList = new List<CommandJson>();
            if (command != null)
            {
                this.CommandList.Add(command);
            }
        }

        public int RequestCount { get; set; }

        public int ResponseCount { get; set; }

        /// <summary>
        /// Gets or sets Origin. Request sent by browser or created by server.
        /// </summary>
        public RequestOrigin Origin { get; set; }

        /// <summary>
        /// Gets or sets BrowserUrl. Url shown in browser. Available only for browser POST request. See also method BrowserPath();
        /// </summary>
        public string BrowserUrl { get; set; }

        /// <summary>
        /// Returns for example "/contact/".
        /// </summary>
        public string BrowserPath
        {
            get
            {
                string result;
                if (Origin == RequestOrigin.Server)
                {
                    UtilFramework.Assert(UtilServer.Context.Request.Method == "GET");
                    result = UtilServer.Context.Request.Path; // Browser refresh
                }
                else
                {
                    UtilFramework.Assert(Origin == RequestOrigin.Browser);
                    UtilFramework.Assert(UtilServer.Context.Request.Method == "POST");
                    UtilFramework.Assert(BrowserUrl != null);
                    result = new Uri(BrowserUrl).AbsolutePath; // Browser back
                }
                return result;
            }
        }

        /// <summary>
        /// Gets or sets CommandList. Command queue sent by one browser request. Commands can also be added during process.
        /// </summary>
        public List<CommandJson> CommandList { get; set; }

        /// <summary>
        /// Gets or sets CommandListIndex. This is the current command to process.
        /// </summary>
        [Serialize(SerializeEnum.None)]
        public int CommandIndex { get; set; }

        /// <summary>
        /// Returns current command to process.
        /// </summary>
        public CommandJson CommandGet()
        {
            CommandJson result = null;
            if (CommandList.Count > CommandIndex)
            {
                result = CommandList[CommandIndex];
            }
            return result;
        }

        [Serialize(SerializeEnum.None)]
        public int CommandAddCount;

        /// <summary>
        /// Add command on top to queue.
        /// </summary>
        public void CommandAdd(CommandJson command)
        {
            CommandAddCount += 1;
            if (CommandAddCount == 8)
            {
                throw new Exception("CommandAdd overflow!");
            }
            CommandList.Add(command);
        }

        /// <summary>
        /// Move to next command in queue.
        /// </summary>
        public void CommandNext()
        {
            if (CommandList.Count > CommandIndex)
            {
                CommandIndex += 1;
            }
        }
    }

    /// <summary>
    /// Application component tree. Tree is serialized and deserialized for every client request. Stores session state from public and internal fields and properties.
    /// </summary>
    public abstract class ComponentJson
    {
        /// <summary>
        /// Constructor to programmatically create new object. Constructor is not called on client request session deserialization (GetUninitializedObject).
        /// </summary>
        internal ComponentJson(ComponentJson owner, string type)
        {
            this.Type = type;
            Constructor(owner, isDeserialize: false);
        }

        internal void Constructor(ComponentJson owner, bool isDeserialize)
        {
            this.Owner = owner;
            if (Owner == null)
            {
                this.Root = this;
                this.RootComponentJsonList = new Dictionary<int, ComponentJson>(); // Init list.
                this.RootReferenceList = new List<(object obj, UtilJson.DeclarationProperty property, int id)>();
            }
            else
            {
                this.Root = owner.Root;
            }
            if (!isDeserialize)
            {
                Root.RootIdCount += 1;
                this.Id = Root.RootIdCount;
                Root.RootComponentJsonList.Add(Id, this); // Id is not yet available if deserialize.
            }

            if (isDeserialize == false)
            {
                if (owner != null)
                {
                    if (owner.List == null)
                    {
                        owner.List = new List<ComponentJson>();
                    }
                    int count = 0;
                    foreach (var item in owner.List)
                    {
                        if (item.TrackBy.StartsWith(this.Type + "-"))
                        {
                            count += 1;
                        }
                    }
                    this.TrackBy = this.Type + "-" + count.ToString();
                    owner.ListInternal.Add(this);
                }
            }
        }

        /// <summary>
        /// Gets Owner. This is the parent of this component.
        /// </summary>
        [Serialize(SerializeEnum.None)]
        public ComponentJson Owner { get; internal set; }

        [Serialize(SerializeEnum.None)]
        internal bool IsRemoved;

        [Serialize(SerializeEnum.None)]
        internal ComponentJson Root;

        internal int RootIdCount;

        /// <summary>
        /// (Id, ComponentJson).
        /// </summary>
        [Serialize(SerializeEnum.None)]
        internal Dictionary<int, ComponentJson> RootComponentJsonList;

        /// <summary>
        /// (Object, Property, ReferenceId). Used for deserialization.
        /// </summary>
        [Serialize(SerializeEnum.None)]
        internal List<(object obj, UtilJson.DeclarationProperty property, int id)> RootReferenceList;

        /// <summary>
        /// Solve ComponentJson references after deserialization.
        /// </summary>
        internal void RootReferenceSolve()
        {
            UtilFramework.Assert(Owner == null);
            UtilFramework.Assert(Root == this);
            foreach (var item in Root.RootReferenceList)
            {
                UtilFramework.Assert(item.property.IsList == false, "Reference to ComponentJson in List not supported!");
                ComponentJson componentJson = Root.RootComponentJsonList[item.id]; // Exception: Given key was not present in dictionary. Do not use method ComponentJson.ListInternal.Remove(); use method ComponentJsonExtension.ComponentRemove();
                item.property.ValueSet(item.obj, componentJson);
            }
        }

        /// <summary>
        /// Gets Id. Client sends command to server. See also <see cref="RequestJson.ComponentId"/>
        /// </summary>
        internal int Id { get; set; }

        /// <summary>
        /// Gets or sets Type. Used by Angular. Type to be rendered for derived classes. See also <see cref="Page"/>.
        /// </summary>
        internal string Type;

        internal string TrackBy { get; set; }

        /// <summary>
        /// Gets or sets custom html style classes for this component.
        /// </summary>
        public string CssClass;

        [Serialize(SerializeEnum.None)]
        internal List<ComponentJson> ListInternal = new List<ComponentJson>(); // Empty list is removed by json serializer.

        /// <summary>
        /// Gets List. List of child components.
        /// </summary>
        public IReadOnlyList<ComponentJson> List
        {
            get
            {
                return ListInternal;
            }
            internal set
            {
                ListInternal = (List<ComponentJson>)value;
            }
        }

        /// <summary>
        /// Gets or sets IsHide. If true component is not sent to client.
        /// </summary>
        public bool IsHide;
    }

    /// <summary>
    /// Extension methods to manage json component tree.
    /// </summary>
    public static class ComponentJsonExtension
    {
        /// <summary>
        /// Returns owner of type T. Searches in parent and grand parents.
        /// </summary>
        public static T ComponentOwner<T>(this ComponentJson component) where T : ComponentJson
        {
            do
            {
                component = component.Owner;
                if (component is T)
                {
                    return (T)component;
                }
            } while (component != null);
            return null;
        }

        private static void ComponentListAll(ComponentJson component, List<ComponentJson> result)
        {
            result.AddRange(component.List);
            foreach (var item in component.List)
            {
                ComponentListAll(item, result);
            }
        }

        /// <summary>
        /// Returns list of all child components recursively including this.
        /// </summary>
        public static List<ComponentJson> ComponentListAll(this ComponentJson component)
        {
            List<ComponentJson> result = new List<ComponentJson>
            {
                component
            };
            ComponentListAll(component, result);
            return result;
        }

        /// <summary>
        /// Returns all child components of type T.
        /// </summary>
        public static List<T> ComponentList<T>(this ComponentJson component) where T : ComponentJson
        {
            var result = new List<T>();
            foreach (var item in component.List)
            {
                if (UtilFramework.IsSubclassOf(item.GetType(), typeof(T)))
                {
                    result.Add((T)item);
                }
            }
            return result;
        }

        public enum PageShowEnum
        {
            None = 0,

            /// <summary>
            /// Add page to sibling pages.
            /// </summary>
            Default = 1,

            /// <summary>
            /// Remove sibling pages.
            /// </summary>
            SiblingRemove = 1,

            /// <summary>
            /// Hide sibling pages and keep their state.
            /// </summary>
            SiblingHide = 2,
        }

        /// <summary>
        /// Shows page or creates new one if it does not yet exist. Invokes also page init async.
        /// </summary>
        public static async Task<T> ComponentPageShowAsync<T>(this ComponentJson owner, T page, PageShowEnum pageShowEnum = PageShowEnum.Default, Action<T> init = null) where T : Page
        {
            T result = page;
            if (page != null && page.IsRemoved == false)
            {
                UtilFramework.Assert(page.Owner == owner, "Wrong Page.Owner!");
            }
            if (pageShowEnum == PageShowEnum.SiblingHide)
            {
                foreach (Page item in owner.List.OfType<Page>())
                {
                    item.IsHide = true; // Hide
                }
            }
            if (page == null || page.IsRemoved)
            {
                result = (T)Activator.CreateInstance(typeof(T), owner);
                init?.Invoke(result);
                await result.InitAsync();
            }
            result.IsHide = false; // Show
            if (pageShowEnum == PageShowEnum.SiblingRemove)
            {
                owner.List.OfType<Page>().ToList().ForEach(page =>
                {
                    if (page != result) { page.ComponentRemove(); }
                });
            }
            return result;
        }

        /// <summary>
        /// Creates new page. Invokes also page init async.
        /// </summary>
        public static Task<T> ComponentPageShowAsync<T>(this ComponentJson owner, PageShowEnum pageShowEnum = PageShowEnum.None, Action<T> init = null) where T : Page
        {
            return ComponentPageShowAsync<T>(owner, null, pageShowEnum, init);
        }

        /// <summary>
        /// Remove this component.
        /// </summary>
        public static void ComponentRemove(this ComponentJson component)
        {
            if (component != null)
            {
                component.Owner?.ListInternal.Remove(component);
                component.Owner = null;
                component.IsRemoved = true;
            }
        }

        /// <summary>
        /// Returns index of this component in parents list.
        /// </summary>
        public static int ComponentIndex(this ComponentJson component)
        {
            return component.Owner.ListInternal.IndexOf(component);
        }

        /// <summary>
        /// Returns count of this component parents list.
        /// </summary>
        public static int ComponentCount(this ComponentJson component)
        {
            return component.Owner.List.Count();
        }

        /// <summary>
        /// Move this component to index position.
        /// </summary>
        public static void ComponentMove(this ComponentJson component, int index)
        {
            var list = component?.Owner.ListInternal;
            list.Remove(component);
            list.Insert(index, component);
        }

        /// <summary>
        /// Move this component to last index.
        /// </summary>
        public static void ComponentMoveLast(this ComponentJson component)
        {
            component.ComponentMove(component.ComponentCount() - 1);
        }

        /// <summary>
        /// Remove all children.
        /// </summary>
        public static void ComponentListClear(this ComponentJson component)
        {
            foreach (var item in component.ListInternal)
            {
                item.Owner = null;
                item.IsRemoved = true;
            }
            component.ListInternal.Clear();
        }

        /// <summary>
        /// Add css class to ComponentJson.
        /// </summary>
        public static void CssClassAdd(this ComponentJson component, string value)
        {
            string cssClass = component.CssClass;
            string cssClassWholeWord = " " + cssClass + " ";
            if (!cssClassWholeWord.Contains(" " + value + " "))
            {
                if (UtilFramework.StringNull(cssClass) == null)
                {
                    component.CssClass = value;
                }
                else
                {
                    component.CssClass += " " + value;

                }
            }
        }

        /// <summary>
        /// Remove css class from ComponentJson.
        /// </summary>
        public static void CssClassRemove(this ComponentJson component, string value)
        {
            string cssClass = component.CssClass;
            string cssClassWholeWord = " " + cssClass + " ";
            if (cssClassWholeWord.Contains(" " + value + " "))
            {
                component.CssClass = cssClassWholeWord.Replace(" " + value + " ", "").Trim();
            }
        }
    }

    public class AppJson : Page
    {
        public AppJson()            
            : base(null)
        {

        }

        /// <summary>
        /// Returns NamingConvention for app related sql tables.
        /// </summary>
        protected virtual NamingConvention NamingConventionApp()
        {
            return new NamingConvention();
        }

        internal async Task<NavigateResult> NavigateInternalAsync(string path)
        {
            var args = new NavigateArgs(path);
            NavigateResult result = new NavigateResult();
            await NavigateAsync(args, result);
            return result;
        }

        /// <summary>
        /// Browser requests (GET) file to download or navigate to subpage. Inside this method no session data is available. Used for example to download a public available (*.pdf) file.
        /// </summary>
        protected internal virtual Task NavigateAsync(NavigateArgs args, NavigateResult result)
        {
            return Task.FromResult(0);
        }

        public class NavigateArgs
        {
            internal NavigateArgs(string path)
            {
                this.Path = path;
                if (UtilServer.PathIsFileName(path))
                {
                    this.FileName = UtilFramework.FolderNameParse(null, path);
                    this.FileNameExtension = UtilFramework.FileNameExtension(FileName);
                }
            }

            /// <summary>
            /// Gets Path. For example: "/Readme.txt" or "/contact/". Or "/", if user clicked browser back button.
            /// </summary>
            public string Path { get; private set; }

            /// <summary>
            /// Gets FileName. For example: "Readme.txt". Is null, if path is for example: "/contact/".
            /// </summary>
            public string FileName { get; private set; }

            /// <summary>
            /// Gets FileNameExtension. For example: ".txt".
            /// </summary>
            public string FileNameExtension { get; private set; }

            /// <summary>
            /// Returns true, if path starts with pathPrefix.
            /// </summary>
            /// <param name="pathPrefix">For example: "/cms/".</param>
            /// <param name="path">For example: "/contact/".</param>
            public bool IsPath(string pathPrefix, out string path)
            {
                if (!pathPrefix.StartsWith("/"))
                {
                    pathPrefix = "/" + pathPrefix;
                }
                if (!pathPrefix.EndsWith("/"))
                {
                    pathPrefix += "/";
                }
                bool result = Path.StartsWith(pathPrefix);
                if (result)
                {
                    path = Path.Substring(pathPrefix.Length - 1);
                }
                else
                {
                    path = null;
                }
                return result;
            }

            /// <summary>
            /// Returns true, if path starts with pathPrefix.
            /// </summary>
            /// <param name="pathPrefix">For example: "/cms/".</param>
            public bool IsPath(string pathPrefix)
            {
                return IsPath(pathPrefix, out _);
            }

            /// <summary>
            /// Returns true, if path starts with pathPrefix.
            /// </summary>
            /// <param name="pathPrefix">For example: "/cmsfile/".</param>
            /// <param name="fileName">For example: "about/Logo.png".</param>
            public bool IsFileName(string pathPrefix, out string fileName)
            {
                bool result = false;
                fileName = null;
                if (FileName != null)
                {
                    if (!pathPrefix.StartsWith("/"))
                    {
                        pathPrefix = "/" + pathPrefix;
                    }
                    if (!pathPrefix.EndsWith("/"))
                    {
                        pathPrefix += "/";
                    }
                    result = Path.StartsWith(pathPrefix);
                    if (result)
                    {
                        fileName = FileName.Substring(pathPrefix.Length - "/".Length);
                    }
                }
                return result;
            }
        }

        public class NavigateResult
        {
            /// <summary>
            /// Gets or sets IsSession. If true, session is requested and method NavigateSessionAsync(); with session data available is called next.
            /// </summary>
            public bool IsSession { get; set; }

            /// <summary>
            /// Gets or sets Data. If not null, this is the file data sent to the browser to download.
            /// </summary>
            public byte[] Data;
        }

        internal async Task<NavigateSessionResult> NavigateSessionInternalAsync(string path, bool isAddHistory)
        {
            var args = new NavigateArgs(path);
            var result = new NavigateSessionResult
            {
                Path = args.Path
            };
            await NavigateSessionAsync(args, result);
            if (result.IsPage)
            {
                if (UtilServer.Context.Request.Method == "GET")
                {
                    // Do not add history entry for any GET
                    isAddHistory = false;
                }
                if (RequestJson.CommandList.FirstOrDefault()?.Command == RequestCommand.NavigateLinkBackwardForward)
                {
                    // Do not add history entry if user clicked backward, forward navigation button.
                    isAddHistory = false;
                }
                if (isAddHistory)
                {
                    if (PathAddHistory != null)
                    {
                        throw new Exception("Only one PathAddHistory entry possible for one request!");
                    }
                    PathAddHistory = result.Path;
                }
            }
            return result;
        }

        /// <summary>
        /// Browser requests file to download or navigate to subpage. Inside this method session data is available. Used for example to download a not publicly available (*.pdf) file.
        /// </summary>
        protected internal virtual Task NavigateSessionAsync(NavigateArgs args, NavigateSessionResult result)
        {
            return Task.FromResult(0);
        }

        public class NavigateSessionResult
        {
            /// <summary>
            /// Gets IsPage. If true, requested url is a page. If false (and Data not null) requested url is a file.
            /// </summary>
            public bool IsPage
            {
                get
                {
                    return Data == null;
                }
            }

            /// <summary>
            /// Gets or sets Data. If not null, this is the file data sent to the browser to download.
            /// </summary>
            public byte[] Data;

            /// <summary>
            /// Gets or sets Path. For example: "/contact/" or "/signin/", if redirected.
            /// </summary>
            public string Path { get; set; }
        }

        /// <summary>
        /// Add navigate command to queue.
        /// </summary>
        /// <param name="path">For example "/contact/"</param>
        /// <param name="isAddHistory">If true, path is added to browser history.</param>
        internal void Navigate(string path, bool isAddHistory)
        {
            this.RequestJson.CommandAdd(new CommandJson { Command = RequestCommand.NavigateLink, Origin = RequestOrigin.Server, ComponentId = Id, NavigateLinkPath = path, NavigateLinkPathIsAddHistory = isAddHistory });
        }

        /// <summary>
        /// Add navigate command to queue.
        /// </summary>
        /// <param name="path">For example "/contact/"</param>
        public void Navigate(string path)
        {
            Navigate(path, isAddHistory: true);
        }

        private NamingConvention namingConventionFramework;

        private NamingConvention namingConventionApp;

        internal NamingConvention NamingConventionInternal(Type typeRow)
        {
            if (UtilDalType.TypeRowIsFrameworkDb(typeRow))
            {
                if (namingConventionFramework == null)
                {
                    namingConventionFramework = new NamingConvention();
                }
            }
            if (namingConventionApp == null)
            {
                namingConventionApp = NamingConventionApp();
            }
            return namingConventionApp;
        }

        internal async Task InitInternalAsync()
        {
            await InitAsync();
            UtilServer.Session.SetString("Main", string.Format("App start: {0}", UtilFramework.DateTimeToString(DateTime.Now.ToUniversalTime())));
        }

        internal async Task ProcessInternalAsync(AppJson appJson)
        {
            UtilStopwatch.TimeStart("Process");
            while (appJson.RequestJson.CommandGet() != null)
            {
                await UtilApp.ProcessHomeIsClickAsync(appJson);
                await UtilApp.ProcessNavigateLinkAsync(appJson); // Link POST instead of GET.
                await UtilGrid.ProcessAsync(appJson); // Process data grid.
                await UtilApp.ProcessBootstrapNavbarAsync(appJson);

                foreach (Page page in this.ComponentListAll().OfType<Page>())
                {
                    await page.ProcessAsync();
                }

                UtilApp.ProcessBootstrapModal(appJson); // Modal dialog window

                appJson.RequestJson.CommandNext();
            }
            
            UtilApp.DivContainerRender(appJson);
            UtilApp.BootstrapNavbarRender(appJson);

            UtilStopwatch.TimeStop("Process");
        }

        /// <summary>
        /// Gets RequestJson. Payload of current request.
        /// </summary>
        [Serialize(SerializeEnum.None)]
        internal RequestJson RequestJson;

        /// <summary>
        /// Gets or sets RequestCount. Used by client. Does not send new request while old is still pending.
        /// </summary>
        internal int RequestCount { get; set; }

        /// <summary>
        /// Gets ResponseCount. Used by server to verify incoming request matches last response.
        /// </summary>
        internal int ResponseCount { get; set; }

        /// <summary>
        /// Gets IsSessionExpired. If true, session expired and application has been recycled.
        /// </summary>
        public bool IsSessionExpired { get; internal set; }

        internal string Version { get; set; }

        internal string VersionBuild { get; set; }

        internal bool IsServerSideRendering { get; set; }

        internal string Session { get; set; }

        internal string SessionApp { get; set; }

        /// <summary>
        /// Gets or sets IsModal. Indicating an object PageModal exists in the component tree. 
        /// Used for example for html "body class='modal-open'" to enable vertical scroll bar.
        /// </summary>
        internal bool IsBootstrapModal { get; set; }

        /// <summary>
        /// Gets or sets IsReload. If true, client reloads page. For example if session expired.
        /// </summary>
        internal bool IsReload { get; set; }

        /// <summary>
        /// Gets RequestUrl. This value is set by the server. For example: http://localhost:49323/". Used by client for app.json post. See also method: UtilServer.RequestUrl();
        /// </summary>
        internal string RequestUrl { get; set; }

        /// <summary>
        /// Gets EmbeddedUrl. Value used by Angular client on first app.json POST to indicate application is embedded an running on other website.
        /// </summary>
        internal string EmbeddedUrl { get; set; }

        /// <summary>
        /// Gets or sets DownloadData Used to send file to download to client.. See also method Convert.ToBase64String();
        /// </summary>
        [Serialize(SerializeEnum.Client)]
        internal string DownloadData;

        /// <summary>
        /// Gets or sets PathAddHistory. This path is added by the browser to the navigation history. For example: "/contact/" or "/signin/", if redirected.
        /// </summary>
        [Serialize(SerializeEnum.Client)]
        internal string PathAddHistory;

        /// <summary>
        /// Gets or sets DownloadFileName. For example Grid.xlsx
        /// </summary>
        [Serialize(SerializeEnum.Client)]
        internal string DownloadFileName;

        /// <summary>
        /// Gets or sets DownloadContentType. See also method UtilServer.ContentType();
        /// </summary>
        [Serialize(SerializeEnum.Client)]
        internal string DownloadContentType;

        /// <summary>
        /// Send file with app.json response to download in client.
        /// </summary>
        internal void Download(byte[] data, string fileName) // Used for Excel export.
        {
            this.DownloadData = Convert.ToBase64String(data);
            this.DownloadFileName = fileName;
            this.DownloadContentType = UtilServer.ContentType(fileName);
        }

        /// <summary>
        /// Gets or sets IsScrollToTop. Used for example for session expired.
        /// </summary>
        [Serialize(SerializeEnum.Client)]
        public bool IsScrollToTop;
    }

    /// <summary>
    /// Json Button. Rendered as html button element.
    /// </summary>
    public class Button : ComponentJson
    {
        public Button(ComponentJson owner)
            : base(owner, nameof(Button))
        {

        }

        public string TextHtml;

        /// <summary>
        /// Gets IsClick. If true, user clicked the button.
        /// </summary>
        public bool IsClick
        {
            get
            {
                var commandJson = ((AppJson)Root).RequestJson.CommandGet();
                return commandJson.Command == RequestCommand.ButtonIsClick && commandJson.ComponentId == Id;
            }
        }
    }

    /// <summary>
    /// Json Div. Rendered as html div element.
    /// </summary>
    public class Div : ComponentJson
    {
        public Div(ComponentJson owner)
            : base(owner, nameof(Div))
        {

        }
    }

    /// <summary>
    /// Renders div with child divs without Angular selector div in between. Used for example for css flexbox, css grid and Bootstrap row.
    /// </summary>
    public class DivContainer : ComponentJson
    {
        public DivContainer(ComponentJson owner)
            : base(owner, nameof(DivContainer))
        {

        }
    }

    public enum BootstrapAlertEnum
    {
        None = 0,

        Info = 1,

        Success = 2,

        Warning = 3,

        Error = 4
    }

    public static class BootstrapExtension
    {
        /// <summary>
        /// Show bootstrap alert (on per page).
        /// </summary>
        public static Html BootstrapAlert(this Page page, string textHtml, BootstrapAlertEnum alertEnum, int index = 0)
        {
            string htmlTextAlert = "<div class='alert {{CssClass}}' role='alert'>{{TextHtml}}</div>";
            string cssClass = null;
            switch (alertEnum)
            {
                case BootstrapAlertEnum.Info:
                    cssClass = "alert-info";
                    break;
                case BootstrapAlertEnum.Success:
                    cssClass = "alert-success";
                    break;
                case BootstrapAlertEnum.Warning:
                    cssClass = "alert-warning";
                    break;
                case BootstrapAlertEnum.Error:
                    cssClass = "alert-danger";
                    break;
                default:
                    break;
            }
            htmlTextAlert = htmlTextAlert.Replace("{{CssClass}}", cssClass).Replace("{{TextHtml}}", textHtml);
            Html result = new Html(page)
            {
                TextHtml = htmlTextAlert
            };
            result.ComponentMove(index);
            return result;
        }
    }

    /// <summary>
    /// Data grid shows row as table, stack or form.
    /// </summary>
    public class Grid : ComponentJson
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public Grid(ComponentJson owner) 
            : base(owner, nameof(Grid))
        {
            this.Mode = GridMode.Table;
        }

        /// <summary>
        /// TypeRow of loaded data grid.
        /// </summary>
        [Serialize(SerializeEnum.Session)]
        internal Type TypeRow;

        /// <summary>
        /// DatabaseEnum of loaded grid.
        /// </summary>
        [Serialize(SerializeEnum.Session)]
        internal DatabaseEnum DatabaseEnum;

        /// <summary>
        /// Load data into grid. Override method Page.GridQuery(); to define query. It's also called to reload data.
        /// </summary>
        public async Task LoadAsync()
        {
            await UtilGrid.LoadAsync(this);
        }

        /// <summary>
        /// Gets or sets ConfigGridList. Can contain multiple configurations. See also property ConfigName.
        /// </summary>
        [Serialize(SerializeEnum.Session)]
        internal List<FrameworkConfigGridIntegrate> ConfigGridList;

        /// <summary>
        /// Gets or sets ConfigFieldList. Can contain multiple configurations. See also property ConfigName.
        /// </summary>
        [Serialize(SerializeEnum.Session)]
        internal List<FrameworkConfigFieldIntegrate> ConfigFieldList;

        /// <summary>
        /// Gets or sets RowListInternal. Data rows loaded from database.
        /// </summary>
        [Serialize(SerializeEnum.Session)]
        internal List<Row> RowListInternal; // TODO Remove empty RowListInternal from JsonClient.

        /// <summary>
        /// Gets RowList. Data rows loaded from database.
        /// </summary>
        public IReadOnlyList<Row> RowList
        {
            get
            {
                return RowListInternal;
            }
        }

        /// <summary>
        /// Gets or sets ConfigName. Switch to select current configuration. Multiple configurations can be stored.
        /// </summary>
        [Serialize(SerializeEnum.Session)]
        internal string ConfigName;

        /// <summary>
        /// Gets or sets ColumnList. Does not include hidden columns.
        /// </summary>
        [Serialize(SerializeEnum.Session)]
        internal List<GridColumn> ColumnList;

        [Serialize(SerializeEnum.Session)]
        internal List<GridRowState> RowStateList;

        /// <summary>
        /// Gets or sets GridCellList.
        /// </summary>
        internal List<GridCell> CellList;

        [Serialize(SerializeEnum.Session)]
        internal List<GridFilterValue> FilterValueList;

        [Serialize(SerializeEnum.Session)]
        internal List<GridSortValue> SortValueList;

        [Serialize(SerializeEnum.Session)]
        internal int OffsetRow;

        [Serialize(SerializeEnum.Session)]
        internal int OffsetColumn;

        /// <summary>
        /// Gets or sets StyleColumn. This is the css grid style attribute grid-template-columns.
        /// </summary>
        internal string StyleColumn;

        /// <summary>
        /// Gets or sets IsGridLookup. If true, this grid is a lookup data grid.
        /// </summary>
        [Serialize(SerializeEnum.Session)]
        internal bool IsGridLookup;

        /// <summary>
        /// Gets or sets GridLookup. Reference to lookup grid for this grid.
        /// </summary>
        internal Grid GridLookup;

        /// <summary>
        /// Gets or sets GridDest. If this data grid is a lookup grid, this is the destination data grid to write to after selection.
        /// </summary>
        internal Grid GridDest;

        /// <summary>
        /// Gets or sets GridLookupDestRowStateId. If this data grid is a lookup grid, this is the destination data row to write to after selection.
        /// </summary>
        [Serialize(SerializeEnum.Session)]
        internal int? GridLookupDestRowStateId;

        /// <summary>
        /// Gets or sets GridLookupDestFieldNameCSharp. If this data grid is a lookup grid, this is the destination grid column (to write to) after selection.
        /// </summary>
        [Serialize(SerializeEnum.Session)]
        internal string GridLookupDestFieldNameCSharp;

        /// <summary>
        /// Gets or sets RowSelected. Currently selected data row by user. Set queues command.
        /// </summary>
        [Serialize(SerializeEnum.None)]
        public Row RowSelected
        {
            get
            {
                Row result = null;
                foreach (var rowState in RowStateList)
                {
                    if (rowState.IsSelect && rowState.RowEnum == GridRowEnum.Index)
                    {
                        result = RowListInternal[rowState.RowId.Value - 1];
                        break;
                    }
                }
                return result;
            }
            set
            {
                UtilGrid.QueueRowIsClick(this, value);
            }
        }

        /// <summary>
        /// Gets RowSelectedRowStateId. Currently selected data row by user.
        /// </summary>
        internal int? RowSelectedRowStateId
        {
            get
            {
                int? result = null;
                foreach (var rowState in RowStateList)
                {
                    if (rowState.IsSelect && rowState.RowEnum == GridRowEnum.Index)
                    {
                        result = rowState.Id;
                        break;
                    }
                }
                return result;
            }
        }

        [Serialize(SerializeEnum.Session)]
        internal GridMode Mode;

        /// <summary>
        /// Returns query to load data grid. Override this method to define sql query.
        /// </summary>
        /// <returns>If return value is null, grid has no header columns and no rows. If value is equal to method Data.QueryEmpty(); grid has header columns but no data rows.</returns>
        internal virtual IQueryable QueryInternal()
        {
            return null;
        }

        /// <summary>
        /// Override this method to reduce session state size. Truncate big data cells in grid. Typically, big data cells are opened individually on a separate page.
        /// </summary>
        internal virtual void TruncateInternal(List<Row> rowList)
        {

        }

        /// <summary>
        /// Override this method for custom implementation. Method is called when data row has been selected. Reload for example a detail data grid.
        /// </summary>
        protected virtual internal Task RowSelectedAsync()
        {
            return Task.FromResult(0);
        }

        protected virtual internal void CellParseFilter(string fieldName, string text, GridCellParseFilterResult result)
        {

        }

        virtual internal Task UpdateInternalAsync(Row rowOld, Row row, DatabaseEnum databaseEnum, UpdateResultInternal result)
        {
            return Task.FromResult(0);
        }

        internal class UpdateResultInternal
        {
            public bool IsHandled;

            public static Grid<TRow>.UpdateResult Convert<TRow>(UpdateResultInternal value, TRow row) where TRow : Row
            {
                return new Grid<TRow>.UpdateResult { Row = row, IsHandled = value.IsHandled };
            }

            public static void Convert<TRow>(Grid<TRow>.UpdateResult value, ref UpdateResultInternal result) where TRow : Row
            {
                result.IsHandled = value.IsHandled;
            }
        }

        virtual internal Task InsertInternalAsync(Row row, DatabaseEnum databaseEnum, InsertResultInternal result)
        {
            return Task.FromResult(0);
        }

        internal class InsertResultInternal
        {
            public bool IsHandled;

            public static Grid<TRow>.InsertResult Convert<TRow>(InsertResultInternal value, TRow row) where TRow : Row
            {
                return new Grid<TRow>.InsertResult { Row = row, IsHandled = value.IsHandled };
            }

            public static void Convert<TRow>(Grid<TRow>.InsertResult value, ref InsertResultInternal result) where TRow : Row
            {
                result.IsHandled = value.IsHandled;
            }
        }

        virtual internal string CellTextInternal(Row row, string fieldName, string text)
        {
            return text;
        }

        virtual internal void CellParseInternal(Row row, string fieldName, string text, ParseResultInternal result)
        {

        }

        virtual internal Task CellParseInternalAsync(Row row, string fieldName, string text, ParseResultInternal result)
        {
            return Task.FromResult(0);
        }

        virtual internal void CellParseFileUploadInternal(GridRowEnum rowEnum, Row row, string fieldName, string fileName, byte[] data, ParseResultInternal result)
        {

        }

        internal class ParseResultInternal
        {
            /// <summary>
            /// Gets or sets IsHandled. If true, framework does no further parsing of user entered text.
            /// </summary>
            public bool IsHandled;

            /// <summary>
            /// Gets or sets ErrorParse. For example: User entered text is not a number.
            /// </summary>
            public string ErrorParse;

            public static Grid<TRow>.ParseResult Convert<TRow>(ParseResultInternal value, TRow row) where TRow : Row
            {
                return new Grid<TRow>.ParseResult { Row = row, ErrorParse = value.ErrorParse, IsHandled = value.IsHandled };
            }

            public static void Convert<TRow>(Grid<TRow>.ParseResult value, ref ParseResultInternal result) where TRow : Row
            {
                result.ErrorParse = value.ErrorParse;
                result.IsHandled = value.IsHandled;
            }
        }

        public enum CellAnnotationAlignEnum
        {
            /// <summary>
            /// None.
            /// </summary>
            None = 0,

            /// <summary>
            /// Align text left.
            /// </summary>
            Left = 1,

            /// <summary>
            /// Align data grid cell text in center .
            /// </summary>
            Center = 2,

            /// <summary>
            /// Align data grid cell text right.
            /// </summary>
            Right = 3,
        }

        virtual internal void CellAnnotationInternal(GridRowEnum rowEnum, Row row, string fieldName, AnnotationResult result)
        {

        }

        /// <summary>
        /// Provides additional annotation information for a data grid cell.
        /// </summary>
        public class AnnotationResult
        {
            /// <summary>
            /// Gets or sets Html. Use for example to transform plain text into a hyper link. For empty html set "&nbsp;" to keep the layout consistent with none empty html fields.
            /// </summary>
            public string Html;

            /// <summary>
            /// Gets or sets HtmlIsEdit. If true, html is rendered and additionally input text box is shown to edit plain html. Applies only if Html is not null.
            /// </summary>
            public bool HtmlIsEdit;

            /// <summary>
            /// Gets or sets HtmlLeft. Use for example to render an image on the left hand side in the cell.
            /// </summary>
            public string HtmlLeft;

            /// <summary>
            /// Gets or sets HtmlRight. Use for example to render an indicator icon on the right hand side in the cell. 
            /// </summary>
            public string HtmlRight;

            /// <summary>
            /// Gets or sets IsReadOnly. If true, user can not edit text.
            /// </summary>
            public bool IsReadOnly;

            /// <summary>
            /// Gets or sets IsPassword. If true, user can not read text.
            /// </summary>
            public bool IsPassword;

            /// <summary>
            /// Gets or sets IsFileUpload. If true, user can upload cell text (data) with file upload.
            /// </summary>
            public bool IsFileUpload;

            /// <summary>
            /// Gets or sets Placeholder. Shown as gray text when edit field is empty. For example: "Street" or "Search".
            /// </summary>
            public string PlaceHolder;

            /// <summary>
            /// Gets or sets Align. Defines text allign of centent in the data grid cell.
            /// </summary>
            public CellAnnotationAlignEnum Align;
        }

        /// <summary>
        /// Contains one query for data grid configuration and one query for data grid field configuration.
        /// </summary>
        public class QueryConfigResult
        {
            /// <summary>
            /// Gets or sets ConfigGridQuery. Should return one record.
            /// </summary>
            public IQueryable<FrameworkConfigGridIntegrate> ConfigGridQuery { get; set; }

            /// <summary>
            /// Gets or sets ConfigFieldQuery.
            /// </summary>
            public IQueryable<FrameworkConfigFieldIntegrate> ConfigFieldQuery { get; set; }
        }

        /// <summary>
        /// Returns configuration query of data grid to load.
        /// </summary>
        /// <param name="tableName">TableName as declared in CSharp code. Type of row to load.</param>
        protected virtual internal void QueryConfig(string tableName, QueryConfigResult result)
        {
            result.ConfigGridQuery = Data.Query<FrameworkConfigGridIntegrate>().Where(item => item.TableNameCSharp == tableName /* && item.ConfigName == grid.ConfigName */); // Multiple configuration can be loaded. See also Grid.Data.

            result.ConfigFieldQuery = Data.Query<FrameworkConfigFieldIntegrate>().Where(item => item.TableNameCSharp == tableName /* && item.ConfigName == grid.ConfigName */); // Multiple configuration can be Loaded. See also Grid.GridData.

            // Example for static configuration:
            // result.ConfigGridQuery = new [] { new FrameworkConfigGridIntegrate { RowCountMax = 2 } }.AsQueryable();
        }

        virtual internal IQueryable LookupQueryInternal(Row row, string fieldName, string text)
        {
            return null; // No lookup data grid.
        }

        /// <summary>
        /// Returns configuration query of lookup data grid to load.
        /// </summary>
        /// <param name="gridLookup">Lookup data grid for which to load the configuration.</param>
        /// <param name="tableName">TableName as declared in CSharp code.</param>
        protected virtual internal void LookupQueryConfig(Grid gridLookup, string tableName, QueryConfigResult result)
        {
            result.ConfigGridQuery = Data.Query<FrameworkConfigGridIntegrate>().Where(item => item.TableNameCSharp == tableName && item.ConfigName == gridLookup.ConfigName);

            result.ConfigFieldQuery = Data.Query<FrameworkConfigFieldIntegrate>().Where(item => item.TableNameCSharp == tableName && item.ConfigName == gridLookup.ConfigName);

            // Example for static configuration:
            // result.ConfigGridQuery = new [] { new FrameworkConfigGridIntegrate { RowCountMax = 2 } }.AsQueryable();
        }

        /// <summary>
        /// Override this method to extract and return text from lookup grid row for further processing. 
        /// Process wise there is no difference between user selecting a row on the lookup grid or entering text manually.
        /// <param name="result">Returns text like entered by user for further processing.</param>
        protected virtual internal void LookupRowSelected(LookupRowSelectedArgs args, LookupRowSelectedResult result)
        {

        }

        public class LookupRowSelectedArgs
        {
            /// <summary>
            /// Gets RowSelected. This is the row which has been clicked by the user in the lookup window.
            /// </summary>
            public Row RowSelected { get; internal set; }

            /// <summary>
            /// Gets FieldName. This is the FieldName for which the lookup window is open.
            /// </summary>
            public string FieldName { get; internal set; }
        }

        public class LookupRowSelectedResult
        {
            /// <summary>
            /// Gets or sets Text. Like the text entered by user for further processing.
            /// </summary>
            public string Text { get; set; }
        }
    }

    public class GridCellParseFilterResult
    {
        public GridCellParseFilterResult(GridFilter gridFilter)
        {
            this.GridFilter = gridFilter;
        }

        public readonly GridFilter GridFilter;

        public bool IsHandled;

        public string ErrorParse;
    }

    public class Grid<TRow> : Grid where TRow : Row
    {
        public Grid(ComponentJson owner) 
            : base(owner)
        {

        }

        internal override IQueryable QueryInternal()
        {
            QueryArgs args = new QueryArgs();
            if (typeof(TRow) == typeof(Row))
            {
                args.Query = null; // Data.QueryEmpty<TRow>(); is not possible since class Row has no TableNameSql defined.
            }
            else
            {
                args.Query = Data.Query<TRow>();
            }
            QueryResult result = new QueryResult
            {
                Query = args.Query // Default query
            };
            Query(args, result);
            return result.Query;
        }

        /// <summary>
        /// Returns query to load data grid.
        /// </summary>
        protected virtual void Query(QueryArgs args, QueryResult result)
        {

        }

        public class QueryArgs
        {
            /// <summary>
            /// Gets Query. This is the default query.
            /// </summary>
            public IQueryable<TRow> Query { get; internal set; }
        }

        public class QueryResult
        {
            /// <summary>
            /// Gets or sets Query. Query used to load data grid.
            /// </summary>
            public IQueryable<TRow> Query { get; set; }
        }

        internal override void TruncateInternal(List<Row> rowList)
        {
            Truncate(new TruncateArgs { RowList = rowList.Cast<TRow>().ToList() });
        }

        /// <summary>
        /// Override this method to reduce session state size. Truncate big data cells in grid. Typically, big data cells are opened individually on a separate page.
        /// </summary>
        protected virtual void Truncate(TruncateArgs args)
        {

        }

        public class TruncateArgs
        {
            /// <summary>
            /// Gets RowList. Truncate big data cells on these rows to reduce session state size.
            /// </summary>
            public List<TRow> RowList { get; internal set; }
        }

        internal override async Task UpdateInternalAsync(Row rowOld, Row row, DatabaseEnum databaseEnum, UpdateResultInternal result)
        {
            UpdateResult resultLocal = UpdateResultInternal.Convert(result, (TRow)row);
            await UpdateAsync(new UpdateArgs { RowOld = (TRow)rowOld, Row = (TRow)row, DatabaseEnum = databaseEnum }, resultLocal);
            UpdateResultInternal.Convert(resultLocal, ref result);
        }

        /// <summary>
        /// Override this method for custom grid save implementation. Return isHandled.
        /// </summary>
        /// <param name="result">Returns true, if custom save was handled. If false, framework will handle update.</param>
        protected virtual Task UpdateAsync(UpdateArgs args, UpdateResult result)
        {
            return Task.FromResult(0);
        }

        public class UpdateArgs
        {
            /// <summary>
            /// Gets RowOld. Data row with old data to update.
            /// </summary>
            public TRow RowOld { get; internal set; }

            /// <summary>
            /// Gets Row. New data row to save to database.
            /// </summary>
            public TRow Row { get; internal set; }

            public DatabaseEnum DatabaseEnum { get; internal set; }
        }

        public class UpdateResult
        {
            /// <summary>
            /// Gets or sets IsHandled. If true, framework does not update data row.
            /// </summary>
            public bool IsHandled;

            /// <summary>
            /// Gets Row. New data row to save to database.
            /// </summary>
            public TRow Row { get; internal set; }
        }

        internal override async Task InsertInternalAsync(Row row, DatabaseEnum databaseEnum, InsertResultInternal result)
        {
            InsertResult resultLocal = InsertResultInternal.Convert(result, (TRow)row);
            await InsertAsync(new InsertArgs { Row = (TRow)row, DatabaseEnum = databaseEnum }, resultLocal);
            InsertResultInternal.Convert(resultLocal, ref result);
        }

        /// <summary>
        /// Override this method for custom grid save implementation. Returns isHandled.
        /// </summary>
        /// <param name="result">Returns true, if custom save was handled.</param>
        protected virtual Task InsertAsync(InsertArgs args, InsertResult result)
        {
            return Task.FromResult(0);
        }

        public class InsertArgs
        {
            /// <summary>
            /// Gets Row. Data row to insert. Set new primary key on this row.
            /// </summary>
            public TRow Row { get; internal set; }

            public DatabaseEnum DatabaseEnum { get; internal set; }
        }

        public class InsertResult
        {
            /// <summary>
            /// Gets or sets IsHandled. If true, framework does not insert data row.
            /// </summary>
            public bool IsHandled;

            /// <summary>
            /// Gets Row. Data row to insert. Set new primary key on this row.
            /// </summary>
            public TRow Row { get; internal set; }
        }

        /// <summary>
        /// Gets RowList. Data rows loaded from database.
        /// </summary>
        public new IReadOnlyList<TRow> RowList
        {
            get
            {
                return base.RowListInternal.Cast<TRow>().ToList();
            }
        }

        /// <summary>
        /// Gets RowSelected. Currently selected data row by user. Set queues command.
        /// </summary>
        [Serialize(SerializeEnum.None)]
        public new TRow RowSelected
        {
            get
            {
                return (TRow)base.RowSelected;
            }
            set
            {
                base.RowSelected = value;
            }
        }

        internal override string CellTextInternal(Row row, string fieldName, string text)
        {
            var args = new CellTextArgs { Row = (TRow)row, FieldName = fieldName, Text = text };
            var result = new CellTextResult { Text = text };
            CellText(args, result);
            return result.Text;
        }

        /// <summary>
        /// Override this method for custom implementation of converting database value to front end grid cell text. Called only if database value is not null.
        /// </summary>
        protected virtual void CellText(CellTextArgs args, CellTextResult result)
        {

        }

        public class CellTextArgs
        {
            /// <summary>
            /// Data grid row.
            /// </summary>
            public TRow Row { get; internal set; }

            /// <summary>
            /// FieldName as declared in CSharp code. Data grid column name.
            /// </summary>
            public string FieldName { get; internal set; }

            /// <summary>
            /// Default database value front end grid cell text.
            /// </summary>
            public string Text { get; internal set; }
        }

        public class CellTextResult
        {
            /// <summary>
            /// Custom database value front end grid cell text.
            /// </summary>
            public string Text;
        }

        internal override void CellParseInternal(Row row, string fieldName, string text, ParseResultInternal result)
        {
            var resultLocal = ParseResultInternal.Convert(result, (TRow)row);
            CellParse(new ParseArgs { Row = (TRow)row, FieldName = fieldName, Text = text }, resultLocal);
            ParseResultInternal.Convert(resultLocal, ref result);
        }

        /// <summary>
        /// Parse user entered text and assign it row. Write parsed value to row. (Or for example multiple fields on row for Uom)
        /// </summary>
        /// <param name="result">Set result.IsHandled to true.</param>
        protected virtual void CellParse(ParseArgs args, ParseResult result)
        {

        }

        public class ParseArgs
        {
            /// <summary>
            /// Write custom parsed value to row.
            /// </summary>
            public TRow Row { get; internal set; }

            /// <summary>
            /// FieldName as declared in CSharp code. Data grid column name.
            /// </summary>
            public string FieldName { get; internal set; }

            /// <summary>
            /// User entered text. It can be empty but never null.
            /// </summary>
            public string Text { get; internal set; }
        }

        public class ParseResult
        {
            /// <summary>
            /// Write custom parsed value to row.
            /// </summary>
            public TRow Row { get; internal set; }

            /// <summary>
            /// Gets or sets IsHandled. If true, framework does no further parsing of user entered text.
            /// </summary>
            public bool IsHandled;

            /// <summary>
            /// Gets or sets ErrorParse. For example: User entered text is not a number.
            /// </summary>
            public string ErrorParse;
        }

        internal override async Task CellParseInternalAsync(Row row, string fieldName, string text, ParseResultInternal result)
        {
            var resultLocal = ParseResultInternal.Convert(result, (TRow)row);
            await CellParseAsync(new ParseArgs { Row = (TRow)row, FieldName = fieldName, Text = text }, resultLocal);
            ParseResultInternal.Convert(resultLocal, ref result);
        }

        /// <summary>
        /// Parse text user entered in cell and write it into parameter 'row'.
        /// </summary>
        /// <param name="result">Return isHandled. If true, framework does no further parsing of user entered text.</param>
        /// <returns></returns>
        protected virtual Task CellParseAsync(ParseArgs args, ParseResult result)
        {
            return Task.FromResult(0);
        }

        internal override void CellParseFileUploadInternal(GridRowEnum rowEnum, Row row, string fieldName, string fileName, byte[] data, ParseResultInternal result)
        {
            var resultLocal = ParseResultInternal.Convert(result, (TRow)row);
            CellParseFileUpload(new FileUploadArgs { Row = (TRow)row, FieldName = fieldName, FileName = fileName, Data = data, IsNew = rowEnum == GridRowEnum.New }, resultLocal);
            ParseResultInternal.Convert(resultLocal, ref result);
        }

        /// <summary>
        /// Parse user uploaded file and assign it to row.
        /// </summary>
        /// <param name="result">Set result.IsHandled to true.</param>
        protected virtual void CellParseFileUpload(FileUploadArgs args, ParseResult result)
        {

        }

        public class FileUploadArgs
        {
            /// <summary>
            /// Gets Row.  Write custom parsed value to row.
            /// </summary>
            public TRow Row { get; internal set; }

            /// <summary>
            /// Gets FieldName. As declared in CSharp code. Data grid column name.
            /// </summary>
            public string FieldName { get; internal set; }

            /// <summary>
            /// Gets FileName. User uploaded file.
            /// </summary>
            public string FileName { get; internal set; }

            /// <summary>
            /// Gets Data. From user uploaded file.
            /// </summary>
            public byte[] Data { get; internal set; }

            /// <summary>
            /// Gets IsNew. If true, file upload is for new row.
            /// </summary>
            public bool IsNew { get; internal set; }
        }

        internal override void CellAnnotationInternal(GridRowEnum rowEnum, Row row, string fieldName, AnnotationResult result)
        {
            var args = new AnnotationArgs { Row = (TRow)row, FieldName = fieldName };

            if (rowEnum == GridRowEnum.Index)
            {
                CellAnnotationRow(args, result);
            }
            else
            {
                args.IsFilter = rowEnum == GridRowEnum.Filter;
                args.IsNew = rowEnum == GridRowEnum.New;
                CellAnnotation(args, result);
            }
        }

        /// <summary>
        /// Override this method to provide additional custom annotation information for a data grid cell. Annotation is updated for every cell on same row when user changes text in one cell.
        /// </summary>
        /// <param name="result">Returns data grid cell annotation.</param>
        protected virtual void CellAnnotationRow(AnnotationArgs args, AnnotationResult result)
        {

        }

        /// <summary>
        /// Annotation for data row.
        /// </summary>
        public class AnnotationArgs
        {
            /// <summary>
            /// Data grid row. Null, if filter or new data row.
            /// </summary>
            public TRow Row { get; internal set; }

            /// <summary>
            /// FieldName as declared in CSharp code. Data grid column name.
            /// </summary>
            public string FieldName { get; internal set; }

            /// <summary>
            /// Gets IsFilter. If true, annotation is for filter row.
            /// </summary>
            public bool IsFilter { get; internal set; }

            /// <summary>
            /// Gets IsNew. If true, annotation is for new row.
            /// </summary>
            public bool IsNew { get; internal set; }
        }

        /// <summary>
        /// Override this method to provide annotation information for data grid cell in filter and new row.
        /// </summary>
        /// <param name="result">Returns data grid cell annotation.</param>
        protected virtual void CellAnnotation(AnnotationArgs args, AnnotationResult result)
        {

        }

        internal override IQueryable LookupQueryInternal(Row row, string fieldName, string text)
        {
            LookupQueryResult result = new LookupQueryResult();
            LookupQuery(new LookupQueryArgs { Row = (TRow)row, FieldName = fieldName, Text = text }, result);
            return result.Query;
        }

        /// <summary>
        /// Override this method to return a linq query for the lookup data grid.
        /// </summary>
        /// <param name="result">Returns query for lookup window.</param>
        protected virtual void LookupQuery(LookupQueryArgs args, LookupQueryResult result)
        {

        }

        public class LookupQueryArgs
        {
            /// <summary>
            /// Gets Row. This is the row user is editing.
            /// </summary>
            public TRow Row { get; internal set; }

            /// <summary>
            /// Gets FieldName. As declared in CSharp code. This is the field the user is editing.
            /// </summary>
            public string FieldName { get; internal set; }

            /// <summary>
            /// Gets Text. This is the text the user entered.
            /// </summary>
            public string Text { get; set; }
        }

        public class LookupQueryResult
        {
            /// <summary>
            /// Gets or sets Query. This is the query for the lookup window.
            /// </summary>
            public IQueryable Query { get; set; }
        }
    }

    /// <summary>
    /// Data grid display mode.
    /// </summary>
    internal enum GridMode
    {
        None = 0,
        Table = 1,
        Stack = 2,
        Form = 3
    }

    /// <summary>
    /// Wrapper providing value store functions.
    /// </summary>
    public sealed class GridFilter
    {
        internal GridFilter(Grid grid)
        {
            this.Grid = grid;
        }

        internal readonly Grid Grid;

        /// <summary>
        /// Returns filter value for field.
        /// </summary>
        private GridFilterValue FilterValue(string fieldNameCSharp)
        {
            GridFilterValue result = Grid.FilterValueList.Where(item => item.FieldNameCSharp == fieldNameCSharp).SingleOrDefault();
            if (result == null)
            {
                result = new GridFilterValue(fieldNameCSharp);
                Grid.FilterValueList.Add(result);
            }
            return result;
        }

        /// <summary>
        /// Set filter value on a column. If text is not equal to text user entered, it will appear as soon as user leves field.
        /// </summary>
        /// <param name="isClear">If true, filter is not applied.</param>
        public void ValueSet(string fieldNameCSharp, object filterValue, FilterOperator filterOperator, string text, bool isClear = false)
        {
            GridFilterValue result = FilterValue(fieldNameCSharp);
            result.FilterValue = filterValue;
            result.FilterOperator = filterOperator;
            if (result.IsFocus == false)
            {
                result.Text = text;
            }
            else
            {
                result.TextLeave = text;
            }
            result.IsClear = isClear;
        }

        internal void TextSet(string fieldNameCSharp, string text)
        {
            Grid.FilterValueList.ForEach(item => item.IsFocus = false);
            GridFilterValue result = FilterValue(fieldNameCSharp);
            result.Text = text;
            result.IsFocus = true;
        }

        /// <summary>
        /// (FieldNameCSharp, FilterValue).
        /// </summary>
        internal Dictionary<string, GridFilterValue> FilterValueList()
        {
            var result = new Dictionary<string, GridFilterValue>();
            if (Grid.FilterValueList != null)
            {
                foreach (var item in Grid.FilterValueList)
                {
                    result.Add(item.FieldNameCSharp, item);
                }
            }
            return result;
        }
    }

    /// <summary>
    /// Stores successfully parsed filter value and operator.
    /// </summary>
    internal sealed class GridFilterValue
    {
        public GridFilterValue(string fieldNameCSharp)
        {
            this.FieldNameCSharp = fieldNameCSharp;
        }

        public readonly string FieldNameCSharp;

        public FilterOperator FilterOperator;

        /// <summary>
        /// Gets or sets FilterValue. This is the successfully parsed user input value.
        /// </summary>
        public object FilterValue;

        /// <summary>
        /// Gets or sets IsClear. If true, filter has been cleared and is not applied.
        /// </summary>
        public bool IsClear;

        /// <summary>
        /// Gets or sets Text of successfully parsed filter.
        /// </summary>
        public string Text;

        /// <summary>
        /// Gets or sets TextLeave. If filter has user input focus, parser can not override text untill user leaves the field.
        /// </summary>
        public string TextLeave;

        /// <summary>
        /// Gets or sets IsFocus. If true, filter has user input focus.
        /// </summary>
        public bool IsFocus;
    }

    internal sealed class GridSortValue
    {
        public GridSortValue(string fieldNameCSharp)
        {
            this.FieldNameCSharp = fieldNameCSharp;
        }

        public readonly string FieldNameCSharp;

        public bool IsSort;

        public static bool? IsSortGet(Grid grid, string fieldNameCSharp)
        {
            bool? result = null;
            var value = grid.SortValueList?.FirstOrDefault();
            if (value != null && value.FieldNameCSharp == fieldNameCSharp)
            {
                result = value.IsSort;
            }
            return result;
        }

        public static void IsSortSwitch(Grid grid, string fieldNameCSharp)
        {
            var value = grid.SortValueList.FirstOrDefault();
            if (value != null && value.FieldNameCSharp == fieldNameCSharp)
            {
                value.IsSort = !value.IsSort; // Switch order
            }
            else
            {
                grid.SortValueList.Insert(0, new GridSortValue(fieldNameCSharp) { IsSort = false });
            }
            while (grid.SortValueList.Count > 2) // Order by then order by (max two levels).
            {
                grid.SortValueList.RemoveAt(grid.SortValueList.Count - 1);
            }
        }
    }

    /// <summary>
    /// Not sent to client.
    /// </summary>
    internal sealed class GridColumn
    {
        public int Id;

        public string FieldNameCSharp;

        /// <summary>
        /// Gets or sets ColumnText. This is the header text for filter.
        /// </summary>
        public string ColumnText;

        /// <summary>
        /// Gets or sets Description. Shown with an information icon in header.
        /// </summary>
        public string Description;

        /// <summary>
        /// Gets or sets IsVisible. If true, column is shown in data grid.
        /// </summary>
        public bool IsVisible;

        public bool IsVisibleScroll;

        /// <summary>
        /// Gets or sets Sort. Order as defined in data grid field config.
        /// </summary>
        public double? Sort;

        /// <summary>
        /// Gets or sets SortField. Order as defined in sql database schema.
        /// </summary>
        public int SortField;

        /// <summary>
        /// Gets or sets Width. This is the css grid style property grid-template-columns Width.
        /// </summary>
        public string Width;
    }

    /// <summary>
    /// Keeps track of data row state. Not sent to client.
    /// </summary>
    internal sealed class GridRowState
    {
        public int Id;

        public GridRowEnum RowEnum;

        public int? RowId; // Filter does not have a data row.

        /// <summary>
        /// Gets or sets IsSelect. User clicked and selected this data row.
        /// </summary>
        public bool IsSelect;

        /// <summary>
        /// Gets or sets IsVisibleScroll. For vertical paging (no database select).
        /// </summary>
        public bool IsVisibleScroll;

        /// <summary>
        /// Gets or sets Row. Data row to update (index) or insert (new) into database.
        /// </summary>
        public Row Row;
    }

    internal enum GridCellEnum
    {
        None = 0,

        /// <summary>
        /// Data grid filter cell. <see cref="GridRowEnum.Filter"/>
        /// </summary>
        Filter = 1,

        /// <summary>
        /// Data grid cell. <see cref="GridRowEnum.Index"/>
        /// </summary>
        Index = 2,

        /// <summary>
        /// Data grid cell. <see cref="GridRowEnum.New"/>
        /// </summary>
        New = 3,

        /// <summary>
        /// Column header with IsSort.
        /// </summary>
        HeaderColumn = 4,

        /// <summary>
        /// Cell label in stack mode.
        /// </summary>
        HeaderRow = 5,

        /// <summary>
        /// Separator label in stack mode.
        /// </summary>
        Separator = 6,
    }

    /// <summary>
    /// Grid cell display sent to client. Unlike GridColumn a cell it is not persistent and lives only while it is IsVisibleScroll or contains ErrorParse.
    /// </summary>
    internal sealed class GridCell
    {
        /// <summary>
        /// Gets or sets Id. Sent back by client with <see cref="RequestJson.GridCellId"/>.
        /// </summary>
        public int Id;

        [Serialize(SerializeEnum.Session)]
        public int ColumnId;

        [Serialize(SerializeEnum.Session)]
        public int RowStateId;

        public GridCellEnum CellEnum;

        /// <summary>
        /// Gets or sets ColumnText. Header for Filter.
        /// </summary>
        public string ColumnText;

        /// <summary>
        /// Gets or sets json text. Can be null but never empty.
        /// </summary>
        public string Text;

        /// <summary>
        /// Gets or sets TextLeave. If not null, client writes TextLeave into cell if focus is lost. This prevents overriding text while user is editing cell. Can be null or empty.
        /// </summary>
        public string TextLeave;

        /// <summary>
        /// Gets or sets TextOld. This is the text before save.
        /// </summary>
        [Serialize(SerializeEnum.Session)]
        public string TextOld;

        /// <summary>
        /// Gets IsModified. If true, user changed text.
        /// </summary>
        [Serialize(SerializeEnum.Session)]
        public bool IsModified;

        /// <summary>
        /// Gets or sets ErrorParse. Text user entered could not be parsed and written to row.
        /// </summary>
        public string ErrorParse;

        /// <summary>
        /// Gets or sets ErrorSave. Row could not be saved to the database.
        /// </summary>
        public string ErrorSave;

        public string Warning;

        public string Placeholder;

        public string Description;

        /// <summary>
        /// Gets or sets IsSelect. If true, cell belongs to selected row.
        /// </summary>
        public bool IsSelect;

        /// <summary>
        /// Gets or sets IsSort. Display column sort triangle.
        /// </summary>
        public bool? IsSort;

        /// <summary>
        /// Gets or sets IsVisibleScroll. If true, cell is visible in scrallable range. If false, cell is not sent to client.
        /// </summary>
        [Serialize(SerializeEnum.Session)]
        public bool IsVisibleScroll;

        /// <summary>
        /// Gets or sets Width. Used for user column resize. See also StyleColumn.
        /// </summary>
        public string Width;

        /// <summary>
        /// Gets or sets GridLookup.
        /// </summary>
        [Serialize(SerializeEnum.Both)] // By default, reference to ComponentJson is not sent to client. Serialize grid to client exclusively. JsonSession serializes it as reference.
        public Grid GridLookup;

        /// <summary>
        /// Gets or sets Html. Use for example to transform plain text into a hyper link.
        /// </summary>
        public string Html;

        /// <summary>
        /// Gets or sets HtmlIsEdit. If true, html is rendered and additionally input text box is shown to edit plain html. Applies only if Html is not null.
        /// </summary>
        public bool HtmlIsEdit;

        /// <summary>
        /// Gets or sets HtmlLeft. Use for example to render an image on the left hand side in the cell.
        /// </summary>
        public string HtmlLeft;

        /// <summary>
        /// Gets or sets HtmlRight. Use for example to render an indicator icon on the right hand side in the cell. 
        /// </summary>
        public string HtmlRight;

        /// <summary>
        /// Gets or sets IsReadOnly. If true, user can not edit text.
        /// </summary>
        public bool IsReadOnly;

        /// <summary>
        /// Gets or sets IsPassword. If true, user can not read text.
        /// </summary>
        public bool IsPassword;

        /// <summary>
        /// Gets or sets Align. Defines text allign of centent in the data grid cell.
        /// </summary>
        public Grid.CellAnnotationAlignEnum Align;

        /// <summary>
        /// Gets or sets IsFileUpload. If true, user can upload cell text (data) with file upload.
        /// </summary>
        public bool IsFileUpload;

        /// <summary>
        /// Gets or sets IsOdd.
        /// </summary>
        public bool IsOdd;
    }

    /// <summary>
    /// Grid paging.
    /// </summary>
    public enum GridIsClickEnum
    {
        None = 0,

        /// <summary>
        /// Page up and load data rows from database.
        /// </summary>
        PageUp = 1,

        /// <summary>
        /// Page down and load data rows from database.
        /// </summary>
        PageDown = 2,

        /// <summary>
        /// Page (scroll) left and show new cells in view. No data row load from database.
        /// </summary>
        PageLeft = 3,

        /// <summary>
        /// Page (scroll) right and show new cells in view. No data row load from database.
        /// </summary>
        PageRight = 4,

        /// <summary>
        /// Show data grid in table mode.
        /// </summary>
        ModeTable=7,

        /// <summary>
        /// Show data grid in stack mode.
        /// </summary>
        ModeStack=8,

        /// <summary>
        /// Show data grid in form mode.
        /// </summary>
        ModeForm=9,

        /// <summary>
        /// Download data rows as Excel (*.xlsx) file.
        /// </summary>
        ExcelDownload=10,

        /// <summary>
        /// Upload data rows as Excel (*.xlsx) file.
        /// </summary>
        ExcelUpload=11,

        /// <summary>
        /// Clear filter and reload data rows from database.
        /// </summary>
        Reload = 5,

        /// <summary>
        /// Open data grid config dialog.
        /// </summary>
        Config = 6,
    }

    public class Html : ComponentJson
    {
        public Html(ComponentJson owner)
            : base(owner, nameof(Html))
        {

        }

        public string TextHtml;

        /// <summary>
        /// Gets or sets IsNoSanatize. If true, Angular does not sanatize TextHtml.
        /// </summary>
        public bool IsNoSanatize;
    }

    public class Page : ComponentJson
    {
        /// <summary>
        /// Constructor. Use method PageShowAsync(); to create new page.
        /// </summary>
        public Page(ComponentJson owner)
            : base(owner, nameof(Page))
        {

        }

        /// <summary>
        /// Calle once a lifetime when page is created.
        /// </summary>
        public virtual Task InitAsync()
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// Override this method to implement custom process at the end of the process chain. Called once every request.
        /// </summary>
        protected virtual internal Task ProcessAsync()
        {
            return Task.FromResult(0);
        }
    }

    /// <summary>
    /// Custom component. For example footer component.
    /// See also file: Application.Website/Shared/CustomComponent/custom01.component.ts
    /// </summary>
    public class Custom01 : ComponentJson
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public Custom01(ComponentJson owner) 
            : base(owner, nameof(Custom01))
        {

        }

        public string TextHtml;
    }
}
