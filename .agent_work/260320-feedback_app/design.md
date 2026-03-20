# 技術設計 — イベントアンケートアプリ（感想一言）

## アーキテクチャ概要

```
┌─────────────────────┐     HTTPS      ┌──────────────────────────┐      SDK       ┌─────────────┐
│  React SPA (Vite)   │ ──────────────► │  Azure Functions (.NET9) │ ──────────────► │  Cosmos DB  │
│  Azure Static Web   │ ◄────────────── │  isolated worker         │ ◄────────────── │  (NoSQL)    │
│  Apps               │     JSON        │  func-vsch-survey        │     Documents   │             │
└─────────────────────┘                 └──────────────────────────┘                 └─────────────┘
  frontend/                               Functions/Survey/                           ghdevdays
  dist/ → SWA deploy                      publish/ → Functions deploy                 └─ survey (PK: /date)
```

## バックエンド設計（Functions/Survey）

### ディレクトリ構成

```
Functions/Survey/
├── Survey.csproj
├── Program.cs                  # DI設定（CosmosClient, Services）
├── host.json                   # Functions ホスト設定
├── local.settings.json         # ローカル開発設定
├── Models/
│   ├── Feedback.cs             # Cosmos DBドキュメントモデル
│   ├── FeedbackRequest.cs      # リクエストDTO
│   └── FeedbackResponse.cs     # レスポンスDTO（成功/エラー共通）
├── Services/
│   ├── IFeedbackService.cs     # サービスインターフェース
│   └── FeedbackService.cs      # Cosmos DB操作（SDK直利用）
└── Functions/
    ├── PostFeedback.cs         # POST /api/feedbacks
    └── GetFeedbacks.cs         # GET /api/feedbacks
```

### 責務分離

| レイヤー | 責務 |
|---------|------|
| **Function** | HTTPリクエスト/レスポンス処理、入力バリデーション、ログ開始点 |
| **Service** | Cosmos DB書き込み・読み取り、永続化ルール |
| **DTO** | リクエスト/レスポンスの型定義、シリアライズ制御 |
| **Program.cs** | DI登録、CosmosClient初期化、設定読み込み |

### データモデル

#### Feedback.cs（Cosmos DBドキュメント）

```csharp
public class Feedback
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("comment")]
    public string Comment { get; set; } = string.Empty;

    [JsonPropertyName("date")]
    public string Date { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd");

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

#### FeedbackRequest.cs（リクエストDTO）

```csharp
public class FeedbackRequest
{
    [JsonPropertyName("comment")]
    public string? Comment { get; set; }
}
```

#### FeedbackResponse.cs（レスポンスDTO）

```csharp
public class FeedbackResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Id { get; set; }

    [JsonPropertyName("createdAt")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("error")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Error { get; set; }

    [JsonPropertyName("feedbacks")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<Feedback>? Feedbacks { get; set; }
}
```

### API設計

#### POST /api/feedbacks

| 項目 | 値 |
|------|-----|
| メソッド | POST |
| ルート | `feedbacks` |
| 認証 | Anonymous |
| Content-Type | application/json |

**リクエスト**:
```json
{ "comment": "とても楽しかったです！" }
```

**成功レスポンス（201）**:
```json
{ "success": true, "id": "550e8400-e29b-41d4-a716-446655440000", "createdAt": "2026-03-20T10:30:00Z" }
```

**バリデーションエラー（400）**:
```json
{ "success": false, "error": "comment は1文字以上2000文字以内で入力してください" }
```

**サーバーエラー（500）**:
```json
{ "success": false, "error": "Internal Server Error" }
```

#### GET /api/feedbacks

| 項目 | 値 |
|------|-----|
| メソッド | GET |
| ルート | `feedbacks` |
| 認証 | Anonymous |

**成功レスポンス（200）**:
```json
{
  "success": true,
  "feedbacks": [
    { "id": "...", "comment": "楽しかった！", "createdAt": "2026-03-20T10:30:00Z" },
    { "id": "...", "comment": "また参加したい", "createdAt": "2026-03-20T10:25:00Z" }
  ]
}
```

### バリデーションルール

| チェック | 条件 | エラーメッセージ |
|---------|------|----------------|
| null/空チェック | `comment` が null、空文字、空白のみ | `comment は必須です` |
| 文字数上限 | `comment` が2001文字以上 | `comment は2000文字以内で入力してください` |
| ボディ不正 | リクエストボディがnullまたはJSON不正 | `リクエストボディが不正です` |

### エラーハンドリング方針

| 例外 | HTTP ステータス | 対応 |
|------|----------------|------|
| バリデーション失敗 | 400 | エラーメッセージを返却 |
| `CosmosException` (4xx) | 500 | ログ記録 + 汎用エラー返却 |
| `CosmosException` (429) | 500 | SDK組み込み再試行後、なお失敗なら汎用エラー |
| `CosmosException` (5xx) | 500 | ログ記録 + 汎用エラー返却 |
| その他の例外 | 500 | ログ記録 + 汎用エラー返却 |

### 認証・接続設定

| 環境 | Cosmos DB接続方式 | 設定箇所 |
|------|------------------|---------|
| ローカル | Cosmos DBエミュレーター（接続文字列） | `local.settings.json` の `CosmosDb:ConnectionString` |
| Azure | Managed Identity + RBAC | アプリ設定の `CosmosDb:AccountEndpoint` |

設定キー:

| キー | 用途 |
|------|------|
| `CosmosDb:ConnectionString` | ローカル開発用接続文字列 |
| `CosmosDb:AccountEndpoint` | Azure本番用エンドポイント |
| `CosmosDb:DatabaseName` | データベース名（`ghdevdays`） |
| `CosmosDb:ContainerName` | コンテナ名（`survey`） |

### Cosmos DB書き込み方式

**SDK直利用を採用**（output binding不使用）

理由:
- バリデーション後の条件分岐がある
- エラーハンドリング（CosmosException）を制御したい
- レスポンスに書き込み結果（id, createdAt）を含める必要がある

### NuGetパッケージ

| パッケージ | 用途 |
|-----------|------|
| `Microsoft.Azure.Functions.Worker` | isolated workerランタイム |
| `Microsoft.Azure.Functions.Worker.Extensions.Http` | HTTPトリガー |
| `Microsoft.Azure.Functions.Worker.Extensions.Http.AspNetCore` | ASP.NET Core統合 |
| `Microsoft.Azure.Functions.Worker.Sdk` | ビルドSDK |
| `Microsoft.ApplicationInsights.WorkerService` | 診断・トレース |
| `Microsoft.Azure.Cosmos` | Cosmos DB SDK |

## フロントエンド設計（frontend/）

### ディレクトリ構成

```
frontend/
├── package.json
├── vite.config.ts
├── tsconfig.json
├── index.html
├── .env.development             # VITE_API_BASE_URL=http://localhost:7071/api
├── src/
│   ├── main.tsx
│   ├── App.tsx                   # メインページ（フォーム + 一覧）
│   ├── App.css
│   ├── types/
│   │   └── Feedback.ts          # 型定義
│   ├── api/
│   │   └── feedbackApi.ts       # APIクライアント（fetch）
│   └── components/
│       ├── FeedbackForm.tsx      # 送信フォーム
│       └── FeedbackList.tsx      # 一覧表示
└── public/
```

### コンポーネント設計

| コンポーネント | 責務 |
|--------------|------|
| `App` | ページレイアウト、FeedbackForm と FeedbackList を配置。一覧再読込のトリガー管理 |
| `FeedbackForm` | テキスト入力（textarea）、文字数カウント表示、送信ボタン、送信結果フィードバック |
| `FeedbackList` | APIから取得した感想一覧を `createdAt` 降順で表示 |

### API クライアント

```typescript
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

// POST /api/feedbacks
export async function postFeedback(comment: string): Promise<FeedbackResponse>

// GET /api/feedbacks
export async function getFeedbacks(): Promise<FeedbackListResponse>
```

### 環境変数

| 変数名 | ローカル | 本番（CI/CD） |
|--------|---------|--------------|
| `VITE_API_BASE_URL` | `http://localhost:7071/api` | `https://func-vsch-survey-ftasbratdygpbndb.japaneast-01.azurewebsites.net/api` |

※ 本番値はCI/CDワークフローの `app_build_command` で注入済み

## CI/CD（既存ワークフロー — 変更不要）

### フロントエンド

- ワークフロー: `.github/workflows/azure-static-web-apps-purple-sea-093b8b000.yml`
- トリガー: `frontend/**` 変更時
- ビルド: `VITE_API_BASE_URL=<本番URL> npm run build`
- 出力: `dist/`
- デプロイ先: Azure Static Web Apps

### バックエンド

- ワークフロー: `.github/workflows/deploy-function.yml`
- トリガー: `Functions/**` 変更時
- ビルド: `dotnet build --configuration Release` + `dotnet publish`
- 出力: `publish/release.zip`
- デプロイ先: Azure Functions (`func-vsch-survey`)

## 決定記録

### Decision - 2026-03-20

**Decision**: Cosmos DB SDK直利用（output binding不使用）
**Context**: HTTPレスポンスに書き込み結果を含める、バリデーション後の条件分岐、CosmosException制御が必要
**Options**: (A) SDK直利用 (B) output binding
**Rationale**: 指針（azure-functions-cosmos.instructions.md）でもSDK直利用を優先と明記。エラー制御の柔軟性が高い
**Impact**: Service層にCosmosClient依存が集約される

### Decision - 2026-03-20

**Decision**: Partition Key を `/date`（既存コンテナの設定に合わせる）
**Context**: Azure上の既存コンテナ `cosmos-ghdevdays-260320` / `ghdevdays` / `survey` のPKが `/date` に設定済み
**Options**: 既存コンテナの設定に従う（PK変更は不可）
**Rationale**: Cosmos DBのPKはコンテナ作成後に変更不可。日付ベースのパーティションにより同日の感想が同パーティションに格納される。GET全件はクロスパーティションクエリになるが小規模なら許容
**Impact**: モデルに `date` フィールド（`yyyy-MM-dd`形式）を追加。`CreateItemAsync` で `PartitionKey(date)` を明示指定
