#!/bin/bash
# 使い方: ./check.sh [DAY番号(デフォルト: 01)]
# answers/dayXX.sql の各解答を practice.db 上で実行し、
# solutions/dayXX.sql の実行結果（データ）とだけ比較して正解/不正解を判定します。
# 模範解答のSQL文そのものは表示しません。
set -e

DAY="${1:-01}"
DB="practice.db"
ANSWERS="answers/day${DAY}.sql"
SOLUTIONS="solutions/day${DAY}.sql"

if [ ! -f "$ANSWERS" ]; then
  echo "エラー: $ANSWERS が見つかりません"
  exit 1
fi
if [ ! -f "$SOLUTIONS" ]; then
  echo "エラー: $SOLUTIONS が見つかりません"
  exit 1
fi
if [ ! -f "$DB" ]; then
  ./setup.sh
fi

TMPDIR=$(mktemp -d)
trap 'rm -rf "$TMPDIR"' EXIT

split_by_question() {
  local infile="$1"
  local outdir="$2"
  mkdir -p "$outdir"
  awk -v outdir="$outdir" '
    /^-- 問[0-9]+/ {
      if (q != "") {
        print buf > (outdir "/" q ".sql")
        close(outdir "/" q ".sql")
      }
      match($0, /[0-9]+/)
      q = substr($0, RSTART, RLENGTH)
      buf = ""
      next
    }
    { buf = buf $0 "\n" }
    END {
      if (q != "") {
        print buf > (outdir "/" q ".sql")
      }
    }
  ' "$infile"
}

split_by_question "$ANSWERS" "$TMPDIR/answers"
split_by_question "$SOLUTIONS" "$TMPDIR/solutions"

total=0
correct=0

for i in $(ls "$TMPDIR/solutions" 2>/dev/null | sed 's/\.sql$//' | sort -n); do
  s="$TMPDIR/solutions/$i.sql"
  a="$TMPDIR/answers/$i.sql"
  total=$((total + 1))

  if [ ! -f "$a" ] || ! grep -q '[^[:space:]]' "$a"; then
    echo "問$i: 未回答"
    echo
    continue
  fi

  actual=$(sqlite3 "$DB" < "$a" 2>&1) || true
  expected=$(sqlite3 "$DB" < "$s" 2>&1)

  if [ "$actual" = "$expected" ]; then
    echo "問$i: 正解"
    correct=$((correct + 1))
  else
    echo "問$i: 不正解"
    echo "  [あなたの結果]"
    echo "$actual" | sed 's/^/    /'
    echo "  [期待される結果]"
    echo "$expected" | sed 's/^/    /'
  fi
  echo
done

echo "======================================"
echo "結果: ${correct} / ${total} 問正解"
