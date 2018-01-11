using System;
using System.IO;

namespace Framework.BuildTool
{
    public class CommandDeploy : Command
    {
        public CommandDeploy()
            : base("deploy", "Deploy to Azure git")
        {
            this.AzureGitUrl = ArgumentAdd("azureGitUrl", "Azure Git Url");
        }

        public readonly Argument AzureGitUrl;

        public override void Run()
        {
            string azureGitUrl = AzureGitUrl.Value;
            string folderPublish = UtilFramework.FolderName + "Server/bin/Debug/netcoreapp2.0/publish/";
            //
            UtilBuildTool.DirectoryDelete(folderPublish);
            UtilFramework.Assert(!Directory.Exists(folderPublish), "Delete folder failed!");
            UtilBuildTool.DotNetPublish(UtilFramework.FolderName + "Server/");
            UtilFramework.Assert(Directory.Exists(folderPublish), "Publish failed!");
            UtilBuildTool.Start(folderPublish, "git", "init");
            UtilBuildTool.Start(folderPublish, "git", "remote add azure " + azureGitUrl);
            // UtilBuildTool.Start(folderPublish, "git", "fetch --all --progress");
            // UtilBuildTool.Start(folderPublish, "git", "add .");
            // UtilBuildTool.Start(folderPublish, "git", "commit -m Deploy");
            // UtilBuildTool.Start(folderPublish, "git", "push azure master -f --porcelain"); // Do not write to stderr. See also: https://git-scm.com/docs/git-push/2.10.0
        }
    }
}
