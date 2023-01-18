# PR_Manager

[![GitHub release (latest by date)](https://img.shields.io/github/v/release/South2190/PR_Manager)](../../releases)
[![](https://img.shields.io/badge/-changelog-green)](changelog.md)

DMM GAMES版「プリンセスコネクト！Re:Dive」の解像度等をいじれるツール。

## 注意事項
- 本ツールはレジストリを書き換えます。みだりに扱うとコンピューターに思わぬ損害を与える可能性があります。**本ツールを使用したことにより発生した損害については一切の責任を負いかねます。**
- ウインドウサイズがデフォルトに戻されてしまいますので、設定を適用した後ゲームウインドウの端を掴まないように注意してください。
- このツールの機能は(実質)プリコネのバグを利用して実現しているものなので、プリコネ側で修正されてしまうと一気に使い物にならなくなる可能性があります。

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
- OS:Windows10以降
- .NET Framework 4.7.2以降インストール済
- DMM GAMES版「プリンセスコネクト！Re:Dive」インストール済[^1]
[^1]:インストール無しで起動した場合、主要機能のほとんどは利用できません。

## 把握済の不具合or今後実装・調整予定の機能
- ゲーム起動中にリアルタイムでウインドウサイズを変更する機能(テスト段階)で解像度を設定した際、正しい値に設定されない。
- ツールの設定機能の実装
- レジストリの変更メッセージの非表示機能
