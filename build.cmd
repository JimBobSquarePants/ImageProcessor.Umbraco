@ECHO OFF

:: Build the package zip
C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319\msbuild.exe package\package.proj

:: PAUSE
@ECHO ON