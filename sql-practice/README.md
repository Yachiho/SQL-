# sql-practice

SQLite を使った SQL 練習用リポジトリです。

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

## ディレクトリ構成

- `schema.sql` / `seed.sql`: テーブル定義とサンプルデータ
- `setup.sh`: `practice.db` を初期化するスクリプト
- `exercises/dayXX.md`: 問題
- `answers/dayXX.sql`: 自分の解答を書く場所
- `solutions/dayXX.sql`: 模範解答（答え合わせ用、`check.sh` が内部的に使用）
- `check.sh`: 採点スクリプト（`./check.sh <DAY番号>`）
