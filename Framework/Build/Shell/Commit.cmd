cd ..\..\..
git status --short
cd ..
git status --short
set /p m=Commit message: 
cd Submodule
git add .
git commit -m "%m%"
git push
cd ..
git add .
git commit -m "%m%"
git push
set /p m=Press enter to continue