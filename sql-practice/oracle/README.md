# Oracle Database 23ai Free を Docker で動かす

`../` (SQLite版)と同じEC題材を、Oracle 23ai Free上で動かすための手順です。
SQLiteとの違いを手を動かして確認することが目的なので、まずは環境構築から進めます。

SQLiteとの違いは [differences.md](./differences.md)（動かして確認する詳しい解説）、
[CHEATSHEET.md](./CHEATSHEET.md)（作業中にサッと見返す早見表）にまとめています。

## 前提

- Docker Desktop（または Docker Engine）がインストール済みであること
- 対象イメージ: Oracle Database 23ai Free（最新版）

イメージの入手方法は2通りあります。すぐ試したい場合は方式Bがおすすめです。

## 方式A: Oracle公式イメージ（Oracle Container Registry）

1. https://container-registry.oracle.com にアクセスし、Oracleアカウントでログインして
   `Database` → `free` のライセンス条項に同意する（初回のみ必要）
2. ローカルでもログインする
   ```bash
   docker login container-registry.oracle.com
   ```
3. イメージを取得
   ```bash
   docker pull container-registry.oracle.com/database/free:latest
   ```
4. コンテナを起動（パスワードは自分の任意の値に変更すること）
   ```bash
   docker run -d --name oracle23ai \
     -p 1521:1521 -p 5500:5500 \
     -e ORACLE_PWD=YourPassword123 \
     container-registry.oracle.com/database/free:latest
   ```

## 方式B: コミュニティイメージ gvenzl/oracle-free（アカウント登録不要・手軽）

Oracleアカウントの登録なしですぐ試せる、学習用によく使われる非公式イメージです。

1. イメージを取得
   ```bash
   docker pull gvenzl/oracle-free:23-slim
   ```
2. コンテナを起動（パスワードは自分の任意の値に変更すること）
   ```bash
   docker run -d --name oracle23ai \
     -p 1521:1521 \
     -e ORACLE_PASSWORD=YourPassword123 \
     gvenzl/oracle-free:23-slim
   ```

## 起動確認（方式A・B共通）

初回起動はデータベースの初期化が走るため数分かかります。ログで完了を確認してください。

```bash
docker logs -f oracle23ai
```

`DATABASE IS READY TO USE!` と表示されたら準備完了です（Ctrl+Cでログ表示を終了できます）。

## 接続文字列の要素

- ホスト: `localhost`（Dockerをローカルで動かしている場合）
- ポート: `1521`（デフォルト）
- サービス名（PDB名）: `FREEPDB1`（23ai Freeのデフォルトのプラガブルデータベース名）

## sqlplus で接続する

コンテナに同梱されている sqlplus をそのまま使うのが一番簡単です（クライアントの別途インストール不要）。

```bash
docker exec -it oracle23ai sqlplus system/YourPassword123@//localhost:1521/FREEPDB1
```

### 練習用の専用ユーザーを作る（推奨）

SYSTEMユーザーで直接作業せず、練習専用のスキーマを作るのが本番に近い作法です。
上記でSYSTEMとして接続した状態で:

```sql
CREATE USER practice IDENTIFIED BY YourPassword123;
GRANT CONNECT, RESOURCE, UNLIMITED TABLESPACE TO practice;
EXIT;
```

以降はこの`practice`ユーザーで接続します。

```bash
docker exec -it oracle23ai sqlplus practice/YourPassword123@//localhost:1521/FREEPDB1
```

## schema_oracle.sql / seed_oracle.sql を流し込む

ホスト側にあるSQLファイルをコンテナにコピーしてから、sqlplus内で `@` コマンドで実行します。

```bash
docker cp schema_oracle.sql oracle23ai:/tmp/schema_oracle.sql
docker cp seed_oracle.sql   oracle23ai:/tmp/seed_oracle.sql

docker exec -it oracle23ai sqlplus practice/YourPassword123@//localhost:1521/FREEPDB1
```

sqlplusのプロンプトで:

```sql
@/tmp/schema_oracle.sql
@/tmp/seed_oracle.sql
```

`seed_oracle.sql`の最後に`COMMIT;`が入っているので、これでデータが確定します。

続けて `differences.md` や `exercises_day01.md` のクエリを、同じsqlplusセッションで
そのまま打ち込んで試してください。

## 文字化け対策

日本語データ（氏名・都市名など）を扱うため、ホスト側ターミナルの文字コードと
Oracleクライアントの文字コード設定(`NLS_LANG`)を合わせておくと安全です。

```bash
export NLS_LANG=.AL32UTF8
```

（`docker exec`でコンテナ内のsqlplusを直接使う場合は、コンテナ側の文字コードは
23ai Freeのデフォルトで `AL32UTF8` になっているため、通常は追加設定なしで問題ありません）

## 後片付け

学習が終わったらコンテナを止めて削除できます（データも消えます）。

```bash
docker stop oracle23ai
docker rm oracle23ai
```

停止するだけでデータを残したい場合は `docker stop oracle23ai` のみ実行し、
再開するときは `docker start oracle23ai` を使ってください。

## うまくいかないとき

- `docker: command not found` → Docker Desktopがインストールされているか、起動しているか確認
- `Error response from daemon: port is already allocated` → 別のプロセスが1521番ポートを使用中。
  `-p 15211:1521` のようにホスト側のポート番号を変えて起動し、接続時も `//localhost:15211/FREEPDB1` とする
- ログに `DATABASE IS READY TO USE!` がなかなか出ない → 初回はイメージのサイズも大きく、
  初期化に5〜10分程度かかることがあるので気長に待つ
- `ORA-12541: TNS:no listener` → まだ起動途中の可能性が高い。`docker logs -f oracle23ai` で
  準備完了メッセージを待ってから再接続する
