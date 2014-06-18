# UsiUtil

USI関連のユーティリティ

## USIClient/USIConnect

Soketを使って別PCでUSIエンジンを動かします。
エンジン同士の対局で２台のPCでの通信対局に対応していないGUIでも、
２台のPCをつかって対局できます。

 PC A                     | PC B
USI対応GUI <-> USIClient <-> USIConnect<-> エンジン

ソケットの練習用に作成

### 使い方

*USIConnectを実行する。  　　
*開始ボタンを押す。  
*USIClient.exe.configの内容を適当に書き換えるか、コマンドラインオプションを以下のようにして実行する。  
'UsiClient -h localhost -p 53556 gpsfish\gpsfish.exe'  



