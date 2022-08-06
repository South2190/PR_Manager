# PR_Manager

[![GitHub release (latest by date)](https://img.shields.io/github/v/release/South2190/PR_Manager)](https://github.com/South2190/PR_Manager/releases)
[![](https://img.shields.io/badge/-changelog-green)](https://github.com/South2190/PR_Manager/blob/main/changelog.md)

DMM GAMES版「プリンセスコネクト！Re:Dive」の解像度等をいじれるツール。

## ダウンロード
- [Releases](https://github.com/South2190/PR_Manager/releases)

## 注意事項
- ***本ツールはベータ版です。***
- 本ツールの使用は自己責任でお願いします。
- ウインドウサイズがデフォルトに戻されてしまいますので、設定を適用した後ウインドウの端を掴まないように注意してください。
- このツールの機能は(実質)プリコネのバグを利用して実現しているものなので、プリコネ側で修正されてしまうと一気に使い物にならなくなる可能性があります。(ウマ娘などは一部対策済の模様)

## なにができるの
- ウインドウモードとフルスクリーンモードの切り替え
- ウインドウサイズを縦横別に任意の値(px)に変更
- ゲーム起動中にリアルタイムでウインドウサイズ変更(テスト段階)
- ゲーム起動時に表示するディスプレイの選択(マルチモニタ環境のみ利用可能)
- ツールからのゲームの起動・終了、ワンクリックでの強制終了

## 使用方法
各設定項目を入力した後、右下の「適用」ボタンを押します。レジストリを書き換えた旨のメッセージが表示されればOKです。

各設定項目の意味に関してはカーソルを合わせた際に説明が表示されますのでそちらを参考にしてみてください。(ただWPFの仕様なのか知らないんだけど、無効化されてるボタンにカーソルを合わせても説明が表示されないらしいので暇な時にここに説明を追記する予定)

## 動作要件
基本的にはDMM GAMES版「プリンセスコネクト！Re:Dive」の動作要件[^1]に準じますが、本ツール自体の動作要件は以下の通りになります。
- OS:Windows7以降
- .NET Framework 4.7.2以降インストール済[^2]
- DMM GAMES版「プリンセスコネクト！Re:Dive」インストール済[^3]
[^1]:[Windowsシステム要件に関するお知らせ | プリンセスコネクト！Re:Dive (プリコネR) 公式サイト | Cygames](https://priconne-redive.jp/news/information/10499/)
[^2]:こちらの要件に関してはあまり気にする必要はないと思います。
[^3]:インストール無しでも起動できますが、主要機能のほとんどは利用できません。

## 把握済の不具合or今後実装・調整予定の機能
- ゲーム起動中にリアルタイムでウインドウサイズを変更する機能(テスト段階)で解像度を設定した際、正しい値に設定されない。
- インポートメニューに公式の既定値を読み込む機能を追加
- 各チェックボックスのチェック状況に応じたテキストボックスの有効・無効の調整
- ツールの設定機能の実装
- ゲームの起動URI、対象のレジストリ値を変更できる高度な設定機能の実装
- レジストリの変更メッセージの非表示機能
