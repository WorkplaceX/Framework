set /p m=Commit message: 
git add .
git commit -m "%m%"
git push
cd ..\..\..\..\
git add .
git commit -m "%m%"
git push
set /p m=