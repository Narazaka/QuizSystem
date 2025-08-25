# Quiz System

VRChatワールド用簡易クイズシステム（ローカル）

## Install

### VCC用インストーラーunitypackageによる方法（おすすめ）

https://github.com/Narazaka/QuizSystem/releases/latest から `net.narazaka.vrchat.quiz-system-installer.zip` をダウンロードして解凍し、対象のプロジェクトにインポートする。

### VCCによる方法

1. https://vpm.narazaka.net/ から「Add to VCC」ボタンを押してリポジトリをVCCにインストールします。
2. VCCでSettings→Packages→Installed Repositoriesの一覧中で「Narazaka VPM Listing」にチェックが付いていることを確認します。
3. アバタープロジェクトの「Manage Project」から「Quiz System」をインストールします。

## Usage

`QuizSystem` プレハブを配置して設定する

## Changelog

- 0.2.0:
  - `_OnQuizStarted` イベントを追加
  - SetActiveByQuizState ユーティリティを追加
  - QuizSystemのプレハブにデフォルトでSetActiveByQuizStateを設定
- 0.1.0: リリース

## License

[Zlib License](LICENSE.txt)
