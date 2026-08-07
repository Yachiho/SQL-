# Oracle チートシート（SQLite対比・早見表）

作業中にサッと見返すための早見表です。動かして確認できる詳しい解説とサンプルクエリは
[differences.md](./differences.md) を参照してください。

## 基本操作

| 項目 | SQLite | Oracle |
|---|---|---|
| 接続 | `sqlite3 practice.db` | `sqlplus user/pass@//host:port/service` |
| テーブル一覧 | `.tables` | `SELECT table_name FROM user_tables;` / `SELECT * FROM tab;` |
| テーブル構造 | `.schema customers` | `DESC customers;` |
| 定数式の評価 | `SELECT 1+1;` | `SELECT 1+1 FROM dual;` |
| 件数制限 | `LIMIT 3` | `FETCH FIRST 3 ROWS ONLY` |
| 件数制限+オフセット | `LIMIT 3 OFFSET 3` | `OFFSET 3 ROWS FETCH NEXT 3 ROWS ONLY` |
| 旧来の件数制限 | （なし） | `WHERE ROWNUM <= 3`（ORDER BY済みのサブクエリを包んで使う） |
| 中間ページの取得 | （なし） | `ROW_NUMBER() OVER (ORDER BY ...)` |

## 型

| SQLite | Oracle | 備考 |
|---|---|---|
| TEXT | VARCHAR2(n) | 長さ指定が必須 |
| INTEGER | NUMBER / NUMBER(p, s) | 桁数を明示するのが一般的 |
| REAL | NUMBER / FLOAT | |
| （型なしの日付文字列） | DATE | 年月日+時分秒を保持する専用型 |
| BLOB | BLOB / RAW | |

## NULL・空文字

| 項目 | SQLite | Oracle |
|---|---|---|
| `''` と NULL | 別物として区別される | 同一視される（`'' IS NULL` が真になる） |
| `LENGTH('')` | 0 | NULL |

## 日付

| 項目 | SQLite | Oracle |
|---|---|---|
| 現在時刻 | `date('now')` / `datetime('now')` | `SYSDATE` |
| 文字列→日付 | 不要（TEXTのまま扱う） | `TO_DATE('2025-06-01', 'YYYY-MM-DD')` |
| 日付→文字列 | `strftime(...)` | `TO_CHAR(d, 'YYYY-MM-DD HH24:MI:SS')` |
| 時刻の保持 | 文字列の書き方次第 | DATE型は常に時分秒を保持する |

## 関数

| 用途 | SQLite | Oracle |
|---|---|---|
| NULL置換 | `IFNULL(x, y)` | `NVL(x, y)` |
| NULLで分岐 | CASE式で代用 | `NVL2(x, 非NULL時の値, NULL時の値)` |
| 複数候補から非NULL | `COALESCE(...)` | `COALESCE(...)`（共通） |
| 簡易switch | `CASE WHEN` | `DECODE(...)` または `CASE WHEN`（共通） |
| 部分文字列 | `substr()` | `SUBSTR()` |
| 位置検索 | `instr()` | `INSTR()` |
| 文字列結合 | `\|\|` | `\|\|`（共通） |

## 結合

| 項目 | SQLite | Oracle |
|---|---|---|
| 外部結合 | `LEFT JOIN` のみ | `LEFT JOIN`（標準）/ `(+)`（旧記法、読めれば十分） |

## 採番

| 項目 | SQLite | Oracle |
|---|---|---|
| 自動採番 | `INTEGER PRIMARY KEY`（暗黙で自動採番） | `GENERATED [ALWAYS\|BY DEFAULT] AS IDENTITY`（12c以降）/ `SEQUENCE`（伝統的な方式） |
| 直前の採番値 | `last_insert_rowid()` | `シーケンス名.CURRVAL` |

## トランザクション

| 項目 | SQLite | Oracle |
|---|---|---|
| 確定 | 設定次第だが自動コミットのことが多い | `COMMIT;` を明示しないと確定しない |
| 取り消し | `ROLLBACK;` | `ROLLBACK;`（共通） |
| 複数行INSERT | `INSERT INTO t VALUES (1,'a'),(2,'b');` | 非対応。1文1行で書く（`INSERT ALL`は例外） |

---

各項目の背景や実際に動かして確認する手順は [differences.md](./differences.md) にまとまっています。
