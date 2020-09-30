using Framework.Json.Bootstrap;
using Framework.Json.Bulma;
using System;
using System.Threading.Tasks;

namespace Framework.Json
{
    public enum AlertEnum
    {
        None = 0,

        Info = 1,

        Success = 2,

        Warning = 3,

        Error = 4
    }

    /// <summary>
    /// Default implementation of generic methods.
    /// </summary>
    internal class Setting
    {
        public virtual Html Alert(Page owner, string textHtml, AlertEnum alertEnum)
        {
            var result = new Html(owner) { TextHtml = textHtml };
            result.ComponentMove(0); // Move to top
            return result;
        }

        public virtual PageModal Modal(Page owner)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Bootstrap implementation of generic methods.
    /// </summary>
    internal class SettingBootstrap : Setting
    {
        public override Html Alert(Page owner, string textHtml, AlertEnum alertEnum)
        {
            return owner.BootstrapAlert(textHtml, alertEnum);
        }
    }

    public static class SettingExtension
    {
        private static Setting Setting(ComponentJson componentJson)
        {
            CssFrameworkEnum settingEnum;
            if (componentJson is AppJson appJson)
            {
                settingEnum = appJson.CssFrameworkEnum;
            }
            else
            {
                settingEnum = componentJson.ComponentOwner<AppJson>().CssFrameworkEnum;
            }
            switch (settingEnum)
            {
                case CssFrameworkEnum.None:
                    return new Setting();
                case CssFrameworkEnum.Bootstrap:
                    return new SettingBootstrap();
                default:
                    throw new Exception("Enum unknown!");
            }
        }

        public static Html CreateAlert(this Page owner, string textHtml, AlertEnum alertEnum = AlertEnum.Info)
        {
            return Setting(owner).Alert(owner, textHtml, alertEnum);
        }

        public static PageModal CreateModal(this Page owner)
        {
            return Setting(owner).Modal(owner);
        }
    }

    public class Alert : Html
    {
        public Alert(ComponentJson owner, string textHtml, AlertEnum alertEnum, int? index = 0) 
            : base(owner)
        {
            var settingEnum = this.ComponentOwner<AppJson>().CssFrameworkEnum;
            switch (settingEnum)
            {
                case CssFrameworkEnum.None:
                    break;
                case CssFrameworkEnum.Bootstrap:
                    break;
                case CssFrameworkEnum.Bulma:
                    {
                        // See also: https://bulma.io/documentation/elements/notification/
                        string textHtmlTemplate = "<div class='{{CssClass}}'><button class='delete'></button>{{TextHtml}}</div>";
                        string cssClass = null;
                        switch (alertEnum)
                        {
                            case AlertEnum.Info:
                                cssClass = "notification is-info";
                                break;
                            case AlertEnum.Success:
                                cssClass = "notification is-success";
                                break;
                            case AlertEnum.Warning:
                                cssClass = "notification is-warning";
                                break;
                            case AlertEnum.Error:
                                cssClass = "notification is-danger";
                                break;
                            default:
                                break;
                        }
                        textHtmlTemplate = textHtmlTemplate.Replace("{{CssClass}}", cssClass).Replace("{{TextHtml}}", textHtml);
                        TextHtml = textHtmlTemplate;
                        IsNoSanatize = true;
                        if (index != null)
                        {
                            this.ComponentMove(index.Value);
                        }
                    }
                    break;
                default:
                    throw new Exception("Enum unknown!");
            }
        }

        protected internal override Task ProcessAsync()
        {
            if (ButtonIsClick())
            {
                this.ComponentRemove();
            }
            return base.ProcessAsync();
        }
    }

    public class PageModal : Page
    {
        public PageModal(ComponentJson owner) : base(owner)
        {
            var settingEnum = this.ComponentOwner<AppJson>().CssFrameworkEnum;
            switch (settingEnum)
            {
                case CssFrameworkEnum.None:
                    break;
                case CssFrameworkEnum.Bootstrap:
                    break;
                case CssFrameworkEnum.Bulma:
                    {
                        // See also: https://bulma.io/documentation/elements/notification/
                        var divModal = new DivContainer(this) { CssClass = "modal is-active" };
                        new Div(divModal) { CssClass = "modal-background" };
                        var divCard = new Div(divModal) { CssClass = "modal-card" };
                        var divHeaderLocal = new DivContainer(divCard) { CssClass = "modal-card-head" };
                        DivHeader = new Div(divHeaderLocal) { CssClass = "modal-card-title" };
                        ButtonClose = new Button(new Div(divHeaderLocal)) { CssClass = "delete" };
                        DivBody = new Div(divCard) { CssClass = "modal-card-body" };
                        DivFooter = new Div(divCard) { CssClass = "modal-card-foot" };
                        // Title
                        {
                            // new Html(result.DivHeader) { TextHtml = "<p>Title</p>" };
                        }
                        // Two buttons in Html
                        {
                            // new Html(result.DivFooter) { TextHtml = "<button class='button is-success'>Save changes</button><button class='button'>Cancel</button>", IsNoSanatize = true };
                        }
                        // Two individual buttons
                        {
                            // new Button(result.DivFooter) { CssClass = "button is-success", TextHtml = "Ok" };
                            // new Html(result.DivFooter) { TextHtml = "&nbsp" };
                            // new Button(result.DivFooter) { CssClass = "button", TextHtml = "Cancel" };
                        }
                    }
                    break;
                default:
                    throw new Exception("Enum unknown!");
            }
        }

        public Div DivHeader;
        
        public Div DivBody;

        public Div DivFooter;

        public Button ButtonClose;

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