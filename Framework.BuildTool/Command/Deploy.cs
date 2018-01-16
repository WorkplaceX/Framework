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
            UtilBuildTool.Start(folderPublish, "git", "config user.email \"deploy@deploy.deploy\""); // Prevent: Error "Please tell me who you are". See also: http://www.thecreativedev.com/solution-github-please-tell-me-who-you-are-error/
            UtilBuildTool.Start(folderPublish, "git", "config user.name \"Bhumi Shah\"");
            UtilBuildTool.Start(folderPublish, "git", "remote add azure " + azureGitUrl);
            UtilBuildTool.Start(folderPublish, "git", "fetch --all -q"); // -q do not write to stderr.
            UtilBuildTool.Start(folderPublish, "git", "add .", isRedirectStdErr: true);
            UtilBuildTool.Start(folderPublish, "git", "commit -m Deploy");
            UtilBuildTool.Start(folderPublish, "git", "push azure master -f", isRedirectStdErr: true); // Do not write to stderr. Can be tested with "dotnet run -- deploy [AzureGitUrl] 2>Error.txt"
        }
    }
}
