# SQLite → Oracle 差分ノート

`schema_oracle.sql` / `seed_oracle.sql` を流し込んだ状態のOracle DBに sqlplus で接続し、
このファイルのクエリをそのままコピペして実行しながら、SQLiteとの挙動の違いを確認してください。
（sqlplusへの接続方法は `README.md` を参照）

各トピックは「① SQLiteでの書き方 → ② Oracleでの書き方 → ③ 動かして確認するクエリ」の3点セットです。

---

## (1) 環境・接続（インスタンス/スキーマ、DESC、SELECT * FROM tab）

**① SQLite**
ファイル1つがデータベース。`sqlite3 practice.db` と打てばそれで接続完了。
テーブル一覧は `.tables`、構造確認は `.schema customers`（いずれもsqlite3固有のドットコマンド）。

**② Oracle**
「インスタンス（DB本体のプロセス群）」の中に「スキーマ（ユーザーごとの名前空間）」があり、
接続時にユーザー名・パスワード・接続先（サービス名）を指定する。
テーブル構造の確認は `DESC`（`DESCRIBE`の省略形）、自分のスキーマのテーブル一覧は
`user_tables`ビューか、古くからある互換ビュー`tab`で見る。

**③ 動かして確認するクエリ**
```sql
-- 自分のスキーマにあるテーブル一覧
SELECT table_name FROM user_tables ORDER BY table_name;

-- 昔からある簡易版（表示専用の互換ビュー）
SELECT * FROM tab;

-- テーブル構造の確認（sqlite3の .schema customers に相当）
DESC customers;
DESC order_items;
```

---

## (2) ページング（LIMIT → FETCH FIRST / OFFSET FETCH / ROWNUM / ROW_NUMBER）

**① SQLite**
```sql
SELECT * FROM products ORDER BY price ASC LIMIT 3;
```

**② Oracle**
Oracle 12c以降は標準SQLの `FETCH FIRST` / `OFFSET ... FETCH` が使える。
それ以前や既存コードでは `ROWNUM` を使った書き方が定番で、今も現場でよく見る。
「途中のページ」を取りたいときは `ROW_NUMBER()` ウィンドウ関数を使うのが安全。

**③ 動かして確認するクエリ**
```sql
-- 標準構文（今後書くならこれ）
SELECT * FROM products ORDER BY price ASC FETCH FIRST 3 ROWS ONLY;

-- オフセット付き（4件目から3件）
SELECT * FROM products ORDER BY price ASC OFFSET 3 ROWS FETCH NEXT 3 ROWS ONLY;

-- 旧来のROWNUM方式（先にORDER BYした結果をサブクエリで包んでからROWNUMを付ける）
SELECT * FROM (
    SELECT * FROM products ORDER BY price ASC
)
WHERE ROWNUM <= 3;

-- 中間ページを取りたい場合はROW_NUMBER()を使う（ROWNUMだけでは書けない）
SELECT * FROM (
    SELECT p.*, ROW_NUMBER() OVER (ORDER BY price ASC) AS rn
    FROM products p
)
WHERE rn BETWEEN 4 AND 6;
```

---

## (3) 空文字とNULLの同一視

**① SQLite**
空文字列 `''` と `NULL` は明確に区別される別の値。
```sql
SELECT '' IS NULL;   -- 0 (false)
SELECT length('');   -- 0
```

**② Oracle**
`VARCHAR2`型の空文字列 `''` は内部的に `NULL` と同一視される（歴史的な仕様。有名な罠）。

**③ 動かして確認するクエリ**
```sql
-- 定数での確認
SELECT CASE WHEN '' IS NULL THEN '空文字はNULL扱い' ELSE 'NULLではない' END AS result
FROM dual;

SELECT LENGTH('') FROM dual;   -- 0ではなくNULLが返ってくる

-- 実データで確認: cityに空文字を入れてみる
INSERT INTO customers (name, email, city, created_at)
VALUES ('差分確認太郎', 'diff_check_empty@example.com', '', TO_DATE('2025-07-01', 'YYYY-MM-DD'));

-- ''を入れたはずなのに IS NULL でヒットする
SELECT id, name, city FROM customers WHERE city IS NULL;

-- 確認が終わったら戻す（COMMITしていなければROLLBACKで取り消せる）
ROLLBACK;
```

---

## (4) DUALテーブル

**① SQLite**
`FROM`句なしで定数式や関数をそのまま評価できる。
```sql
SELECT 1 + 1;
SELECT date('now');
```

**② Oracle**
構文上 `SELECT` には必ず `FROM`句が必要。定数式や関数の動作確認専用に、
1行1列だけのダミーテーブル `DUAL` を使う。

**③ 動かして確認するクエリ**
```sql
SELECT 1 + 1 FROM dual;
SELECT SYSDATE FROM dual;
SELECT 'Hello' || ' ' || 'Oracle' FROM dual;
```

---

## (5) 日付（SYSDATE / TO_DATE / TO_CHAR、DATE型が時刻を持つこと）

**① SQLite**
日付は単なる文字列（TEXT）。日付関数はあるが型としての保証はない。
```sql
SELECT date('now');
SELECT datetime('now');
```

**② Oracle**
`DATE`型は「年月日+時分秒」を必ず内部に保持する専用の型。
文字列 ⇔ DATE の変換は `TO_DATE` / `TO_CHAR` で明示的に行う。

**③ 動かして確認するクエリ**
```sql
-- 現在時刻（時分秒付き）
SELECT SYSDATE FROM dual;

-- 文字列 -> DATE
SELECT TO_DATE('2025-06-01', 'YYYY-MM-DD') FROM dual;

-- DATE -> 文字列（表示形式を自分で指定する）
SELECT TO_CHAR(SYSDATE, 'YYYY-MM-DD HH24:MI:SS') FROM dual;

-- DATE型が時刻も持っていることの確認
-- seed_oracle.sqlはTO_DATE('YYYY-MM-DD')で日付だけ指定しているが、
-- 内部的には時刻部分が 00:00:00 として保持されている
SELECT ordered_at, TO_CHAR(ordered_at, 'YYYY-MM-DD HH24:MI:SS') AS with_time
FROM orders
WHERE id = 1;
```

---

## (6) 関数の方言（NVL / NVL2 / COALESCE / DECODE / SUBSTR / INSTR、|| は共通）

**① SQLite**
`IFNULL(x, y)`、`COALESCE(...)`、`CASE WHEN`、`substr()`、`instr()` を使う。

**② Oracle**
`NVL(x, y)`（IFNULLに相当・Oracle独自）、`NVL2(x, 非NULL時, NULL時)`（Oracle独自）、
`COALESCE(...)`（標準構文・SQLiteと共通）、`DECODE(...)`（CASE式の元祖・Oracle独自）、
`SUBSTR()` / `INSTR()`（名前はほぼ共通）。文字列結合の `||` は両方で共通して使える。

**③ 動かして確認するクエリ**
```sql
-- NVL: cityがNULLなら'不明'にする
SELECT name, NVL(city, '不明') AS city FROM customers;

-- NVL2: cityがNULLでなければ'あり'、NULLなら'なし'
SELECT name, NVL2(city, 'あり', 'なし') AS has_city FROM customers;

-- COALESCE: SQLiteと書き方が共通
SELECT name, COALESCE(city, '不明') AS city FROM customers;

-- DECODE: 簡易switch文（CASE式のOracle独自の前身。今も現場でよく見る）
SELECT id, status,
       DECODE(status, 'completed', '完了',
                       'pending',   '保留',
                       'cancelled', 'キャンセル',
                       '不明') AS status_ja
FROM orders;

-- SUBSTR / INSTR / || は書き方がSQLiteとほぼ共通
SELECT SUBSTR(email, 1, INSTR(email, '@') - 1) AS local_part,
       name || '様' AS greeting
FROM customers;
```

---

## (7) 外部結合の旧記法 (+) と 標準 LEFT JOIN の対応

**① SQLite**
`(+)`記法は存在しない。標準の`LEFT JOIN`のみ。
```sql
SELECT c.name, o.id AS order_id
FROM customers c
LEFT JOIN orders o ON o.customer_id = c.id;
```

**② Oracle**
標準の`LEFT JOIN`も使えるが、古いコードでは`WHERE`句に`(+)`を付ける独自記法が
今も大量に残っている。自分で新しく書くときは標準記法を使えばよいが、
既存コードを読むために`(+)`は読めるようにしておく必要がある。
`(+)`は「行が無くてもNULLで埋めてよい側（=外側）」のテーブルの結合列に付ける。

**③ 動かして確認するクエリ**
```sql
-- 標準記法（今後書くときはこちら）
SELECT c.name, o.id AS order_id
FROM customers c
LEFT JOIN orders o ON o.customer_id = c.id
ORDER BY c.id;

-- 旧(+)記法（上と同じ結果になる。読めることが目的）
SELECT c.name, o.id AS order_id
FROM customers c, orders o
WHERE o.customer_id (+) = c.id
ORDER BY c.id;
```

---

## (8) 採番（AUTOINCREMENT → シーケンス / IDENTITY列）

**① SQLite**
`INTEGER PRIMARY KEY`だけで自動的にrowidの別名になり暗黙で採番される。
```sql
CREATE TABLE sample (id INTEGER PRIMARY KEY, name TEXT);
INSERT INTO sample (name) VALUES ('test');
SELECT last_insert_rowid();
```

**② Oracle**
自動採番の仕組みを明示的に定義する必要がある。伝統的にはシーケンス、
12c以降はIDENTITY列（`schema_oracle.sql`ではこちらを採用）。

**③ 動かして確認するクエリ**
```sql
-- シーケンス方式のイメージ（このリポジトリのテーブルでは未使用だが、現場で頻出）
CREATE SEQUENCE sample_seq START WITH 1 INCREMENT BY 1;
CREATE TABLE sample (id NUMBER DEFAULT sample_seq.NEXTVAL PRIMARY KEY, name VARCHAR2(100));
INSERT INTO sample (name) VALUES ('test');
SELECT sample_seq.CURRVAL FROM dual;   -- 直前に採番された値
DROP TABLE sample PURGE;
DROP SEQUENCE sample_seq;

-- IDENTITY列方式（customersテーブルで実際に確認する）
INSERT INTO customers (name, email, city, created_at)
VALUES ('採番確認太郎', 'diff_check_identity@example.com', '東京', TO_DATE('2025-07-02', 'YYYY-MM-DD'));

SELECT id, name FROM customers WHERE email = 'diff_check_identity@example.com';

-- 確認が終わったら戻す
ROLLBACK;
```
