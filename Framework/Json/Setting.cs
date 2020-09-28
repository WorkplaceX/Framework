using Framework.Json.Bootstrap;
using Framework.Json.Bulma;
using System;
using System.Threading.Tasks;

namespace Framework.Json
{
    public enum SettingEnum
    {
        None = 0,

        Bootstrap = 1,

        Bulma = 2
    }

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
        public virtual ComponentJson Alert(Page owner, string textHtml, AlertEnum alertEnum)
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
        public override ComponentJson Alert(Page owner, string textHtml, AlertEnum alertEnum)
        {
            return owner.BootstrapAlert(textHtml, alertEnum);
        }
    }

    /// <summary>
    /// Bulma implementation of generic methods.
    /// </summary>
    internal class SettingBulma : Setting
    {
        public override ComponentJson Alert(Page owner, string textHtml, AlertEnum alertEnum)
        {
            return owner.BulmaAlert(textHtml, alertEnum);
        }

        public override PageModal Modal(Page owner)
        {
            return owner.BulmaModal();
        }
    }

    public static class SettingExtension
    {
        private static Setting Setting(ComponentJson componentJson)
        {
            SettingEnum settingEnum;
            if (componentJson is AppJson appJson)
            {
                settingEnum = appJson.SettingEnum;
            }
            else
            {
                settingEnum = componentJson.ComponentOwner<AppJson>().SettingEnum;
            }
            switch (settingEnum)
            {
                case SettingEnum.None:
                    return new Setting();
                case SettingEnum.Bootstrap:
                    return new SettingBootstrap();
                case SettingEnum.Bulma:
                    return new SettingBulma();
                default:
                    throw new Exception("Enum unknown!");
            }
        }

        public static ComponentJson CreateAlert(this Page owner, string textHtml, AlertEnum alertEnum = AlertEnum.Info)
        {
            return Setting(owner).Alert(owner, textHtml, alertEnum);
        }

        public static PageModal CreateModal(this Page owner)
        {
            return Setting(owner).Modal(owner);
        }
    }

    public class PageModal : Page
    {
        public PageModal(ComponentJson owner) : base(owner)
        {

        }

        public Div DivContent;

        public Div DivHeader;

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