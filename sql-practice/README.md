# sql-practice

SQLite を使った SQL 練習用リポジトリです。

Windowsでsqlite3未導入の状態から使い始める場合は [WINDOWS_SETUP.md](./WINDOWS_SETUP.md) を参照してください。

## セットアップ

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
