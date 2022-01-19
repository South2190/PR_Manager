# PR_Manager
DMM GAMES版「プリンセスコネクト！Re:Dive」の解像度等をいじれるツール。

## ダウンロード
- [Releases](https://github.com/South2190/PR_Manager/releases)

## 注意事項
- ***本ツールはベータ版です。***
- 本ツールの使用は自己責任でお願いします。
- 改変等は自由にして頂いて大丈夫です。但し二次配布は無しで。
- ウインドウサイズがデフォルトに戻されてしまいますので、設定を適用した後ウインドウの端を掴まないように注意してください。
- このツールの機能は実質プリコネのバグを利用して実現しているものなので、プリコネ側で修正されてしまうと一気に使い物にならなくなる可能性があります。(ウマ娘などは対策済の模様)

## なにができるの
- ウインドウモードとフルスクリーンモードの切り替え
- 任意の解像度を縦横別々に指定(アス比を変更したい場合など)
- ゲーム起動時に表示するモニタの選択(マルチモニタ環境のみ利用可能)
- ゲームをワンクリックで強制終了

## 使用方法
各設定項目を入力した後、右下の「適用」ボタンを押します。レジストリを書き換えた旨のメッセージが表示されればOKです。

各設定項目の意味に関してはカーソルを合わせた際に説明が表示されますのでそちらを参考にしてみてください。(ただWPFの仕様なのか知らないんだけど無効化されてるボタンにカーソルを合わせても説明が表示されないらしいので暇な時にここに説明を追記する予定)

## 動作要件
基本的にはDMM GAMES版「プリンセスコネクト！Re:Dive」の動作要件に準じますが、本ツール自体の動作要件は以下の通りになります。
- Windows7以降
- .NET Framework 4.7.2以降[^1]
- DMM GAMES版「プリンセスコネクト！Re:Dive」インストール済[^2]
[^1]:こちらの要件に関してはあまり気にする必要はないと思います。
[^2]:インストール無しで起動した場合、一部の機能は制限されます。

## 更新履歴
- [changelog.md](https://github.com/South2190/PR_Manager/blob/main/changelog.md)
