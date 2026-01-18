@echo off
setlocal enabledelayedexpansion

REM Output file
set MANIFEST=manifest.xml

REM Start manifest.xml
echo ^<?xml version="1.0" encoding="utf-8"?^> > "%MANIFEST%"
echo ^<releases^> >> "%MANIFEST%"
echo ^  ^<release version="1.0.0" name="Initial Release"^> >> "%MANIFEST%"
echo ^    ^<files^> >> "%MANIFEST%"

REM Loop through all files in current directory (not folders)
for %%F in (*.*) do (
    REM Exclude manifest.xml itself
    if /I not "%%F"=="%MANIFEST%" (
        REM Exclude .git, .vs, source folders and hidden/system files
        if /I not "%%F"==".gitattributes" (
            attrib "%%F" | find /i "H" >nul
            if errorlevel 1 (
                REM Get MD5 hash
                for /f "tokens=1,* delims= " %%A in ('certutil -hashfile "%%F" MD5 ^| find /i "MD5"') do (
                    set HASH=%%B
                )
                echo ^      ^<file filename="%%F" hash="!HASH!" /^> >> "%MANIFEST%"
            )
        )
    )
)

REM Loop through all folders except excluded ones
for /d %%D in (*) do (
    if /I not "%%D"=="source" if /I not "%%D"==".git" if /I not "%%D"==".vs" (
        REM Recursively add files in allowed folders
        for /r "%%D" %%F in (*.*) do (
            REM Exclude manifest.xml itself
            if /I not "%%~nxF"=="%MANIFEST%" (
                attrib "%%F" | find /i "H" >nul
                if errorlevel 1 (
                    REM Get MD5 hash
                    for /f "tokens=1,* delims= " %%A in ('certutil -hashfile "%%F" MD5 ^| find /i "MD5"') do (
                        set HASH=%%B
                    )
                    REM Use relative path
                    set RELPATH=%%F
                    set RELPATH=!RELPATH:%CD%\=!
                    set RELPATH=!RELPATH:~1!
                    echo ^      ^<file filename="!RELPATH!" hash="!HASH!" /^> >> "%MANIFEST%"
                )
            )
        )
    )
)

echo ^    ^</files^> >> "%MANIFEST%"
echo ^  ^</release^> >> "%MANIFEST%"
echo ^</releases^> >> "%MANIFEST%"

echo Manifest generated: %MANIFEST%
pause