@echo off
setlocal ENABLEDELAYEDEXPANSION

REM ===== �ݒ� =====
set PKG=com.ilcsoft.alcm
set REL_DIR=/data/data/%PKG%/files
set REL_DB=%REL_DIR%/loans.db3
set REL_WAL=%REL_DIR%/loans.db3-wal
set REL_SHM=%REL_DIR%/loans.db3-shm

REM ===== �o�b�`�t�@�C���z���� data\yyyyMMdd_hhmm �t�H���_���쐬 =====
set SCRIPT_DIR=%~dp0
set DATA_DIR=%SCRIPT_DIR%data

REM ���t�Ǝ����i�[���l�߁j���擾
for /f "tokens=1-4 delims=/ " %%a in ("%date%") do set YYYY=%%a& set MM=%%b& set DD=%%c
set HH=%time:~0,2%
if "%HH:~0,1%"==" " set HH=0%HH:~1,1%
set MI=%time:~3,2%

set TS=%YYYY%%MM%%DD%_%HH%%MI%
set DEST_DIR=%DATA_DIR%\%TS%

if not exist "%DATA_DIR%" mkdir "%DATA_DIR%"
if not exist "%DEST_DIR%" mkdir "%DEST_DIR%"

echo �o�͐�: "%DEST_DIR%"

echo === ADB�ڑ��m�F ===
adb get-state 1>nul 2>nul || (echo �[����������܂��� & exit /b 1)

echo === run-as �e�X�g ===
adb shell run-as %PKG% ls %REL_DIR% 1>nul 2>nul || goto FALLBACK

echo === exec-out ��DB�𒼐ڃ_���v ===
adb exec-out run-as %PKG% cat %REL_DB% > "%DEST_DIR%\loans.db3" || (echo DB�_���v���s & exit /b 2)

echo �ǉ��t�@�C��(-wal/-shm)�̎擾�i���݂���Εۑ��j
adb exec-out run-as %PKG% cat %REL_WAL% > "%DEST_DIR%\loans.db3-wal" 2>nul
adb exec-out run-as %PKG% cat %REL_SHM% > "%DEST_DIR%\loans.db3-shm" 2>nul

echo ����: "%DEST_DIR%"
exit /b 0

:FALLBACK
echo === run-as ���ʂ�Ȃ����߃t�H�[���o�b�N�iDownload�o�R�j ===
REM ������Download�փR�s�[���Ă��� pull�i-wal/-shm �����s�j
adb shell run-as %PKG% cp %REL_DB% /sdcard/Download/loans.db3 || (echo Download�R�s�[���s & exit /b 3)
adb shell run-as %PKG% cp %REL_WAL% /sdcard/Download/loans.db3-wal 2>nul
adb shell run-as %PKG% cp %REL_SHM% /sdcard/Download/loans.db3-shm 2>nul

adb pull /sdcard/Download/loans.db3 "%DEST_DIR%\loans.db3" || (echo pull���s & exit /b 4)
adb pull /sdcard/Download/loans.db3-wal "%DEST_DIR%\loans.db3-wal" 2>nul
adb pull /sdcard/Download/loans.db3-shm "%DEST_DIR%\loans.db3-shm" 2>nul

echo ����: "%DEST_DIR%"
exit /b 0
