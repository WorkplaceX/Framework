# Framework Update
Checklist to update framework to latest .NET Core and Angular.

## Update Angular
```cmd
npm uninstall -g @angular/cli
npm cache clean --force
npm install -g @angular/cli
ng update
```

## Update Packages
```cmd
npm audit
npm audit fix
```

https://update.angular.io/
