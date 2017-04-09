cd ..\..\..
git status --short
cd ..
git status --short
set /p m=Commit message: 
cd Submodule
git checkout master & REM Prevent detached branch
git add .
git commit -m "%m%"
git push
cd ..
git add .
git commit -m "%m%"
git push
echo ### Status ###
git status
cd Submodule
git status
set /p m=Press Enter...