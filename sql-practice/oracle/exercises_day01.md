# Oracle Day01: SQLiteとの違いを手で覚える

`schema_oracle.sql` / `seed_oracle.sql` を流し込んだOracle DBに sqlplus で接続し、
以下の問いに対するSQLを実際に書いて実行してください。
各問は `differences.md` のどのトピックに対応するかを括弧で示しています。

1.（環境・接続）SQLiteで `.tables` を打つと登録されているテーブル一覧が見られる。Oracleで自分のスキーマ内のテーブル一覧を取得するSQLを書け。

2.（ページング）SQLiteの `SELECT * FROM products ORDER BY price ASC LIMIT 3;` と同じ結果になるOracleのSQLを、標準の `FETCH FIRST` 構文で書け。

3.（ページング）上と同じ並び順で「4件目から3件」を取得するOracleのSQLを、`ROW_NUMBER()` を使う方法で書け（`OFFSET`句を使わない書き方で）。

4.（空文字とNULL）`customers` テーブルに `city` が空文字 `''` の行を1件INSERTし、`city IS NULL` で検索してヒットするかどうかを確認するSQL（INSERT文とSELECT文の両方）を書け。

5.（DUAL）SQLiteで `SELECT 1+1;` と書けるが、Oracleで同じ計算をするSQLを書け。

6.（日付）文字列 `'2025-06-01'` をOracleのDATE型に変換した上で、`orders` テーブルの `ordered_at` が2025-06-01以降の注文を抽出するSQLを書け。

7.（日付）`orders` テーブルの `ordered_at` 列を `'YYYY-MM-DD HH24:MI:SS'` 形式の文字列として表示するSQLを書き、時刻部分が実際にどう格納されているか確認せよ。

8.（関数の方言）`customers` の `city` が `NULL` の場合に `'不明'` と表示するSQLを、`NVL` を使う方法と `COALESCE` を使う方法の両方で書け。

9.（外部結合）「注文が1件もない顧客も含めて」全顧客と注文idの一覧を取得するSQLを、標準の `LEFT JOIN` と、旧 `(+)` 記法の両方で書け。

10.（採番）`customers` テーブルに新しい顧客を1件、`id` を指定せずにINSERTし、採番された `id` を確認するSQLを書け（IDENTITY列の挙動を利用すること）。
