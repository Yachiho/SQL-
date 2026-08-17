# Oracle Database 23ai Free を Docker で動かす

`../`（SQLite版）と同じEC題材（customers / products / orders / order_items）を、
Oracle Database 23ai Free上で動かすための環境構築ガイドです。
Dockerもデータベースも初めての人でも、このページを上から順にやれば
「Docker上でOracleを起動 → SQL Developerから接続 → 既存のEC題材を触れる」状態になります。

学習用のDDL/データはこのディレクトリに用意済みです。

- `schema_oracle.sql` : テーブル定義（DDL）
- `seed_oracle.sql`   : サンプルデータ投入（DML）

> これらのファイルが見当たらない場合は、先に `../schema.sql` / `../seed.sql`（SQLite版）を
> 参考にOracle構文へ書き換えたファイルを用意してから、本ガイドの手順に進んでください。

SQLiteとの違いは [differences.md](./differences.md)（動かして確認する詳しい解説）、
[CHEATSHEET.md](./CHEATSHEET.md)（作業中にサッと見返す早見表）にまとめています。
Day01の練習問題は [exercises_day01.md](./exercises_day01.md) を参照してください。

## この後やること（全体の流れ）

1. Dockerで Oracle Database 23ai Free を起動する
2. SQL Developer をインストールして接続する
3. 学習用ユーザーを作成する（任意だが推奨）
4. `schema_oracle.sql` / `seed_oracle.sql` を流し込む
5. 既存のEC題材（`differences.md` / `exercises_day01.md`）を触ってみる

自動化スクリプト `setup_oracle.sh` を使うと 1〜4 をまとめて実行できます
（詳しくは[自動化スクリプトで一気にセットアップする](#自動化スクリプトで一気にセットアップする)を参照）。
初めての人はまず手動で一通り流れを理解することをおすすめします。

## 前提

- Docker Desktop（または Docker Engine）がインストール・起動済みであること
- ディスクの空き容量が十分にあること（イメージ+データ領域で数GB必要）
- メモリは最低2GB、できれば4GB以上をDockerに割り当てられること

---

## 1. Docker で Oracle Database 23ai Free を起動する

### 使用イメージ

Oracle公式が Oracle Container Registry で配布している **Oracle Database 23ai Free** の
コンテナイメージを使います。

- イメージ名: `container-registry.oracle.com/database/free:latest`
- 無料で利用できますが、初回のみ Oracle アカウントでのライセンス同意が必要です。

1. https://container-registry.oracle.com にアクセスし、Oracleアカウントでログインして
   `Database` → `free` のライセンス条項に同意する（初回のみ必要）
2. ローカルでもログインする
   ```bash
   docker login container-registry.oracle.com
   ```
3. イメージを取得する
   ```bash
   docker pull container-registry.oracle.com/database/free:latest
   ```

> **Oracleアカウントの登録が面倒な場合**
> コミュニティイメージ `gvenzl/oracle-free:23-slim` を使うと、アカウント登録なしですぐに
> 同等の環境を試せます（学習用途でよく使われる非公式イメージです）。使い方はほぼ同じで、
> パスワード指定用の環境変数だけ `ORACLE_PWD` ではなく `ORACLE_PASSWORD` になります。
> ```bash
> docker pull gvenzl/oracle-free:23-slim
> ```

### docker run で起動する

パスワードは自分の任意の値に変更してください（8文字以上、英大文字・小文字・数字を含む値を推奨）。
`-v` で指定する名前付きボリュームにより、コンテナを削除してもデータを永続化できます。

```bash
docker run -d \
  --name oracle23ai \
  -p 1521:1521 \
  -p 5500:5500 \
  -e ORACLE_PWD=YourPassword123 \
  -v oracle23ai-data:/opt/oracle/oradata \
  container-registry.oracle.com/database/free:latest
```

各オプションの意味:

| オプション | 意味 |
|---|---|
| `-d` | バックグラウンドで起動（デタッチモード） |
| `--name oracle23ai` | コンテナ名。以降のコマンドでこの名前を使う |
| `-p 1521:1521` | DBリスナーのポート。ホストの1521番をコンテナの1521番に接続する |
| `-p 5500:5500` | Enterprise Manager Express（Web管理画面）用ポート。不要なら省略可 |
| `-e ORACLE_PWD=...` | SYS / SYSTEM など管理ユーザーの初期パスワード |
| `-v oracle23ai-data:/opt/oracle/oradata` | データファイルを永続化する名前付きボリューム |

（`gvenzl/oracle-free:23-slim` を使う場合は最後の行を該当イメージ名に、`ORACLE_PWD` を
`ORACLE_PASSWORD` に読み替えてください。）

### 起動完了の確認方法

初回起動はデータベースの初期化が走るため数分〜10分程度かかります。ログで完了を確認してください。

```bash
docker logs -f oracle23ai
```

以下のようなメッセージが表示されたら準備完了です（Ctrl+Cでログ表示を終了できます）。

```
#########################
DATABASE IS READY TO USE!
#########################
```

現在の状態は `docker ps` でも確認できます。`STATUS` 列が `Up ...` になっていれば
コンテナ自体は起動していますが、上記メッセージが出るまではDBへの接続はまだできません。

### よくあるトラブル

| 症状 | 原因 / 対処 |
|---|---|
| `port is already allocated` | 別のプロセスがポート1521を使用中。`-p 15211:1521` のようにホスト側のポート番号を変えて起動し、接続時も `1521`の代わりに`15211`を使う |
| ログに `DATABASE IS READY TO USE!` がなかなか出ない | 初回起動はイメージも大きく初期化に時間がかかる（5〜10分程度）。`docker logs -f oracle23ai` を流したまま気長に待つ。プログレスが全く進まない場合はDockerのメモリ割り当てを見直す |
| コンテナが起動直後に落ちる / `Out of memory` 系のログが出る | メモリ不足が原因のことが多い。Docker Desktopの「Settings → Resources」でメモリを最低2GB、できれば4GB以上に増やしてから `docker start oracle23ai` で再起動する |
| `ORA-12541: TNS:no listener` | まだ起動途中の可能性が高い。`docker logs -f oracle23ai` で準備完了メッセージを待ってから再接続する |
| `docker: command not found` | Docker Desktopがインストールされているか、起動しているか確認する |

---

## 2. SQL Developer から接続する

### SQL Developer のダウンロード

Oracle SQL Developer はOracle公式が無料で配布しているGUIクライアントです。以下からダウンロードできます。

- https://www.oracle.com/database/sqldeveloper/technologies/download/

OS（Windows / macOS / Linux）に合った版を選び、インストール（またはZIP展開）してください。
Java同梱版を選べば、事前にJavaを別途インストールする必要はありません。

### 新規接続を作成する

1. SQL Developerを起動し、左側の「接続」ペインで緑色の「＋」（新規接続）をクリック
2. 以下の項目を入力する

| 項目 | 値 |
|---|---|
| 接続名 | 任意（例: `oracle23ai`） |
| ユーザー名 | `system` |
| パスワード | Docker起動時に `ORACLE_PWD`（または`ORACLE_PASSWORD`）で指定した値 |
| 接続タイプ | Basic |
| ホスト名 | `localhost` |
| ポート | `1521` |
| ロール（接続の種別） | 「サービス名」を選択し、`FREEPDB1` と入力（SIDではなくサービス名） |

3. 「テスト」ボタンを押して接続確認する（画面下部に `成功` / `Success` と表示されればOK）
4. 「保存」を押してから「接続」を押す

### クエリを実行する

1. 接続に成功すると、その接続名のタブでSQLワークシートが開く
   （開かない場合は接続を右クリック→「SQLワークシートを開く」）
2. ワークシートに以下のようなSQLを入力する

   ```sql
   SELECT * FROM dual;
   ```

3. `Ctrl+Enter`（または`F9`：文の実行）または `F5`（スクリプトの実行）でクエリを実行する
   - `Ctrl+Enter` / `F9` : カーソル位置の1文だけを実行し、結果をグリッド表示する
   - `F5` : ワークシート内の複数文をまとめて実行し、結果をログ形式で表示する

### オブジェクトツリーでテーブル構造を確認する

1. 左側の接続ツリーで対象の接続を展開する
2. `テーブル`（Tables）を展開すると、そのユーザーが持つテーブル一覧が表示される
   （最初は空、または`system`スキーマの内部テーブルのみ）
3. テーブル名をダブルクリックすると、列定義・制約・データなどをタブ形式で確認できる

学習用ユーザー作成後に `schema_oracle.sql` を流し込むと、ここに
`CUSTOMERS` / `PRODUCTS` / `ORDERS` / `ORDER_ITEMS` が表示されるようになります。

---

## 3. 学習用ユーザーを作成する（任意・推奨）

`system` で直接作業せず、練習専用のスキーマ（ユーザー）を作るのが本番に近い作法です。

上記の手順でSQL Developerから `system` ユーザーとして接続した状態で、SQLワークシートに
以下を入力して実行します（`F5`推奨。パスワードは自分の任意の値に変更してください）。

```sql
CREATE USER practice IDENTIFIED BY YourPassword123;
GRANT CONNECT, RESOURCE, UNLIMITED TABLESPACE TO practice;
```

実行後、SQL Developerで新しい接続をもう1つ作成します（手順は上記「新規接続を作成する」と同じ）。

| 項目 | 値 |
|---|---|
| 接続名 | 任意（例: `oracle23ai-practice`） |
| ユーザー名 | `practice` |
| パスワード | 上記で指定した値 |
| ホスト名 / ポート / サービス名 | `localhost` / `1521` / `FREEPDB1`（`system`のときと同じ） |

以降はこの `practice` 接続で作業します。

### schema_oracle.sql / seed_oracle.sql を流し込む（SQL Developerの場合）

1. `practice` 接続でSQLワークシートを開く
2. ワークシート右クリック、または「ファイル」メニューから `schema_oracle.sql` を開く
3. `F5`（スクリプトの実行）でDDLを流す
4. 同様に `seed_oracle.sql` を開き、`F5`で実行する（末尾の`COMMIT;`まで実行されることを確認）
5. 左のオブジェクトツリーを更新（テーブルを右クリック→「更新」）すると
   `CUSTOMERS` などのテーブルが表示される

```sql
-- 動作確認クエリ（practice接続のワークシートで実行）
SELECT COUNT(*) FROM customers;
```

---

## 4. CLI（sqlplus）で接続したい人向け

GUIを使わず、コンテナに同梱されている `sqlplus` をそのまま使うこともできます
（クライアントの別途インストール不要）。

```bash
docker exec -it oracle23ai sqlplus system/YourPassword123@//localhost:1521/FREEPDB1
```

`practice`ユーザー作成後は、そのユーザーで接続します。

```bash
docker exec -it oracle23ai sqlplus practice/YourPassword123@//localhost:1521/FREEPDB1
```

### schema_oracle.sql / seed_oracle.sql を流し込む（sqlplusの場合）

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

### GUIとCLIの使い分け

普段の学習・データ確認はSQL Developer（オブジェクトツリーやグリッド表示で見やすい）、
スクリプトの一括実行や自動化・確認作業は sqlplus（コマンド一発で完結する）が向いています。
どちらで接続してもDBの中身は同じなので、気分や作業内容に応じて使い分けてください。

---

## 自動化スクリプトで一気にセットアップする

上記1〜4の手順（コンテナ起動 → 起動待ち → 学習用ユーザー作成 → schema/seed流し込み）を
まとめて実行するスクリプトが `setup_oracle.sh` です。

```bash
cd sql-practice/oracle
chmod +x setup_oracle.sh
./setup_oracle.sh
```

デフォルトではコンテナ名 `oracle23ai`、パスワード `YourPassword123`、ユーザー名 `practice`
でセットアップします。変更したい場合は環境変数で上書きできます。

```bash
ORACLE_PWD=MyOwnPassword1 HOST_PORT=1521 ./setup_oracle.sh
```

実行後はSQL Developerから「新規接続を作成する」の手順で `practice` ユーザーとして接続すれば、
すでにテーブルとデータが入った状態で使い始められます。

---

## 文字化け対策

日本語データ（氏名・都市名など）を扱うため、ホスト側ターミナルの文字コードと
Oracleクライアントの文字コード設定(`NLS_LANG`)を合わせておくと安全です。

```bash
export NLS_LANG=.AL32UTF8
```

（`docker exec`でコンテナ内のsqlplusを直接使う場合は、コンテナ側の文字コードは
23ai Freeのデフォルトで `AL32UTF8` になっているため、通常は追加設定なしで問題ありません。
SQL Developerも通常は自動でUTF-8を検出します。文字化けする場合は
「ツール→設定→環境→エンコーディング」をUTF-8に設定してください。）

## 後片付け

学習が終わったらコンテナを止めて削除できます。`-v`で作成した名前付きボリュームは
残るため、同じ`docker run`コマンドで再作成すればデータも復元されます
（完全にデータも消したい場合は `docker volume rm oracle23ai-data` も実行してください）。

```bash
docker stop oracle23ai
docker rm oracle23ai
```

停止するだけでコンテナを残しておきたい場合は `docker stop oracle23ai` のみ実行し、
再開するときは `docker start oracle23ai` を使ってください。

## うまくいかないとき

- `docker: command not found` → Docker Desktopがインストールされているか、起動しているか確認
- `Error response from daemon: port is already allocated` → 別のプロセスが1521番ポートを使用中。
  `-p 15211:1521` のようにホスト側のポート番号を変えて起動し、接続時も `//localhost:15211/FREEPDB1` とする
- ログに `DATABASE IS READY TO USE!` がなかなか出ない → 初回はイメージのサイズも大きく、
  初期化に5〜10分程度かかることがあるので気長に待つ
- `ORA-12541: TNS:no listener` → まだ起動途中の可能性が高い。`docker logs -f oracle23ai` で
  準備完了メッセージを待ってから再接続する
- SQL Developerの接続テストで失敗する → ホスト名`localhost`・ポート`1521`・サービス名`FREEPDB1`の
  綴りを再確認し、`docker logs -f oracle23ai` で起動完了メッセージが出ているか確認する
- メモリ不足でコンテナが落ちる → Docker Desktopの「Settings → Resources」でメモリ割り当てを増やす
