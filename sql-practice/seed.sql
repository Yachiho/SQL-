INSERT INTO customers (id, name, email, city, created_at) VALUES
(1, '佐藤健',   'sato@example.com',     '東京',   '2025-01-15'),
(2, '鈴木愛',   'suzuki@example.com',   '大阪',   '2025-02-03'),
(3, '高橋修',   'takahashi@example.com','東京',   '2025-02-20'),
(4, '田中優',   'tanaka@example.com',   '名古屋', '2025-03-10'),
(5, '伊藤誠',   'ito@example.com',      '大阪',   '2025-04-01'),
(6, '渡辺茜',   'watanabe@example.com', '福岡',   '2025-05-12');

INSERT INTO products (id, name, category, price) VALUES
(1, 'ワイヤレスイヤホン',   '家電', 8000),
(2, 'USB-Cケーブル',       '家電', 1200),
(3, 'プログラミング入門書', '書籍', 3200),
(4, 'コーヒー豆 1kg',      '食品', 2500),
(5, 'ノートPCスタンド',    '家電', 4500),
(6, 'SQL実践ガイド',       '書籍', 3800),
(7, '緑茶 500ml×24',      '食品', 2000);

INSERT INTO orders (id, customer_id, ordered_at, status) VALUES
(1, 1, '2025-05-01', 'completed'),
(2, 1, '2025-05-20', 'completed'),
(3, 2, '2025-05-05', 'completed'),
(4, 3, '2025-05-11', 'cancelled'),
(5, 2, '2025-06-01', 'pending'),
(6, 4, '2025-06-03', 'completed'),
(7, 1, '2025-06-15', 'completed'),
(8, 5, '2025-06-18', 'completed'),
(9, 3, '2025-06-20', 'pending');

INSERT INTO order_items (id, order_id, product_id, quantity) VALUES
(1, 1, 1, 1), (2, 1, 2, 2),
(3, 2, 3, 1),
(4, 3, 4, 3),
(5, 4, 5, 1),
(6, 5, 6, 1), (7, 5, 2, 1),
(8, 6, 1, 1), (9, 6, 5, 1),
(10, 7, 4, 2), (11, 7, 7, 1),
(12, 8, 6, 1), (13, 8, 3, 1),
(14, 9, 7, 4);
