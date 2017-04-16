@echo off
cd ..\..\..
echo ### Git Status ###
git status --short
cd ..
git status --short
set /p branchName=BranchName (Default master):
if "%branchName%"=="" (set branchName=master)
set /p m=Commit message: 
cd Submodule
git checkout master & REM Prevent detached branch
git checkout -B "%branchName%"
git add .
git commit -m "%m%"
git pull
git push -u origin "%branchName%"
cd ..
git checkout -B "%branchName%"
git add .
git commit -m "%m%"
git pull
git push -u origin "%branchName%"
echo ### Git Status ###
git status --short
cd Submodule
git status --short
set /p m=Press Enter...