# Framework Update
Checklist to update framework to latest .NET Core, Angular, Bootstrap and Bulma versions.

## Update

* Update Nuget packages
* Update Angular and website

* Framework.Angular/application/
```cmd
ng update
ng update --all
```

* Framework.Cli/Template/Application.Website/LayoutBulma/
* Framework.Cli/Template/Application.Website/LayoutDefault/
* Framework.Cli/Template/Application.Website/LayoutEmpty/
```cmd
npm audit
npm audit fix
```

## Update Angular
```cmd
npm uninstall -g @angular/cli
npm cache clean --force
npm install -g @angular/cli
ng update
```

https://update.angular.io/
