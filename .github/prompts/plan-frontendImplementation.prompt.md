# Plan: React + Vite フロントエンド実装

## TL;DR
`frontend/` ディレクトリを新規作成し、React + Vite + TypeScript + Tailwind CSS でフィードバック投稿フォーム＆一覧表示のシングルページアプリを構築する。デザインはGitHub Copilotスタイルのダークテーマ。デプロイワークフローはスコープ外。

## 前提・調査結果
- `frontend/` ディレクトリは未作成（ゼロから）
- バックエンドAPI（Azure Functions .NET 9）は実装済み:
  - **POST /api/feedbacks** — `{ comment: string }` (max 2000文字) → `{ success, id, createdAt }` (201) / `{ success, error }` (400/500)
  - **GET /api/feedbacks** — `{ success, feedbacks: [{ id, comment, date, createdAt }] }` (200)
- CORS設定済み: `http://localhost:5173`（Viteデフォルトポート）
- copilot-instructionsの「build」ディレクトリ指定あり → Viteの出力先を `build` に設定
- デプロイワークフロー（GitHub Actions for Azure SWA）はスコープ外

## Steps

### Phase 1: プロジェクト初期化
1. `frontend/` に Vite + React + TypeScript プロジェクトをスキャフォールド
   - `npm create vite@latest frontend -- --template react-ts`
2. Tailwind CSS v4 をインストール・設定
   - `npm install tailwindcss @tailwindcss/vite`
   - vite.config.ts に Tailwind plugin 追加
   - `src/index.css` に `@import "tailwindcss"` 追加
3. `vite.config.ts` を設定:
   - ビルド出力先を `build` に変更（`build.outDir: 'build'`）
   - 開発時のAPIプロキシ設定（`/api` → `http://localhost:7071`）

### Phase 2: 型定義 & API層
4. `src/types/feedback.ts` — TypeScript型定義（バックエンドモデル準拠）
   - `Feedback` 型: `{ id, comment, date, createdAt }`
   - `FeedbackRequest` 型: `{ comment: string }`
   - `FeedbackResponse` 型: `{ success, id?, createdAt?, error?, feedbacks? }`
5. `src/api/feedbacks.ts` — API関数
   - `postFeedback(comment: string): Promise<FeedbackResponse>`
   - `getFeedbacks(): Promise<FeedbackResponse>`
   - ベースURL: `/api`（プロキシを活用）

### Phase 3: UIコンポーネント実装（GitHub Copilotスタイル）
6. **デザインテーマ**:
   - ダークテーマ（背景 `#0d1117` / カード `#161b22` / ボーダー `#30363d`）
   - アクセントカラー: Copilotパープル-ブルー（`#8b5cf6` / `#6366f1`）
   - テキスト: `#e6edf3`（メイン）/ `#7d8590`（サブ）
   - フォント: システムフォント + monospace要素
7. `src/components/Header.tsx` — ヘッダー
   - Copilotスパークルアイコン + タイトル
8. `src/components/FeedbackForm.tsx` — 投稿フォーム
   - textarea（2000文字制限、文字数カウンター付き）
   - 送信ボタン（ローディング状態・成功/エラーフィードバック）
   - バリデーション: 空文字不可、2000文字制限
9. `src/components/FeedbackList.tsx` — 一覧表示
   - GET API呼び出し → フィードバックカード一覧
   - 各カード: comment + createdAt（相対時間表示）
   - ロード中・空状態・エラー状態のUI
10. `src/App.tsx` — ページレイアウト統合
    - Header + FeedbackForm + FeedbackList を配置
    - 投稿成功時に一覧を自動リフレッシュ

### Phase 4: 仕上げ
11. 不要なViteデフォルトファイル削除（`src/App.css`, `src/assets/react.svg` 等）
12. `frontend/.gitignore` の確認（node_modules, build等）

## Relevant files（全て新規作成）
- `frontend/package.json` — 依存関係（react, react-dom, vite, tailwindcss, typescript）
- `frontend/vite.config.ts` — Vite設定（outDir: build, proxy: /api → localhost:7071）
- `frontend/src/index.css` — Tailwind読み込み + グローバルスタイル
- `frontend/src/types/feedback.ts` — 型定義
- `frontend/src/api/feedbacks.ts` — API呼び出し関数
- `frontend/src/components/Header.tsx` — ヘッダー
- `frontend/src/components/FeedbackForm.tsx` — 投稿フォーム
- `frontend/src/components/FeedbackList.tsx` — 一覧表示
- `frontend/src/App.tsx` — メインページ
- `frontend/src/main.tsx` — エントリーポイント

## 既存ファイル（参照のみ）
- `Functions/Survey/Models/Feedback.cs` — バックエンドモデル（型定義の参照元）
- `Functions/Survey/Models/FeedbackRequest.cs` — リクエストDTO
- `Functions/Survey/Models/FeedbackResponse.cs` — レスポンスDTO
- `Functions/Survey/Functions/PostFeedback.cs` — POST API実装
- `Functions/Survey/Functions/GetFeedbacks.cs` — GET API実装
- `Functions/Survey/local.settings.json` — CORS `http://localhost:5173` 設定済み

## Verification
1. `cd frontend && npm install && npm run build` でビルド成功確認（`build/` に出力）
2. `npm run dev` で開発サーバー起動 → `http://localhost:5173` でアクセス
3. バックエンド起動状態で:
   - フォームにコメント入力 → 送信 → 201レスポンス確認
   - 一覧にフィードバックが表示されること
   - 空コメント送信 → バリデーションエラー表示
   - 2000文字以上入力 → フォーム側でブロック
4. `npm run build` → `build/` ディレクトリ生成確認

## Decisions
- **Viteビルド出力先**: `build`（copilot-instructions準拠、Viteデフォルトの `dist` から変更）
- **APIプロキシ**: 開発時は Vite proxy で `/api` → `http://localhost:7071` に転送
- **ルーティング**: シングルページのためreact-routerは不使用
- **状態管理**: useStateで十分（小規模のため外部ライブラリ不使用）
- **デプロイ**: Azure SWA GitHub Actionsワークフローはスコープ外
- **Tailwind CSS v4**: `@tailwindcss/vite` プラグイン方式（v4の推奨設定）
- **copilot-instructionsとの差異注記**: instructions上は Survey API（/api/surveys）と記載があるが、実装済みバックエンドは Feedback API（/api/feedbacks）。バックエンド実装に準拠する。
