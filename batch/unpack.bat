set winrar="C:\Program Files\WinRAR\WinRAR.exe"
%winrar% x -ai -ibck %1 tmp\ && exit

:error
exit 1