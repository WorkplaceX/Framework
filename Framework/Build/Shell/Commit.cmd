@echo off
cd ..\..\..
echo ### Git Status ###
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
echo ### Git Status ###
git status --short
cd Submodule
git status --short
set /p m=Press Enter...