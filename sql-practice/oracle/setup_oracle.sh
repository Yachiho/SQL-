#!/bin/bash
# ============================================================
# setup_oracle.sh
#
# Oracle Database 23ai Free をDockerで起動し、
# 学習用ユーザーを作成した上で schema_oracle.sql / seed_oracle.sql を
# 流し込むまでを自動化するスクリプトです。
# 詳しい手順や用語の説明は README.md を参照してください。
#
# 使い方:
#   ./setup_oracle.sh
#
# 環境変数で上書き可能な設定（省略時は右側のデフォルト値を使用）:
#   IMAGE          container-registry.oracle.com/database/free:latest
#   CONTAINER_NAME oracle23ai
#   ORACLE_PWD     YourPassword123   (system/sysのパスワード)
#   PRACTICE_USER  practice
#   PRACTICE_PWD   ${ORACLE_PWD}と同じ値
#   HOST_PORT      1521
#   VOLUME_NAME    oracle23ai-data
#   SERVICE_NAME   FREEPDB1
# ============================================================

set -e

IMAGE="${IMAGE:-container-registry.oracle.com/database/free:latest}"
CONTAINER_NAME="${CONTAINER_NAME:-oracle23ai}"
ORACLE_PWD="${ORACLE_PWD:-YourPassword123}"
PRACTICE_USER="${PRACTICE_USER:-practice}"
PRACTICE_PWD="${PRACTICE_PWD:-$ORACLE_PWD}"
HOST_PORT="${HOST_PORT:-1521}"
VOLUME_NAME="${VOLUME_NAME:-oracle23ai-data}"
SERVICE_NAME="${SERVICE_NAME:-FREEPDB1}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "=== [1/5] コンテナの状態を確認しています ==="
if docker ps --format '{{.Names}}' | grep -qx "${CONTAINER_NAME}"; then
    # 同名コンテナが既に起動中の場合はそのまま再利用する
    echo "コンテナ '${CONTAINER_NAME}' は既に起動中です。再利用します。"
elif docker ps -a --format '{{.Names}}' | grep -qx "${CONTAINER_NAME}"; then
    # 同名コンテナが存在するが停止中の場合は起動するだけ（作り直さない）
    echo "コンテナ '${CONTAINER_NAME}' は存在しますが停止中です。起動します。"
    docker start "${CONTAINER_NAME}"
else
    # 同名コンテナが存在しない場合のみ新規作成する
    # ポート・パスワード・イメージを変更したい場合を除いて、
    # 作り直したい場合は先に `docker rm -f ${CONTAINER_NAME}` を実行してから
    # 本スクリプトを再実行してください（データも消えるので注意）。
    echo "=== [2/5] コンテナを新規作成します（初回は数分かかります） ==="
    docker run -d \
        --name "${CONTAINER_NAME}" \
        -p "${HOST_PORT}:1521" \
        -e ORACLE_PWD="${ORACLE_PWD}" \
        -v "${VOLUME_NAME}:/opt/oracle/oradata" \
        "${IMAGE}"
fi

echo "=== [3/5] データベースの起動完了を待っています ==="
echo "（初回はDBの初期化が走るため、数分〜10分程度かかることがあります）"
MAX_WAIT_SECONDS=1800
ELAPSED=0
until docker logs "${CONTAINER_NAME}" 2>&1 | grep -q "DATABASE IS READY TO USE"; do
    if [ "${ELAPSED}" -ge "${MAX_WAIT_SECONDS}" ]; then
        echo "エラー: ${MAX_WAIT_SECONDS}秒待っても起動完了メッセージが確認できませんでした。"
        echo "'docker logs ${CONTAINER_NAME}' でログを確認してください。"
        exit 1
    fi
    sleep 5
    ELAPSED=$((ELAPSED + 5))
    echo "  ... 起動待ち（${ELAPSED}秒経過）"
done
echo "データベースの起動が完了しました。"

echo "=== [4/5] 学習用ユーザー '${PRACTICE_USER}' を作成します ==="
# 既に同名ユーザーが存在する場合（再実行時など）はエラーにせずスキップする
docker exec -i "${CONTAINER_NAME}" sqlplus -s "system/${ORACLE_PWD}@//localhost:1521/${SERVICE_NAME}" <<SQL
SET ECHO OFF
WHENEVER SQLERROR CONTINUE
BEGIN
    EXECUTE IMMEDIATE 'CREATE USER ${PRACTICE_USER} IDENTIFIED BY "${PRACTICE_PWD}"';
    DBMS_OUTPUT.PUT_LINE('ユーザー ${PRACTICE_USER} を作成しました');
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE = -1920 THEN
            DBMS_OUTPUT.PUT_LINE('ユーザー ${PRACTICE_USER} は既に存在するため作成をスキップしました');
        ELSE
            RAISE;
        END IF;
END;
/
GRANT CONNECT, RESOURCE, UNLIMITED TABLESPACE TO ${PRACTICE_USER};
EXIT;
SQL

echo "=== [5/5] schema_oracle.sql / seed_oracle.sql を流し込みます ==="
# 2回目以降の実行では「テーブルが既に存在する」「主キーが重複している」等の
# エラーが出ますが、初回実行が正常に終わっていれば無視して問題ありません。
# まっさらな状態からやり直したい場合は、先に system 接続で対象ユーザーを
# 作り直す（DROP USER ${PRACTICE_USER} CASCADE; の後に本スクリプトを再実行）などしてください。
docker cp "${SCRIPT_DIR}/schema_oracle.sql" "${CONTAINER_NAME}:/tmp/schema_oracle.sql"
docker cp "${SCRIPT_DIR}/seed_oracle.sql"   "${CONTAINER_NAME}:/tmp/seed_oracle.sql"

docker exec -i "${CONTAINER_NAME}" sqlplus -s "${PRACTICE_USER}/${PRACTICE_PWD}@//localhost:1521/${SERVICE_NAME}" <<SQL
SET ECHO ON
WHENEVER SQLERROR CONTINUE
@/tmp/schema_oracle.sql
@/tmp/seed_oracle.sql
EXIT;
SQL

echo ""
echo "=== セットアップ完了 ==="
echo "接続情報（SQL Developer / sqlplus 共通）:"
echo "  ホスト     : localhost"
echo "  ポート     : ${HOST_PORT}"
echo "  サービス名 : ${SERVICE_NAME}"
echo "  ユーザー   : ${PRACTICE_USER}"
echo "  パスワード : ${PRACTICE_PWD}"
echo ""
echo "詳しい接続手順は README.md の「SQL Developer から接続する」を参照してください。"
