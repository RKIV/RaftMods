git add .
for /F "tokens=2" %%i in ('date /t') do set mydate=%%i
set mytime=%time%
echo Current time is %mydate%:%mytime%
pause
git commit -m "%mydate%:%mytime%"
git push
pause