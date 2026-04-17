import os

# README.md の内容を定義
readme_content = """# MyWebSystem - 自作HTTPサーバープロジェクト

Rancher Desktop と VS Code Dev Containers を活用した、C#による独自プロトコル解析・HTTPサーバー実装プロジェクトです。

## 🎯 プロジェクトの目的
既存のWebフレームワーク（ASP.NET Core等）に頼らず、`System.Net.Sockets` を用いてTCP/IPレイヤーからHTTPプロトコルを自作することで、Web技術の深層（パケット解析、ステートレス通信、リクエスト/レスポンス構造）を理解することを目的としています。

## 🏗 プロジェクト構成
本リポジトリは .NET のマルチプロジェクト構成を採用しています。

- **HttpServer.App**: 実行用コンソールアプリケーション。TCPリスナーの待機、クライアント接続の管理、レスポンスの送信を担当します。
- **HttpServer.Core**: 解析用クラスライブラリ。受信した生の文字列を `HttpRequest` オブジェクトへパースするロジック（独自プロトコル解析）を切り出しています。
- **HttpServer.Tests**: xUnitによるテストプロジェクト。パースロジックが期待通りに動くかを、サーバーを起動せずに検証します。

## 🚀 開発環境の開始方法
1. **Rancher Desktop** を起動し、エンジンが `dockerd (moby)` であることを確認します。
2. VS Code でこのフォルダを開きます。
3. 右下のポップアップ、またはコマンドパレットから **「Reopen in Container」** を選択します。
4. コンテナ起動時、`.devcontainer/setup.sh` が自動実行され、ソリューション環境が構築されます。

## 💻 実行とテスト
### サーバーの起動
コンテナ内のターミナルで以下を実行します：
```bash
dotnet run --project HttpServer.App