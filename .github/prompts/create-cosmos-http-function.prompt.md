---
description: 'Functions/Survey 向けに、HTTP トリガーで受けて Cosmos DB に保存する Azure Functions の雛形を生成する'
argument-hint: 'functionName=..., route=..., authLevel=..., dbName=..., containerName=..., partitionKey=..., inputDto=...'
---

[共通方針](../copilot-instructions.md) と [仕様駆動ワークフロー](../instructions/spec-driven-workflow.instructions.md) を前提に、このリポジトリの `Functions/Survey` 配下へ **C# / Azure Functions isolated worker** の HTTP トリガー関数を追加・更新してください。

まず実装に入る前に、次の不足情報を確認してください。未指定・曖昧な項目があれば、簡潔な箇条書きで質問してから進めてください。

- 関数名
- `route`
- 認証レベル（`Anonymous` / `Function` / `Admin` など）
- Cosmos DB の Database 名
- Cosmos DB の Container 名
- partition key
- 入力 DTO の項目定義

実装時の前提:

- 対象は `Functions/Survey` 配下のみ
- **Azure Functions isolated worker** を前提にする
- Cosmos DB は **SDK 直利用**（バインディングではなくクライアント利用）にする
- DI を使って依存性を `Program.cs` に登録する
- 入力 DTO のバリデーションを行う
- 明示的なエラーハンドリングを入れる
- **シークレットをコードに埋め込まない**。接続情報や設定値は環境変数 / アプリ設定 / `local.settings.json` のキー名で扱う

雛形生成・更新時は、必要に応じて次の観点を含めてください。

- Function クラス
- 入力 DTO
- Cosmos DB 保存用サービス
- 設定名（例: Database / Container / Connection 関連のキー名）
- `Program.cs` の DI 登録

作業の進め方:

1. 不足情報を確認する
2. 想定する変更ファイルと役割を短く示す
3. 最小差分で `Functions/Survey` 配下の実装を追加・更新する
4. 変更内容、前提設定、未解決事項を簡潔にまとめる

追加の指示がない限り、既存方針と矛盾しない命名・構成を優先してください。