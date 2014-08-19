Call ..\tools\nuget.exe restore ..\src\ImageProcessor.Umbraco.sln
:: Build the package zip
"%programfiles(x86)%\MSBuild\12.0\Bin\MsBuild.exe" package.proj

@IF %ERRORLEVEL% NEQ 0 GOTO err
@EXIT /B 0
:err
@PAUSE
@EXIT /B 1