# Windows セットアップ手順（sqlite3未導入の状態から）

Windowsのコマンドプロンプトには `apt` や `sudo` はありません。ここではWindowsで、
管理者権限なし・sudoなしで一から `sql-practice` を使えるようにする手順をまとめます。

## 0. 前提: Git for Windows

`git clone` を行うためにGitが必要です。未導入の場合は https://git-scm.com/download/win
からインストーラーをダウンロードして実行してください（デフォルト設定のままでOK）。
これにより **Git Bash** も同時にインストールされます。以降の手順はすべて Git Bash 上で行います。
（コマンドプロンプトやPowerShellではなく、スタートメニューから「Git Bash」を開いてください）

## 1. リポジトリをクローン

Git Bashで、作業したいフォルダに移動してから:

```bash
git clone https://github.com/Yachiho/sql-.git
cd sql-
```

このリポジトリは `claude/sql-practice-setup-bazzd5` ブランチがデフォルトブランチなので、
クローンした時点で最新の内容が手に入ります。

## 2. sqlite3.exe を入手する（インストーラー不要）

1. ブラウザで https://sqlite.org/download.html を開く
2. 「Precompiled Binaries for Windows」の欄から
   `sqlite-tools-win-x64-*.zip`（古いPCの場合は `win32-x86` 版）をダウンロード
3. ダウンロードしたzipを展開する（例: `C:\sqlite3\` に展開。中に `sqlite3.exe` がある状態にする）

## 3. sqlite3.exe をPATHに通す（管理者権限不要）

1. Windowsキーを押して「環境変数」と検索し、
   「**アカウントの環境変数を編集**」を選ぶ（これはユーザー単位の設定で管理者権限は不要）
2. 「ユーザー環境変数」の一覧から `Path` を選択 →「編集」→「新規」
3. 手順2で展開したフォルダのパス（例: `C:\sqlite3`）を追加する
4. 「OK」を押して全てのダイアログを閉じる
5. 開いている Git Bash を一度すべて閉じて、再度開き直す（PATHの変更を反映させるため）

### 動作確認

Git Bashで:

```bash
sqlite3 --version
```

バージョン情報が表示されればOKです。

## 4. データベースを構築する

Git Bashで `sql-practice` フォルダに移動して実行:

```bash
cd sql-practice
./setup.sh
```

最後に `顧客数|6` と表示されれば成功です。

## 5. 問題を解く

1. 好きなエディタ（メモ帳、VS Codeなど）で `exercises/day01.md` を開き、問題を確認する
2. 同じく `answers/day01.sql` を開き、`-- 問1` 〜 `-- 問10` の下に自分のSQLを書いて保存する

## 6. 採点する

Git Bashで:

```bash
./check.sh 01
```

各問題について「正解」「不正解」「未回答」が表示されます。不正解の場合は自分の実行結果と
期待される結果（データ）の差分が表示されますが、模範解答のSQL文自体は表示されません。

## うまくいかないとき

- `sqlite3: command not found` → 手順3のPATH設定後にGit Bashを開き直したか確認してください
- `./setup.sh: Permission denied` → `chmod +x setup.sh check.sh` を実行してから再度試してください
- `bash: ./setup.sh: cannot execute: required file not found` →
  改行コードがCRLFになっている可能性があります。`git config --global core.autocrlf input`
  を実行してから再度 `git clone` してみてください
