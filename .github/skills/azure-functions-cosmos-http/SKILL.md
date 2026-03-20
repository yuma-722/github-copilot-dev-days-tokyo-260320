---
name: azure-functions-cosmos-http
description: Azure Functions isolated worker（C#/.NET）の HTTP トリガーで入力を受け取り、Cosmos DB へ書き込む実装を設計・整理・レビューする。Azure Functions、Cosmos DB、HTTP trigger、DTO、Managed Identity、RBAC、partition key、レスポンス設計に言及したとき、またはそれらの雛形方針や再利用ワークフローが必要なときに使用する。
---

# Azure Functions + Cosmos DB HTTP 実装スキル

このSkillは、HTTPトリガー Azure Functions から Cosmos DB へ書き込む C#/.NET 実装の前提をそろえ、再利用しやすい構成に整理するための資料＋ワークフローである。前提は **isolated worker + DI** とし、ここでは**プロダクトコードを直接書かず**、確認事項・構成・判断基準を短くまとめる。

## 先に確認する

生成や提案の前に、次を確認する。

- **関数名**: 何をするエンドポイントか
- **HTTP**: method、route、auth level、呼び出し元
- **Cosmos DB**: account、database、container
- **キー設計**: partition key と `id` の決め方
- **DTO**: request / response の項目、必須・任意、制約
- **レスポンス**: 成功時の本文、失敗時のエラー形式、返却コード

`id` と partition key の決め方が曖昧な場合は、先に確認してから進める。ここを曖昧にしたまま雛形化しない。

## 推奨構成

単純なサンプルでも、責務は分ける。

- **Function**: HTTP 入出力、バリデーション結果の返却、ログ開始点
- **DTO**: request / response / error payload
- **Service**: Cosmos DB 書き込みと永続化ルール
- **Program.cs**: DI、クライアント登録、設定読み込み
- **config**: `local.settings.json` と環境変数名の整理

Function から Cosmos DB に直接すべてを書くより、永続化処理は Service に寄せる。Function は薄く保つ。

## Cosmos DB 書き込みの判断基準

- **既定**: Cosmos DB SDK の直接利用を優先する
- **output binding を検討してよい場面**: 単純な 1 件書き込みで、変換や事前チェックが少ないケース
- **SDK を選ぶ場面**: バリデーション、重複判定、条件分岐、診断、複数操作、例外制御が必要なケース

迷ったら SDK を選ぶ。HTTP API では、戻り値・例外・診断を制御しやすい構成を優先する。

## 認証と設定

- Azure 上では **Managed Identity + RBAC** を優先する
- Cosmos DB への権限は、必要最小限のデータプレーン権限で設計する
- ローカルでは `local.settings.json` または環境変数で設定し、接続方法を明示する
- 接続文字列前提に固定せず、Managed Identity 利用時の設定差分を最初に整理する

最低限、次を確認する。

- ローカル開発時に何を使うか（接続文字列 / Entra ID）
- Azure 配備時にどの ID で Cosmos DB にアクセスするか
- 必要な RBAC ロールと割り当て先
- 設定キー名をどうそろえるか

## 最低限の実装方針

- **返却コード**: 少なくとも `200/201`、`400`、`401/403`、`409`、`500` の扱いを決める
- **バリデーション**: DTO の必須項目、文字数、列挙値、整合性を先に決める
- **ログ**: 正常開始、入力不正、書き込み成功、例外を追える粒度にする
- **診断**: 失敗時に request context と Cosmos DB 例外の要点を残す

レスポンス本文は成功系と異常系で形をそろえる。呼び出し側が判定しやすい形を優先する。

## 進め方

1. 先に確認事項を埋める
2. `id` と partition key を確定する
3. Function / DTO / Service / Program.cs / config の責務を切る
4. SDK 直利用か output binding かを判断する
5. 認証・RBAC・ローカル設定を整理する
6. 返却コード、バリデーション、ログ、診断の最低限を定義する

## 期待する出力

- 実装前の確認項目リスト
- 推奨ファイル構成と責務分担
- 設定項目と認証方式の整理
- Cosmos DB 書き込み方式の判断メモ
- 実装レビュー時の確認観点

必要なら、このSkillを起点に雛形案・レビュー観点・不足確認の質問を短く返す。プロダクトコードはこのSkill本文に含めない。
