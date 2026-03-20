# 要件定義 — イベントアンケートアプリ（感想一言）

## 概要

イベント参加者が「感想を一言」送信し、送信済みの感想一覧を閲覧できるWebアプリケーション。

- **フロントエンド**: React + Vite + TypeScript（Azure Static Web Apps）
- **バックエンド**: .NET 9 Azure Functions isolated worker（Azure Functions）
- **データベース**: Azure Cosmos DB (NoSQL)

## 要件一覧（EARS記法）

### REQ-001: 感想送信（イベント駆動）

ユーザーが感想送信フォームで送信ボタンを押したとき、システムは感想テキストをバックエンドAPIへ送信し、Cosmos DBに保存する。

- 感想テキストは `comment` フィールドとして送信する
- `comment` は1文字以上2000文字以内の文字列であること
- 保存時に `id`（GUID）と `createdAt`（UTC日時）をサーバー側で自動付与する
- 保存成功時、システムは `201 Created` と `{ success: true, id, createdAt }` を返す

### REQ-002: 感想送信バリデーション（異常系）

`comment` が空文字・null・未指定、または2000文字を超過した場合、システムは `400 Bad Request` と `{ success: false, error: "<エラー内容>" }` を返し、Cosmos DBには書き込まない。

### REQ-003: サーバーエラー時の応答（異常系）

Cosmos DBへの書き込みまたは読み取りで予期しない例外が発生した場合、システムは `500 Internal Server Error` と `{ success: false, error: "Internal Server Error" }` を返す。例外の詳細はクライアントに公開しない。

### REQ-004: 感想一覧取得（イベント駆動）

ユーザーが一覧画面を表示したとき、システムはバックエンドAPIから全件の感想を `createdAt` 降順で取得し、画面に表示する。

- レスポンス形式: `{ success: true, feedbacks: [{ id, comment, createdAt }] }`
- ステータス: `200 OK`

### REQ-005: フロントエンド送信フィードバック（イベント駆動）

送信ボタンを押したとき、システムは以下のフィードバックを表示する:

- 送信成功時: 成功メッセージを表示し、入力欄をクリアし、一覧を再読込する
- 送信失敗時: エラーメッセージを表示する（入力内容は保持する）

### REQ-006: フロントエンド文字数制限（状態駆動）

感想入力欄に入力中のとき、システムは残り文字数を表示する。2000文字を超過した場合、送信ボタンを無効化する。

### REQ-007: APIルーティング（普遍）

システムは以下のAPIエンドポイントを提供する:

| メソッド | ルート | 機能 |
|---------|--------|------|
| POST | `/api/feedbacks` | 感想登録 |
| GET | `/api/feedbacks` | 感想一覧取得 |

認証レベルは `Anonymous` とする。

### REQ-008: データモデル（普遍）

Cosmos DBドキュメントは以下の構造を持つ:

| フィールド | 型 | 必須 | 説明 |
|-----------|-----|------|------|
| `id` | string | ○ | GUID（サーバー自動生成） |
| `comment` | string | ○ | 感想テキスト（1〜2000文字） |
| `date` | string | ○ | 日付（`yyyy-MM-dd`形式、サーバー付与、Partition Key） |
| `createdAt` | DateTime | ○ | 登録日時（UTC、サーバー付与） |

- **Account**: `cosmos-ghdevdays-260320`
- **Database**: `ghdevdays`
- **Container**: `survey`
- **Partition Key**: `/date`

### REQ-009: CORS（普遍）

システムはフロントエンドのオリジンからのリクエストを許可するCORS設定を行う。

- ローカル開発: `http://localhost:5173`
- 本番: Azure Static Web Apps のURL

### REQ-010: セキュリティ（普遍）

- 接続文字列・キー・シークレットをソースコードに埋め込まない
- 本番環境では Managed Identity + RBAC で Cosmos DB にアクセスする
- ローカル開発では Cosmos DB エミュレーターを使用する
- ログに個人情報やシークレットを出力しない

## 依存関係・制約

| 区分 | 内容 |
|------|------|
| フロントエンド | React + Vite + TypeScript、`frontend/` ディレクトリ |
| バックエンド | .NET 9 Azure Functions isolated worker、`Functions/Survey/` ディレクトリ |
| CI/CD（フロント） | `.github/workflows/azure-static-web-apps-purple-sea-093b8b000.yml`（output: `dist`） |
| CI/CD（バック） | `.github/workflows/deploy-function.yml`（source: `./Functions/Survey`） |
| Cosmos DB SDK | `Microsoft.Azure.Cosmos`（output binding ではなくSDK直利用） |
| API Base URL（本番） | `https://func-vsch-survey-ftasbratdygpbndb.japaneast-01.azurewebsites.net/api` |
| API Base URL（ローカル） | `http://localhost:7071/api` |

## データフロー

```
[ブラウザ] → POST /api/feedbacks → [Azure Functions] → CreateItemAsync → [Cosmos DB]
[ブラウザ] → GET  /api/feedbacks → [Azure Functions] → Query          → [Cosmos DB]
```

## エッジケース・障害シナリオ

| シナリオ | 期待動作 |
|---------|---------|
| comment が空文字 | 400 Bad Request |
| comment が null / 未指定 | 400 Bad Request |
| comment が2001文字 | 400 Bad Request |
| comment が2000文字ちょうど | 201 Created（正常） |
| comment が1文字 | 201 Created（正常） |
| リクエストボディが空 / JSON不正 | 400 Bad Request |
| Cosmos DB 接続エラー | 500 Internal Server Error |
| Cosmos DB スロットリング (429) | 500 Internal Server Error（SDK再試行後） |
| 同時大量送信 | 各リクエスト独立処理（id はGUIDで衝突しない） |

## 信頼度スコア

**92%（高）**

根拠:
- 技術スタック（.NET 9 isolated worker + Cosmos DB SDK + React/Vite）は十分に確立されている
- データモデルが極めてシンプル（3フィールド）
- 既存CI/CDワークフローが `Functions/Survey` と `frontend/` にそのまま合致する
- PK設計(`/id`)は小規模イベントで問題ない
- 唯一の不確実性: 大規模イベント時のGET全件取得のパフォーマンス（Phase 1では許容、必要に応じてページネーション追加）
