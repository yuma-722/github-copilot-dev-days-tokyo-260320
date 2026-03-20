# 実装計画 — イベントアンケートアプリ（感想一言）

## Phase 1: バックエンド（Functions/Survey）

### Task 1.1: プロジェクト初期化

- **説明**: .NET 9 isolated worker の Azure Functions プロジェクトを `Functions/Survey/` に作成する。NuGetパッケージ（`Microsoft.Azure.Cosmos`, `Microsoft.Azure.Functions.Worker`, `Microsoft.Azure.Functions.Worker.Extensions.Http.AspNetCore`, `Microsoft.ApplicationInsights.WorkerService`）を追加する
- **期待結果**: `dotnet build` が成功する。`Survey.csproj`, `host.json`, `local.settings.json` が生成される
- **依存**: なし
- **対応要件**: —
- **ステータス**: [ ] 未着手

### Task 1.2: データモデル定義

- **説明**: `Models/Feedback.cs`（Cosmos DBドキュメント）、`Models/FeedbackRequest.cs`（リクエストDTO）、`Models/FeedbackResponse.cs`（レスポンスDTO）を作成する
- **期待結果**: 3つのモデルクラスがビルドを通る。`System.Text.Json` のシリアライズ属性が正しく付与されている
- **依存**: Task 1.1
- **対応要件**: REQ-008
- **ステータス**: [ ] 未着手

### Task 1.3: サービス層実装

- **説明**: `Services/IFeedbackService.cs`（インターフェース）と `Services/FeedbackService.cs`（Cosmos DB SDK直利用の実装）を作成する。`CreateAsync`（1件保存）と `GetAllAsync`（全件取得 createdAt降順）を実装する
- **期待結果**: インターフェースと実装クラスがビルドを通る。`CosmosClient` は DI 経由で受け取る
- **依存**: Task 1.2
- **対応要件**: REQ-001, REQ-004
- **ステータス**: [ ] 未着手

### Task 1.4: DI・設定（Program.cs / local.settings.json）

- **説明**: `Program.cs` で `CosmosClient` のDI登録と `IFeedbackService` → `FeedbackService` のDI登録を行う。`local.settings.json` にCosmos DBエミュレーターの接続設定を記載する
- **期待結果**: DI構成が完了し、ビルドが通る。設定キー: `CosmosDb:ConnectionString`, `CosmosDb:DatabaseName`, `CosmosDb:ContainerName`
- **依存**: Task 1.3
- **対応要件**: REQ-010
- **ステータス**: [ ] 未着手

### Task 1.5: POST /api/feedbacks 実装

- **説明**: `Functions/PostFeedback.cs` を作成。HTTPリクエストのデシリアライズ → バリデーション（null/空/2000文字超過）→ `IFeedbackService.CreateAsync` → 201/400/500 レスポンス返却
- **期待結果**: バリデーション成功時は201、失敗時は400、例外時は500を返す
- **依存**: Task 1.3, Task 1.4
- **対応要件**: REQ-001, REQ-002, REQ-003, REQ-007
- **ステータス**: [ ] 未着手

### Task 1.6: GET /api/feedbacks 実装

- **説明**: `Functions/GetFeedbacks.cs` を作成。`IFeedbackService.GetAllAsync` → 200/500 レスポンス返却
- **期待結果**: 全件を createdAt 降順で返す。例外時は500
- **依存**: Task 1.3, Task 1.4
- **対応要件**: REQ-004, REQ-003, REQ-007
- **ステータス**: [ ] 未着手

### Task 1.7: CORS設定

- **説明**: `local.settings.json` に `Host.CORS` で `http://localhost:5173` を追加する。`host.json` は最小限の設定とする
- **期待結果**: ローカル起動時にフロントエンドからのリクエストがCORSエラーにならない
- **依存**: Task 1.1
- **対応要件**: REQ-009
- **ステータス**: [ ] 未着手

### Task 1.8: バックエンドビルド検証

- **説明**: `dotnet build --configuration Release` が成功することを確認する
- **期待結果**: エラー0件でビルド完了
- **依存**: Task 1.5, Task 1.6, Task 1.7
- **対応要件**: —
- **ステータス**: [ ] 未着手

## Phase 2: フロントエンド（frontend/）

### Task 2.1: Vite + React + TypeScript プロジェクト初期化

- **説明**: `frontend/` ディレクトリに Vite + React + TypeScript プロジェクトを作成する。`npm create vite@latest . -- --template react-ts` 相当
- **期待結果**: `npm run build` が `dist/` に出力成功する
- **依存**: なし
- **対応要件**: —
- **ステータス**: [ ] 未着手

### Task 2.2: 型定義・APIクライアント

- **説明**: `src/types/Feedback.ts`（Feedback型, レスポンス型）と `src/api/feedbackApi.ts`（fetch ベース、`VITE_API_BASE_URL` 使用）を作成する
- **期待結果**: `postFeedback(comment)` と `getFeedbacks()` が型安全に利用できる
- **依存**: Task 2.1
- **対応要件**: REQ-001, REQ-004
- **ステータス**: [ ] 未着手

### Task 2.3: FeedbackForm コンポーネント

- **説明**: `src/components/FeedbackForm.tsx` — textarea（2000文字制限）、残り文字数表示、送信ボタン（超過時無効化）、送信結果メッセージ
- **期待結果**: 文字数カウントが動作し、送信成功時に入力クリア+コールバック呼び出し、失敗時にエラー表示
- **依存**: Task 2.2
- **対応要件**: REQ-005, REQ-006
- **ステータス**: [ ] 未着手

### Task 2.4: FeedbackList コンポーネント

- **説明**: `src/components/FeedbackList.tsx` — 感想一覧を createdAt 降順で表示。日時はローカルタイムゾーンに変換して表示
- **期待結果**: APIから取得した一覧が時系列で表示される
- **依存**: Task 2.2
- **対応要件**: REQ-004
- **ステータス**: [ ] 未着手

### Task 2.5: App 統合・環境変数設定

- **説明**: `src/App.tsx` で FeedbackForm と FeedbackList を配置。送信成功時に一覧を再読込する。`.env.development` に `VITE_API_BASE_URL=http://localhost:7071/api` を設定
- **期待結果**: 1ページに送信フォームと一覧が表示され、送信後に一覧が更新される
- **依存**: Task 2.3, Task 2.4
- **対応要件**: REQ-005
- **ステータス**: [ ] 未着手

### Task 2.6: フロントエンドビルド検証

- **説明**: `npm run build` が成功し、`dist/` ディレクトリが生成されることを確認する
- **期待結果**: ビルドエラーなし、`dist/` に HTML/JS/CSS が生成される
- **依存**: Task 2.5
- **対応要件**: —
- **ステータス**: [ ] 未着手

## Phase 3: ドキュメント更新

### Task 3.1: copilot-instructions.md 更新

- **説明**: `.github/copilot-instructions.md` の API 仕様・データモデルセクションを今回の「感想一言」仕様に置き換える
- **期待結果**: API ルート、リクエスト/レスポンス形式、データモデルが最新の仕様と一致する
- **依存**: Task 1.8
- **対応要件**: —
- **ステータス**: [ ] 未着手

### Task 3.2: README.md 更新

- **説明**: プロジェクト概要、セットアップ手順、API仕様をREADMEに記載する
- **期待結果**: 新規開発者がREADMEを読んでローカル環境を構築し、動作確認できる
- **依存**: Task 2.6
- **対応要件**: —
- **ステータス**: [ ] 未着手

## 進捗サマリー

| Phase | タスク数 | 完了 | 残り |
|-------|---------|------|------|
| Phase 1: バックエンド | 8 | 0 | 8 |
| Phase 2: フロントエンド | 6 | 0 | 6 |
| Phase 3: ドキュメント | 2 | 0 | 2 |
| **合計** | **16** | **0** | **16** |
