# PR_Manager

[![GitHub release (latest by date)](https://img.shields.io/github/v/release/South2190/PR_Manager)](../../releases)
[![](https://img.shields.io/badge/-changelog-green)](changelog.md)

DMM GAMES版「プリンセスコネクト！Re:Dive」の解像度等をいじれるツール。

> [!Caution]
> このツールはレジストリを書き換えます。みだりに扱ったり、上級者向け設定の設定内容を誤ったりするとコンピューターに思わぬ損害を与える可能性があります。**本ツールを使用したことにより発生した損害については一切の責任を負いかねます。**

> [!Important]
> このツールの機能は(実質)プリコネのバグを利用して実現しているものなので、プリコネ側で修正されてしまうと一切機能しなくなる可能性があります。

## なにができるの
- ウインドウモードとフルスクリーンモード、仮想フルスクリーンモードの切り替え
- ウインドウサイズを縦横別に任意の値(px)に変更
- ゲーム起動中にリアルタイムでウインドウサイズ変更(開発中)
- ゲーム起動時に表示するディスプレイの選択(マルチモニタ環境のみ利用可能)
- ツールからのゲームの起動・終了、ワンクリックでの強制終了

## 使い方(仮)
各設定項目を入力した後、右下の「適用」ボタンを押します。レジストリを書き換えた旨のメッセージが表示されればOKです。

各設定項目の意味に関してはカーソルを合わせた際に説明が表示されますのでそちらを参考にしてみてください。

> [!Note]
> ウインドウサイズがデフォルトに戻されてしまいますので、設定を適用した後ゲームウインドウの端を掴まないように注意してください。

## 動作要件
- OS:Windows10(x64)以降[^1]
- .NET Framework 4.8以降
- DMM GAMES版「プリンセスコネクト！Re:Dive」インストール済
[^1]:Windows8.1以前、x86、ARM64環境では動作確認を行っていません。

## バージョンの更新方法
新しいバージョンがリリースされた場合は、既存の`PR_Manager.exe`を置き換えてください。

## BETA機能について
現在開発中で、試験的に実装している新機能を設定ファイルから有効化することで利用することができます。

### BETA機能の有効化手順

> [!Tip]
> この手順は**バージョン1.0.0.230118以降**で利用することができます。

1. `PR_Manager.exe`を一度実行し、すぐに終了します。`PR_Manager.exe.config`というファイルが同一フォルダ内に生成されます。既に同一ファイルが存在する場合は新たに生成されることはありません。
2. ファイルをテキストエディタで開き、約２0行目以降に以下のような項目があることを確認してください。無い場合はツールのバージョンの確認、もしくはファイルを削除した上でもう一度1.から試してください。

```xml
    <!-- BETA機能設定
         BETA機能を使用するには、各要素のコメントアウトを外してください。 -->
    <!-- ゲーム実行中にリアルタイムで解像度の変更を適用(管理者権限が必要) -->
    <!-- <add key="ApplyInRunning" value="True" /> -->
    <!-- 設定を適用する際のメッセージボックスの表示を無効化する(エラーメッセージを除く) -->
    <!-- <add key="ShowApplyMessage" value="False"/> -->
```

3. 有効化したい項目のコメントアウト(`<!--`と`-->`)を外して保存します。

例：`ゲーム実行中にリアルタイムで解像度の変更を適用(管理者権限が必要)`の項目を有効化する場合

```xml
    <!-- ゲーム実行中にリアルタイムで解像度の変更を適用(管理者権限が必要) -->
    <!-- <add key="ApplyInRunning" value="True" /> -->
```
↓↓↓
```xml
    <!-- ゲーム実行中にリアルタイムで解像度の変更を適用(管理者権限が必要) -->
    <add key="ApplyInRunning" value="True" />
```

> [!Tip]
> ツールを古いバージョンからアップデートした場合、最新バージョンで追加されたBETA機能の有効化設定が`PR_Manager.exe.config`ファイルに存在しないことがあります。その場合は、一度`PR_Manager.exe.config`ファイルを削除した上で、もう一度手順1を実施することで最新バージョンの`PR_Manager.exe.config`ファイルが生成されます。
