@echo off
setlocal ENABLEDELAYEDEXPANSION

REM ===== 設定 =====
set PKG=com.ilcsoft.alcm
set REL_DIR=/data/data/%PKG%/files
set REL_DB=%REL_DIR%/loans.db3
set REL_WAL=%REL_DIR%/loans.db3-wal
set REL_SHM=%REL_DIR%/loans.db3-shm

REM ===== バッチファイル配下の data\yyyyMMdd_hhmm フォルダを作成 =====
set SCRIPT_DIR=%~dp0
set DATA_DIR=%SCRIPT_DIR%data

REM 日付と時刻（ゼロ詰め）を取得
for /f "tokens=1-4 delims=/ " %%a in ("%date%") do set YYYY=%%a& set MM=%%b& set DD=%%c
set HH=%time:~0,2%
if "%HH:~0,1%"==" " set HH=0%HH:~1,1%
set MI=%time:~3,2%

set TS=%YYYY%%MM%%DD%_%HH%%MI%
set DEST_DIR=%DATA_DIR%\%TS%

if not exist "%DATA_DIR%" mkdir "%DATA_DIR%"
if not exist "%DEST_DIR%" mkdir "%DEST_DIR%"

echo 出力先: "%DEST_DIR%"

echo === ADB接続確認 ===
adb get-state 1>nul 2>nul || (echo 端末が見つかりません & exit /b 1)

echo === run-as テスト ===
adb shell run-as %PKG% ls %REL_DIR% 1>nul 2>nul || goto FALLBACK

echo === exec-out でDBを直接ダンプ ===
adb exec-out run-as %PKG% cat %REL_DB% > "%DEST_DIR%\loans.db3" || (echo DBダンプ失敗 & exit /b 2)

echo 追加ファイル(-wal/-shm)の取得（存在すれば保存）
adb exec-out run-as %PKG% cat %REL_WAL% > "%DEST_DIR%\loans.db3-wal" 2>nul
adb exec-out run-as %PKG% cat %REL_SHM% > "%DEST_DIR%\loans.db3-shm" 2>nul

echo 完了: "%DEST_DIR%"
exit /b 0

:FALLBACK
echo === run-as が通らないためフォールバック（Download経由） ===
REM 内部→Downloadへコピーしてから pull（-wal/-shm も試行）
adb shell run-as %PKG% cp %REL_DB% /sdcard/Download/loans.db3 || (echo Downloadコピー失敗 & exit /b 3)
adb shell run-as %PKG% cp %REL_WAL% /sdcard/Download/loans.db3-wal 2>nul
adb shell run-as %PKG% cp %REL_SHM% /sdcard/Download/loans.db3-shm 2>nul

adb pull /sdcard/Download/loans.db3 "%DEST_DIR%\loans.db3" || (echo pull失敗 & exit /b 4)
adb pull /sdcard/Download/loans.db3-wal "%DEST_DIR%\loans.db3-wal" 2>nul
adb pull /sdcard/Download/loans.db3-shm "%DEST_DIR%\loans.db3-shm" 2>nul

echo 完了: "%DEST_DIR%"
exit /b 0
