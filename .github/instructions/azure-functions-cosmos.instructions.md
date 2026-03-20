---
name: 'Azure Functions + Cosmos DB (Survey)'
description: 'Functions/Survey 向けの .NET isolated worker / HTTP / Cosmos DB 実装指針'
applyTo: 'Functions/Survey/**/*.{cs,csproj,json}'
---

# Azure Functions + Cosmos DB 実装指針

- `Functions/Survey` の .NET Azure Functions は **isolated worker** 前提で実装する。`Microsoft.Azure.WebJobs.*` や in-process 前提のコードは提案しない。
- HTTP trigger は **リクエスト DTO / レスポンス DTO / エラー DTO** を明示し、成功・失敗のレスポンス形と HTTP ステータスを先に固める。`4xx` と `5xx` は分けて返す。
- 入力値は Function 入口で検証する。必須項目、文字数、列挙値、配列件数を明示し、バリデーション失敗は仕様に沿って `400` または `422` を返す。
- Cosmos DB への書き込みは **`Microsoft.Azure.Cosmos` SDK の直利用を優先**し、`CosmosClient` / `Container` を使う。**Cosmos DB output binding は単純な単発書き込みでのみ検討**し、基本推奨は SDK 直利用とする。
- isolated worker では、Cosmos DB の **入力**に SDK 型を使えるが、**出力・保存**は output binding より SDK クライアント直接利用を優先する。
- 永続化処理は Function 本体に直書きしない。Function は HTTP 入出力とバリデーションに寄せ、保存処理はサービスへ分離する。
- `CosmosClient` は DI で登録して再利用する。毎回 new しない。Function / Service / 設定解決を疎結合に保つ。
- 保存モデルでは **`id` と partition key を明示**する。`CreateItemAsync` / `UpsertItemAsync` でも partition key を明示する。partition key が未確定なら、コード生成前に必ず確認する。
- 本番接続は **Managed Identity + RBAC** を前提にし、ローカルは `local.settings.json` などの開発用設定を前提にする。**接続文字列・キー・シークレットをコードへ埋め込まない。**
- Managed Identity 利用時、Cosmos DB のデータ操作は Azure RBAC ではなく **Cosmos DB built-in RBAC** のロール割り当てが必要な点を前提にする。管理ロールだけで動く前提にしない。
- アカウント、データベース、コンテナー、接続設定名、partition key のいずれかが未確定なら、推測で固定せず確認してから実装する。
- ログは `ILogger<T>` を使い、入力エラー・外部依存エラー・保存失敗を区別して記録する。個人情報やシークレットはログに出さない。
- Cosmos DB 例外は `CosmosException` を前提に扱い、少なくとも `400` 系、競合、`429`、`5xx` を意識してレスポンス設計と再試行方針を分ける。
- `Program.cs` では診断を意識し、Application Insights / 構造化ログ / 依存関係トレースを有効にできる構成を優先する。
- コンテナー作成やスループット作成をアプリ実行時に暗黙で行う前提は避け、必要ならインフラ側で明示管理する。
