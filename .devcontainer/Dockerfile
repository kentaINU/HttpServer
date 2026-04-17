FROM mcr.microsoft.com/devcontainers/dotnet:1-8.0-bookworm

# 1. エラーの原因となる古いYarnリポジトリの設定ファイルを削除
# 2. その後、通常のアップデートとパッケージインストールを実行
RUN rm -f /etc/apt/sources.list.d/yarn.list \
    && apt-get update \
    && apt-get install -y git zsh curl