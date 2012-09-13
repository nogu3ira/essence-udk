@echo off
chcp 866
if "%~1"=="/?" if "%~2"=="" (
	echo   ��墨஢���� � ���⠭������� �襭�� �஥�� uoQServer
	echo   �ᯮ�짮�����: %~n0 [X ^| ���_䠩��]
	echo   �
	echo   ����� ��� ��ࠬ��஢ �ਢ���� � ��娢�஢���� �஥��, 
	echo   ����祭�� ��娬� ����頥��� � ��७� �襭�� � �����
	echo   ����騩 ���: "YYYY.MM.DD-HH.MM.SS-DIRNAME.tar.bz2", 
	echo   ᫥����⥫쭮 ����� � ��ࠬ��஬ �ਢ���� � ���⠭�������
	echo   �襭�� �஥�� �� ��娢� � ������ ���� ��������� 
	echo   ��ଥ�஬ ^<���_䠩��^> ���� ��।��塞� ��⮬���᪨ �� 
	echo   楫��᫥����� ��ࠬ��� ^<X^>, �ࠪ�ਧ��饬� ����� 
	echo   ��娢� � ����. �� ���� "%~n0 0" ���⠭���� �襭�� ��
	echo   ��᫥����� ��娢�, � "%~n0 1" �� �।��᫥����� � �.�.
	echo   �
	exit /b 0
)
setlocal ENABLEEXTENSIONS ENABLEDELAYEDEXPANSION
color F1	
cls
call :folder "%~dp0"

if "%~1"=="" (		
	title "!$folder!" - ��娢�஢���� �஥��...
	if not exist "%~dp0_temp\" (mkdir "%~dp0_temp\"&attrib +H "%~dp0_temp\" /S /D)
	set y=%DATE:~6,4%
	set n=%DATE:~3,2%
	set d=%DATE:~0,2%
	set h=%TIME:~0,2%
	set m=%TIME:~3,2%
	set s=%TIME:~6,2%
	set file=!y!.!n!.!d!-!h!.!m!.!s!-!$folder!

	if exist "%~dp0_temp\!file!.lst" del "%~dp0_temp\!file!.lst"
	echo _ReSharper.*>>"%~dp0_temp\!file!.lst"
	echo _temp>>"%~dp0_temp\!file!.lst"
	echo *\obj>>"%~dp0_temp\!file!.lst"
	echo *\bin>>"%~dp0_temp\!file!.lst"
	echo *.tar.bz2>>"%~dp0_temp\!file!.lst"
																																	  ::"Compressing.*\."
	call "%~dp0Deploying\7za.exe" a -ttar -ssw -r0 -x@"%~dp0_temp\!file!.lst" "%~dp0_temp\!file!.tar" "%~dp0*" | findstr /P /I /V /R /C:"Compressing"
	call "%~dp0Deploying\7za.exe" a -tbzip2 -mx7 "%~dp0!file!.tar.bz2" "%~dp0_temp\!file!.tar"

	del "%~dp0_temp\!file!.lst"
	del "%~dp0_temp\!file!.tar"
	rmdir /S /Q "%~dp0_temp\"
	echo �
	echo ______________________________________________________________________________
	echo ^>^> "!file!"
	echo                                             ��������������������������ͻ
	echo �������������������������������������������Ķ ��娢�஢���� �����襭�� �������
	echo                                             ��������������������������ͼ
	pause
	exit 0
) else (
	if not "!$folder!"=="_temp" (
		if not exist "%~dp0_temp\" (mkdir "%~dp0_temp"&attrib +H "%~dp0_temp" /S /D)
		copy /Y /B "%~dp0%~nx0" "%~dp0_temp\%~nx0"
		call "%~dp0_temp\%~nx0" %1
		goto :EOF
	)
	call :folder "%~dp0" 1
	title "!$folder!" - ���⠭������� �஥��...	
	set file=%~1
)
call :isnum "%file%"
REM		echo "%file%"-%$isnum%
if "%$isnum%"=="1" (
	if exist "%~dp0%~n0.lst" del "%~dp0%~n0.lst"
	dir "%~dp0..\" /B /O:N /A:A | findstr /P /I /R /C:".*\.tar\.bz2">>"%~dp0%~n0.lst"
REM		for /f "usebackq delims=" %%A in ("%~dp0%~n0.lst") do @echo %%A
REM		echo __________________________________________________

	set n=0
	for /F "usebackq delims=" %%i in ("%~dp0%~n0.lst") do (set /a n+=1)
	set /A n-=!file!
	if /I !n! LEQ 0 set n=1
	for /f "usebackq  delims=" %%i in (`find /n /v "" "%~dp0%~n0.lst" ^| find "[!n!]"`) do (set file=%%i)	
	call :strlen "[!n!]"	
	for /F %%i in ("^!file:~!$strlen!^!") do set file=%%i
)

if "!argY!"=="" (
	:Prompt1
	set /P prompt=�� ���⢥ত��� ���⠭������� "!file!"? [Y:N]:
	if "!prompt!"=="n" (exit /b) else (if "!prompt!"=="N" exit /b) 
	if not "!prompt!"=="y" if not "!prompt!"=="Y" goto :Prompt1
)
title "!$folder!" - ���⠭������� �஥�� �� "%file%"...

copy /Y /B "%~dp0..\Deploying\7za.exe" "%~dp07za.exe" 

if exist "%~dp0%file:~0,-4%" del "%~dp0%file:~0,-4%"
call "%~dp07za.exe" x -tbzip2 -y "%~dp0..\%file%" -o"%~dp0"

REM ��������
if exist "%~dp0%~n0.lst" del "%~dp0%~n0.lst"
dir "%~dp0..\" /B /A:D | findstr /P /I /V /R /C:"_temp">>"%~dp0%~n0.lst"
for /F "usebackq delims=" %%i in ("%~dp0%~n0.lst") do (rmdir /S /Q "%~dp0..\%%i\")

if exist "%~dp0%~n0.lst" del "%~dp0%~n0.lst"
dir "%~dp0..\" /B /A:A | findstr /P /I /V /R /C:".*\.tar\.bz2">>"%~dp0%~n0.lst"
for /F "usebackq delims=" %%i in ("%~dp0%~n0.lst") do (del /F /Q "%~dp0..\%%i")

call "%~dp07za.exe" x -ttar -y "%~dp0%file:~0,-4%" -o"%~dp0..\"| findstr /P /I /V /R /C:"Extracting"
if exist "%~dp0%file:~0,-4%" del "%~dp0%file:~0,-4%"

for /d /r "%~dp0..\" %%d in (.svn) do @if exist "%%d" attrib +H "%%d"

if exist "%~dp07za.exe" del "%~dp07za.exe"
if exist "%~dp0%~n0.lst" del "%~dp0%~n0.lst"
rem	if exist "%~dp0" rmdir /S /Q "%~dp0"

echo �
echo ______________________________________________________________________________
echo ^>^> !file!
echo                                             ��������������������������ͻ
echo �������������������������������������������Ķ ���⠭������� �����襭�� �������
echo                                             ��������������������������ͼ
pause
exit 0

:folder
	set $folder=%~dp1
	if not ""=="%~2" if not "0"=="%~2" (
		set folderbuf=%~dp1
		for /L %%f in (1,1,%~2) do (
			call :folder "!folderbuf!"
			call :strlen "!$folder!"
			for /L %%i in (0,1,!$strlen!) do set folderbuf=!folderbuf:~0,-1!
		)
		set $folder=!folderbuf!
	)	
	for /D %%a in ("!$folder:~0,-1!.ext") do set $folder=%%~na
	goto :EOF
	
:strlen
	set $strlen=0&set $strbuf=%~1
	if ""=="%~1" goto :EOF
	:StrLenLoop
	set /A $strlen+=1
	call set $strchr=%%$strbuf:~!$strlen!%%
	if ""=="!$strchr!" goto :EOF
	goto :StrLenLoop
	
:isnum
	set $isnum=1&set $strbuf=%~1
	call :strlen "!$strbuf!"
	if /I !$strlen! LEQ 0 goto :IsNumEndLoop
	set /A $strlen-=1
	for /L %%i in (0,1,!$strlen!) do (
		set $strchr=!$strbuf:~%%i,1!
		if not "!$strchr!"=="0" if not "!$strchr!"=="1" if not "!$strchr!"=="2" if not "!$strchr!"=="3" if not "!$strchr!"=="4"^
		if not "!$strchr!"=="5" if not "!$strchr!"=="6" if not "!$strchr!"=="7" if not "!$strchr!"=="8" if not "!$strchr!"=="9"^
			goto :IsNumEndLoop
	)
	goto :EOF
	:IsNumEndLoop
	set $isnum=
	goto :EOF
		
	