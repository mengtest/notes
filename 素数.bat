@echo off

echo ���Ǹ��ж�һ�����Ƿ�Ϊ�����Ľű�
:start
echo.
set /p x=�����ж�����:
set /a n=x-1
set i=1
if %x% == 1 (
	echo ��������
    goto start
    exit
)
if %x% == 2 (
	echo ������
    goto start
    exit
)
echo %x%|findstr /be "[0-9]*" >nul && goto isPrime || goto isU
exit

:isPrime
set /a i+=1
set /a is=%x%%%%i%
set /a z=%x%/%i%
if  %is% equ 0 (
    echo =%i%*%z%
    echo ��������
    goto start
)
if %i% equ %n% (
    echo ������ 
    goto start
)
goto isPrime
exit

:isU
if /i %x% == Xu1Fan (  ::/i ���Դ�Сд
    echo  Xu1Fan �㻹����Ŷ! rui sou sou xi dou xi la   sou la xi xi xi xi la xi la sou
    goto start
)else ( ::elseҪ��)���� ( ��else �пո�
    echo ������� 
    goto start
) 
exit
