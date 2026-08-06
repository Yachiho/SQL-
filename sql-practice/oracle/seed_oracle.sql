-- ============================================================
-- seed_oracle.sql
-- schema_oracle.sql に対するサンプルデータ投入（../seed.sql のOracle版）
--
-- 気づきポイント（8トピック以外のおまけの差分）:
--   SQLite/PostgreSQL/MySQLでは
--     INSERT INTO t (a, b) VALUES (1, 'x'), (2, 'y');
--   のような複数行まとめてのVALUES指定ができるが、Oracleはこの書き方をサポートしない。
--   1文=1行のINSERTを繰り返す必要がある（またはINSERT ALLを使う）。
--
-- 日付は必ず TO_DATE('YYYY-MM-DD', 'YYYY-MM-DD') 形式で明示的に変換する。
-- SQLite版のように '2025-06-01' という文字列をそのままDATE列には入れられない
-- （文字列とDATE型は別物として扱われる）。
-- ============================================================

-- 顧客
INSERT INTO customers (id, name, email, city, created_at) VALUES
    (1, '佐藤健', 'sato@example.com', '東京', TO_DATE('2025-01-15', 'YYYY-MM-DD'));
INSERT INTO customers (id, name, email, city, created_at) VALUES
    (2, '鈴木愛', 'suzuki@example.com', '大阪', TO_DATE('2025-02-03', 'YYYY-MM-DD'));
INSERT INTO customers (id, name, email, city, created_at) VALUES
    (3, '高橋修', 'takahashi@example.com', '東京', TO_DATE('2025-02-20', 'YYYY-MM-DD'));
INSERT INTO customers (id, name, email, city, created_at) VALUES
    (4, '田中優', 'tanaka@example.com', '名古屋', TO_DATE('2025-03-10', 'YYYY-MM-DD'));
INSERT INTO customers (id, name, email, city, created_at) VALUES
    (5, '伊藤誠', 'ito@example.com', '大阪', TO_DATE('2025-04-01', 'YYYY-MM-DD'));
INSERT INTO customers (id, name, email, city, created_at) VALUES
    (6, '渡辺茜', 'watanabe@example.com', '福岡', TO_DATE('2025-05-12', 'YYYY-MM-DD'));

-- 商品
INSERT INTO products (id, name, category, price) VALUES
    (1, 'ワイヤレスイヤホン', '家電', 8000);
INSERT INTO products (id, name, category, price) VALUES
    (2, 'USB-Cケーブル', '家電', 1200);
INSERT INTO products (id, name, category, price) VALUES
    (3, 'プログラミング入門書', '書籍', 3200);
INSERT INTO products (id, name, category, price) VALUES
    (4, 'コーヒー豆 1kg', '食品', 2500);
INSERT INTO products (id, name, category, price) VALUES
    (5, 'ノートPCスタンド', '家電', 4500);
INSERT INTO products (id, name, category, price) VALUES
    (6, 'SQL実践ガイド', '書籍', 3800);
INSERT INTO products (id, name, category, price) VALUES
    (7, '緑茶 500ml×24', '食品', 2000);

-- 注文
INSERT INTO orders (id, customer_id, ordered_at, status) VALUES
    (1, 1, TO_DATE('2025-05-01', 'YYYY-MM-DD'), 'completed');
INSERT INTO orders (id, customer_id, ordered_at, status) VALUES
    (2, 1, TO_DATE('2025-05-20', 'YYYY-MM-DD'), 'completed');
INSERT INTO orders (id, customer_id, ordered_at, status) VALUES
    (3, 2, TO_DATE('2025-05-05', 'YYYY-MM-DD'), 'completed');
INSERT INTO orders (id, customer_id, ordered_at, status) VALUES
    (4, 3, TO_DATE('2025-05-11', 'YYYY-MM-DD'), 'cancelled');
INSERT INTO orders (id, customer_id, ordered_at, status) VALUES
    (5, 2, TO_DATE('2025-06-01', 'YYYY-MM-DD'), 'pending');
INSERT INTO orders (id, customer_id, ordered_at, status) VALUES
    (6, 4, TO_DATE('2025-06-03', 'YYYY-MM-DD'), 'completed');
INSERT INTO orders (id, customer_id, ordered_at, status) VALUES
    (7, 1, TO_DATE('2025-06-15', 'YYYY-MM-DD'), 'completed');
INSERT INTO orders (id, customer_id, ordered_at, status) VALUES
    (8, 5, TO_DATE('2025-06-18', 'YYYY-MM-DD'), 'completed');
INSERT INTO orders (id, customer_id, ordered_at, status) VALUES
    (9, 3, TO_DATE('2025-06-20', 'YYYY-MM-DD'), 'pending');

-- 注文明細
INSERT INTO order_items (id, order_id, product_id, quantity) VALUES (1, 1, 1, 1);
INSERT INTO order_items (id, order_id, product_id, quantity) VALUES (2, 1, 2, 2);
INSERT INTO order_items (id, order_id, product_id, quantity) VALUES (3, 2, 3, 1);
INSERT INTO order_items (id, order_id, product_id, quantity) VALUES (4, 3, 4, 3);
INSERT INTO order_items (id, order_id, product_id, quantity) VALUES (5, 4, 5, 1);
INSERT INTO order_items (id, order_id, product_id, quantity) VALUES (6, 5, 6, 1);
INSERT INTO order_items (id, order_id, product_id, quantity) VALUES (7, 5, 2, 1);
INSERT INTO order_items (id, order_id, product_id, quantity) VALUES (8, 6, 1, 1);
INSERT INTO order_items (id, order_id, product_id, quantity) VALUES (9, 6, 5, 1);
INSERT INTO order_items (id, order_id, product_id, quantity) VALUES (10, 7, 4, 2);
INSERT INTO order_items (id, order_id, product_id, quantity) VALUES (11, 7, 7, 1);
INSERT INTO order_items (id, order_id, product_id, quantity) VALUES (12, 8, 6, 1);
INSERT INTO order_items (id, order_id, product_id, quantity) VALUES (13, 8, 3, 1);
INSERT INTO order_items (id, order_id, product_id, quantity) VALUES (14, 9, 7, 4);

-- SQLiteと違い、既定でトランザクションが開始されており明示的にCOMMITしないと確定しない
COMMIT;
