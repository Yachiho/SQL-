# Git 使い方ガイド（困ったときのメモ）

Git Bashでよく使うコマンドと、これまで実際に遭遇したエラーの対処法をまとめたものです。
sqlite3のインストールに関する詰まりポイントは [WINDOWS_SETUP.md](./WINDOWS_SETUP.md) を参照してください。

## 基本の流れ

1. Git Bashを開く（Windowsキー→「Git Bash」で検索）
2. 作業したい場所に移動する
   ```bash
   cd ~/Desktop
   ```
3. 最初の1回だけ: リポジトリをコピーしてくる（クローン）
   ```bash
   git clone https://github.com/Yachiho/sql-.git
   ```
4. 2回目以降、更新を取り込みたいとき
   ```bash
   cd ~/Desktop/sql-
   git pull origin claude/sql-practice-setup-bazzd5
   ```

## よく使うコマンド一覧

| コマンド | やること |
|---|---|
| `pwd` | 今どのフォルダにいるか表示 |
| `ls` / `ls -la` | 今のフォルダの中身を表示（`-la`だと隠しフォルダの`.git`も見える） |
| `cd フォルダ名` | そのフォルダに移動 |
| `cd ..` | 1つ上のフォルダに戻る |
| `git clone <URL>` | リポジトリを丸ごとコピーしてくる（最初の1回だけ） |
| `git pull origin <ブランチ名>` | リモートの最新内容を取り込む（2回目以降） |
| `git status` | 今の状態（変更の有無、ブランチ名など）を確認 |
| `git remote -v` | どのURLと繋がっているか確認 |
| `git branch` | 今いるブランチを確認（`*`が付いている行） |
| `git log --oneline -5` | 直近5件の更新履歴を確認 |

## 「これなに？」用語集

- **リポジトリ (repository)**: プロジェクトのファイル一式＋変更履歴をまとめたもの。フォルダの中に隠しフォルダ`.git`があるのが目印。
- **リモート (remote) / origin**: GitHub上にある「本体」の場所のこと。`origin`はその場所につけられたニックネーム（決まり文句のようなもの）。
- **クローン (clone)**: リモートのリポジトリを、初めて手元にまるごとコピーしてくること。
- **プル (pull)**: 既に手元にあるリポジトリに対して、リモートの最新の変更だけを取り込むこと。
- **ブランチ (branch)**: 作業の枝分かれ。このプロジェクトでは基本的に `claude/sql-practice-setup-bazzd5` というブランチだけを使う。
- **コミット (commit)**: 変更を記録するひとまとまりの単位（このガイドでは自分で作る必要はなく、Claude側で行う）。

## よくあるエラーと対処

### `fatal: destination path 'sql-' already exists and is not an empty directory.`

**原因:** クローンしようとした場所に、同じ名前の `sql-` フォルダが既にある。

**対処:** 古いフォルダを別名にリネームしてから、改めてクローンする。
```bash
mv sql- sql-_old
git clone https://github.com/Yachiho/sql-.git
```

### `There is no tracking information for the current branch.`

**原因:** 今いるブランチが、リモートのどのブランチと対応するか設定されていない。

**対処:** ブランチ名を明示してpullする。
```bash
git pull origin claude/sql-practice-setup-bazzd5
```

### `fatal: 'origin' does not appear to be a git repository`

**原因:** `origin`（リモートのニックネーム）がそもそも登録されていない。

**対処:** 手動で登録してからpullする。
```bash
git remote add origin https://github.com/Yachiho/sql-.git
git pull origin claude/sql-practice-setup-bazzd5
```

### 右クリックしても「Git Bash Here」が出ない

**原因:** Windows 11の簡略化された右クリックメニューに隠れている（または表示されない設定になっている）。

**対処:** 右クリックせず、Git Bashを単体で起動して`cd`で目的のフォルダに移動する。
```bash
cd ~/Desktop/sql-
```

## 困ったときの調べ方（切り分けの順番）

1. `pwd` → 今どこにいるか確認
2. `ls -la` → `.git`フォルダがあるか確認（無ければgitリポジトリの外にいる）
3. `git status` → 今の状態を確認
4. `git remote -v` → `origin`が設定されているか確認
5. `git branch` → 今のブランチ名を確認

エラーが出たら、まずこの順番で確認し、出てきたメッセージ全文をそのまま貼ってもらえれば対応しやすくなります。
