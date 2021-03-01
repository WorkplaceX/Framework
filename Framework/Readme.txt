Doc
-Add new website (method InitConfigCli)
-hosts
-ConnectionString
-3rd level domain
-Sql templates for navigate, user, role, permission
-Integrate infrastructure

vNext
-Html snipped
-Class App.Setting to choose Bulma, Bootstrap for Modal and Alert.
-Insert ConfigField without ConfigGrid does not give a duplicate error
-Handle Json property with getter only. Code: "Is attribute [Serialize(SerializeEnum.None)] missing?" [Ok]
-Add new grid configuration
-IsServerSideRendering to display loading alert
-Class View namespace organize [Ok]
-IsUseDeveloperExceptionPage on class ConfigCliEnvironment
-Cli generate command no BuiltIn Id's to CSharp code
-BingMap no global variables
-New sql table FrameworkPage to register all page classes --> use deployDb, pages might belong to different App [Ok]
-Register enums like class Page --> use deployDb [Ok]
-FrameworkConfigField multiple instances of same field [Ok]

ToDo
-https://github.com/Microsoft/sql-server-samples
-Html5 valid
-Navigation like https://rangle.io/
-IoC for singleton and [ThreadStatic]
-Rendering A TemplateRef As A Child Of The Body Element (https://www.bennadel.com/blog/3737-rendering-a-templateref-as-a-child-of-the-body-element-in-angular-9-0-0-rc-5.htm)

ToDo
-pixy.org Images
-unsplash.com Images
-xiconeditor.com Images
-https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs
-BootstrapExtension.BootstrapRow --> DivContainer [Ok]
-ComponentJsonExtension typeof(T).Name replace with null. Child has either name or is identified by type. No more Name [Ok]
-https://w3bits.com/css-dropdown-menu/ for ThemeDefault.
-https://www.youtube.com/watch?v=moBhzSC455o (Build a Responsive Grid CSS Website Layout From Scratch)
-Move AppSelector to ConfigFramework.json --> ConfigWebServer.json WebsiteList [Ok]
-SSL [Ok]
-Load Grid with null query no error [Ok]
-Remove not used grids from AppSession [Ok]
-Show session size on AppJson
-Component Version
-Cli show node version, npm version --> build.sh [Ok]
-Build version UtilFramework.VersionBuild recover after build failure [Ok]
-Search password in logs [Ok]
-App root (<data-app>) two times in index.html
-Use string instead of removing property (Warning! Type not supported by framework.)
-Rename Generate.cs to Database.cs [Ok]
-ComponentJson sealed classes internal constructor. [Ok]
-Nuget package for Framework and FrameworkCli. And npm for Framework/Client
-Rename GridCell.Text to GridCell.T [Ok]
-Visual Code support with workspace and run
-Parse index.html for Snippet and inject with Snippet json component
-Angular template url to html file in Application folder. New component Custom1
-Activator.CreateInstance replace with linq activator performance https://stackoverflow.com/questions/4432026/activator-createinstance-performance-alternative [Ok]

v2.1
-Linq to shared memory queries. (services.AddSingleton) [Ok]

ToDo
-Question: sealed class Grid because of OfType<Grid>(). However Page derived class is possible [Ok]
-ProcessAsync [Ok]
-deploy can not be called twice. (remote azure already exists) [Ok]
-Ci config json. [Ok]
-Delete folder "Framework\Client\dist" on build. [Ok]
-Container component for tree structure.
-Json post [Ok]
-Universal [Ok]
-Scss [Ok]
-Build badge for visualstudio.com https://stackoverflow.com/questions/48785274/adding-vsts-build-status-to-github-page; https://docs.microsoft.com/en-us/vsts/release-notes/2018/mar-05-vsts#share-deployment-status-using-a-badge [Ok]

Research
-https://angular.io/api/common/NgForOf

Submodule
-git submodule deinit Submodule
-git rm Submodule
-git submodule add https://github.com/WorkplaceX/Framework.git