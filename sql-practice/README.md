# sql-practice

SQLite を使った SQL 練習用リポジトリです。

Windowsでsqlite3未導入の状態から使い始める場合は [WINDOWS_SETUP.md](./WINDOWS_SETUP.md) を参照してください。

SQLiteに慣れた上で次はOracleを学びたい場合は [oracle/](./oracle/) を参照してください。同じEC題材をOracle構文で用意し、SQLiteとの違いを手を動かして確認できます。

## Windows: ダブルクリックで使う（アプリ版）

コマンドを毎回打ちたくない場合は、`sql-practice.bat` をダブルクリックしてください。
メニューが表示され、番号を選ぶだけで「環境構築」「採点」「SQLを自由に試す」ができます
（`sqlite3` のインストールと WINDOWS_SETUP.md の手順3までは事前に済ませておく必要があります）。

```
1. 環境構築（practice.db を作成）
2. 採点する
3. SQLを自由に試す（対話モード）
4. 終了
```

デスクトップにショートカットを作って（`sql-practice.bat` を右クリック→「ショートカットの作成」）
デスクトップに置いておくと、次回からはそのショートカットをダブルクリックするだけで使えます。

## セットアップ（コマンドで実行する場合）

```bash
cd sql-practice
./setup.sh
```

`practice.db`（顧客・商品・注文・注文明細のテーブル）が作成されます。

## 解き方

1. `exercises/day01.md` の問題を確認する
2. `answers/day01.sql` に自分の解答SQLを書き込む（`-- 問1` 〜 `-- 問10` の下に記述）
3. 採点する

```bash
./check.sh 01
```

各問について「正解」「不正解」「未回答」を表示します。不正解の場合は自分のクエリの実行結果と期待される結果（データ）の差分を表示しますが、模範解答のSQL文自体は表示しません。

## 自由に試す（インタラクティブモード）

`answers/dayXX.sql` に書く前に、SQLを気軽に試したいときは `sqlite3` の対話モードが使えます。

```bash
sqlite3 practice.db
```

`sqlite>` というプロンプトになるので、SQLを直接打って実行できます。

```sql
sqlite> .headers on
sqlite> .mode column
sqlite> SELECT * FROM customers;
```

主なコマンド:

- `.tables` : テーブル一覧を表示
- `.schema customers` : テーブルの構造を表示
- `.quit` : 対話モードを終了

ここで試した内容は `answers/dayXX.sql` には反映されません。良さそうなクエリができたら、`answers/dayXX.sql` にコピーして保存してください。

## ディレクトリ構成

- `schema.sql` / `seed.sql`: テーブル定義とサンプルデータ
- `setup.sh`: `practice.db` を初期化するスクリプト
- `exercises/dayXX.md`: 問題
- `answers/dayXX.sql`: 自分の解答を書く場所
- `solutions/dayXX.sql`: 模範解答（答え合わせ用、`check.sh` が内部的に使用）
- `check.sh`: 採点スクリプト（`./check.sh <DAY番号>`）
- `sql-practice.bat`: Windows用メニューアプリ（ダブルクリックで起動）
- `setup.bat` / `check.bat` / `practice.bat`: `sql-practice.bat` から呼ばれる個別のWindows用ランチャー
- `_find_bash.bat`: Git Bashの場所を自動検出する内部ファイル（直接実行しない）
- `oracle/`: Oracle Database 23ai Free版の教材一式（詳細は [oracle/README.md](./oracle/README.md)）
