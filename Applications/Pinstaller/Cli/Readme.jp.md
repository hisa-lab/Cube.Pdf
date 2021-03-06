CubePinstaller
====

Copyright © 2010 CubeSoft, Inc.  
support@cube-soft.jp  
https://www.cube-soft.jp/

## はじめに

CubePinstaller は、プリンタのインストールおよびアンインストールを実行するための
コマンドライン型アプリケーションです。CubePinstaller を使用するには、
.NET Framework 3.5 以降がインストールされている必要があります。
.NET Framework は、以下の URL からダウンロードして下さい。

* Download .NET Framework  
  https://dotnet.microsoft.com/download/dotnet-framework

CubePinstaller は、**C:\ProgramData\CubeSoft\CubePDF\Log** フォルダに実行ログを
出力しています。問題が発生した時は、これらのログを添付して support@cube-soft.jp
までご連絡お願いします。

## 使用方法

```
CubePinstaller.exe JSON /Command COMMAND [OPTIONS] 
```

CubePinstaller の必須パラメータは **JSON** および **COMMAND** の 2 種類です。
**JSON** には、インストールまたはアンインストールするプリンタ構成を記載した
JSON 形式のファイルへのパスを記載します。
**COMMAND** には、下記の 3 種類の中から一つを指定します。

* **Install**  
  指定された構成でプリンタ等をインストールします。
  既にインストールされている項目は、処理をスキップします。
* **Uninstall**  
  指定された構成でプリンタ等をアンインストールします。
* **Reinstall**  
  指定された構成でプリンタ等をいったんアンインストールした後、
  再度インストールを実行します。

**OPTIONS** に指定可能なオプションは下記の通りです。

* **/Resource DIRECTORY**  
  JSON ファイルに記載される、インストールに必要な各種ファイルが存在する
  ディレクトリへのパスを指定します。
* **/Relative**  
  コマンドライン上で指定されたパスを CubePinstaller.exe が存在する
  ディレクトリからの相対パスとして認識します。
* **/Force**  
  JSON ファイルに記載されたプリンタドライバやポートモニタに依存する全ての要素を
  強制的にアンインストールします。このオプションを指定しない場合、対象となる
  プリンタドライバ等が他のプリンタに使用されているなどの理由で、アンインストールに
  失敗する事があります。
* **/Retry COUNT**  
  プリンタ等のインストールまたはアンインストールに失敗した時に再試行する回数を
  指定します。
* **/Timeout SECOND**  
  プリンタ等のインストールまたはアンインストール実行時のタイムアウト時間の初期値を
  秒単位で指定します。実際のタイムアウト時間は、実行に失敗する度に等倍されます。
  例えば 30 を指定した場合、実際のタイムアウト時間は 30 秒、60 秒、90 秒、...と
  増加していきます。

CubePinstaller の実行コマンド例は下記の通りです。

```
CubePinstaller.exe CubePrinter.json
    /Command Reinstall
    /Force
    /Relative
    /Resource Printers
    /Retry 6
    /Timeout 30
```

## JSON 仕様

CubePinstaller に指定する構成用 JSON ファイルの仕様は下記の通りです。

```
{
    "Printers": [ ... ],
    "PrinterDrivers": [ ... ],
    "Ports": [ ... ],
    "PortMonitors": [ ... ]
}
```

構成要素は **Printers**, **PrinterDrivers**, **Ports**, **PortMonitors** の
4 種類であり、いずれも配列形式で複数の要素を指定する事ができます。
また、既にインストールされている要素を利用する等の理由で新たにインストールする
ものが存在しない場合、該当項目を省略する事も可能です。

### PortMonitors

**PortMonitors** には、インストールまたはアンインストールするポートモニタを
指定します。指定可能な項目は、**Name**, **FileName**, **Config** の 3 種類で
いずれも string です。

**Name** はインストールまたはアンインストールするポートモニタの名前で必須項目です。
**FileName** および **Config** はインストールに必要なモジュール名を表します。
これらの項目はアンインストール時には省略する事ができます。
また、指定されたモジュールは **/Resource** オプションで指定されたディレクトリに
存在するものとします。

### Ports

**Ports** には、インストールまたはアンインストールするポートを指定します。
指定可能な項目は下記の通りです。

* **Name** (string)  
  インストールまたはアンインストールするポートの名前を表します。
* **MonitorName** (string)  
  ポートが利用するポートモニタの名前を表します。
  ここで指定されるポートモニタは、既にインストールされているか、または、
  同じ構成ファイルに記述されている必要があります。
* **Application** (string)  
  ポートが実行するアプリケーションのパスを表します。
* **Arguments** (string)  
  ポートがアプリケーションを実行する際に指定する引数を表します。
* **Temp** (string)  
  一時ファイル等を保存するディレクトリのパスを表します。
  ログイン中のユーザおよび SYSTEM アカウントが書き込み可能なディレクトリを
  指定して下さい。
* **WaitForExit** (bool)  
  アプリケーションが終了するまで、次の印刷ジョブの処理を待機するかどうかを
  表します。false の場合、複数のプロセスが同時に実行される可能性があります。
* **RunAsUser** (bool)  
  アプリケーションをログオン中のユーザで実行するかどうかを表します。
  false の場合、SYSTEM アカウントで実行されます。尚、**Proxy** の項目が省略
  されている場合、この項目に関わらず SYSTEM アカウントで実行されます。
* **Proxy** (string)  
  SYSTEM アカウントからログオン中のユーザに切り替えるためのプログラムのパスを
  指定します。該当機能を自ら実装する等の場合を除き、同梱する CubeProxy.exe の
  パスを指定して下さい。

### PrinterDrivers

**PrinterDrivers** には、インストールまたはアンインストールするプリンタドライバを
指定します。指定可能な項目は、**Name**, **MonitorName**, **FileName**, **Config**,
**Data**, **Help**, **Dependencies**, **Repository** の 8 種類で、
**Dependencies** は string の配列、それ以外は string です。

**Name** はインストールまたはアンインストールするプリンタドライバの名前、
**MonitorName** はプリンタドライバが利用するポートモニタの名前を表し、
いずれも必須項目です。また、**MonitorName** で指定されるポートモニタは、
既にインストールされているか、または、同じ構成ファイルに記述されている必要が
あります。

**Repository** は、各種モジュールを DriverStore ディレクトリから検索する際に
使用します。例えば、64bit 環境において ntprint を指定した場合、
DriverStore/FileRepository/ntprint.ing_amd64_xxxxxxxxxxxxxxx/amd64
からの取得を試みます。

**FileName**, **Config**, **Data**, **Help**, **Dependencies** はインストールに
必要なモジュール名を表します。これらの項目はアンインストール時には省略する事が
できます。また、指定されたモジュールは **/Resource** オプションで指定された
ディレクトリ、または DriverStore ディレクトリ下に存在するものとします。

尚、**Data** で指定するファイルには PCFileName と言う項目が存在します。
この項目と実際のファイル名が異なる場合、プリンタドライバのインストールに失敗する
現象が確認されています。

### Printers

**Printers** には、インストールまたはアンインストールするプリンタを指定します。
指定可能な項目は、**Name**, **ShareName**, **DriverName**, **PortName** の
4 種類でいずれも string です。また、これらの項目はいずれも必須項目です。

**Name** は、インストールまたはアンインストールするプリンタ名、**ShareName** は
このプリンタを LAN 等のネットワーク上で共有する時の名前を表します。
**DriverName** および **PortName** は、プリンタが利用するプリンタドライバおよび
ポートの名前を表します。これらは、既にインストールされているか、または、同じ構成
ファイルに記述されている必要があります。

### Examples

CubePinstaller に指定する JSON ファイルの構成例は下記の通りです。

```
{
    "Printers" : [{
        "Name"         : "CubePDF",
        "ShareName"    : "CubePDF",
        "DriverName"   : "CubePDF",
        "PortName"     : "CubePDF:"
    }],
    "PrinterDrivers" : [{
        "Name"         : "CubePDF",
        "MonitorName"  : "CubeMon",
        "Data"         : "cubepdf.ppd",
        "Repository"   : "ntprint",
        "FileName"     : "pscript5.dll",
        "Config"       : "ps5ui.dll",
        "Help"         : "pscript.hlp",
        "Dependencies" : [ "pscript.ntf", "pscrptfe.ntf", "ps_schm.gdl" ]
    }],
    "Ports" : [{
        "Name"         : "CubePDF:",
        "MonitorName"  : "CubeMon",
        "Application"  : "C:\\Program Files\\CubePDF\\CubePdf.exe",
        "Proxy"        : "C:\\Program Files\\CubePDF\\CubeProxy.exe",
        "Arguments"    : "",
        "Temp"         : "C:\\ProgramData\\CubeSoft\\CubePDF",
        "WaitForExit"  : false,
        "RunAsUser"    : true
    }],
    "PortMonitors" : [{
        "Name"         : "CubeMon",
        "FileName"     : "cubemon.dll",
        "Config"       : "cubemonui.dll"
    }]
}
```
