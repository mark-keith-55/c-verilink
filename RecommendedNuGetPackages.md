# 推奨NuGetパッケージ - .NET Framework 4.7.2チャットアプリケーション

## 必須パッケージ

### 1. Newtonsoft.Json
- **パッケージ**: `Newtonsoft.Json`
- **用途**: JSON形式のサーバーリストをシリアライズ/デシリアライズ
- **インストール**: `Install-Package Newtonsoft.Json`

## 推奨パッケージ

### 2. Serilog
- **パッケージ**: `Serilog`, `Serilog.Sinks.File`, `Serilog.Sinks.Console`
- **用途**: 構造化ログ出力、エラートラッキング
- **インストール**: 
  ```
  Install-Package Serilog
  Install-Package Serilog.Sinks.File
  Install-Package Serilog.Sinks.Console
  ```

### 3. System.Threading.Tasks.Extensions
- **パッケージ**: `System.Threading.Tasks.Extensions`
- **用途**: async/await パターンの拡張サポート
- **インストール**: `Install-Package System.Threading.Tasks.Extensions`

### 4. System.Net.Http
- **パッケージ**: `System.Net.Http`
- **用途**: HTTP通信（将来的にREST APIとの連携用）
- **インストール**: `Install-Package System.Net.Http`

## 追加機能向けパッケージ

### 5. Microsoft.Extensions.Configuration
- **パッケージ**: `Microsoft.Extensions.Configuration.Json`
- **用途**: 設定ファイル管理
- **インストール**: `Install-Package Microsoft.Extensions.Configuration.Json`

### 6. Microsoft.Extensions.DependencyInjection
- **パッケージ**: `Microsoft.Extensions.DependencyInjection`
- **用途**: 依存性注入コンテナ
- **インストール**: `Install-Package Microsoft.Extensions.DependencyInjection`

### 7. AutoMapper
- **パッケージ**: `AutoMapper`
- **用途**: オブジェクト間のマッピング
- **インストール**: `Install-Package AutoMapper`

### 8. FluentValidation
- **パッケージ**: `FluentValidation`
- **用途**: 入力値検証
- **インストール**: `Install-Package FluentValidation`

## セキュリティ強化パッケージ

### 9. System.Security.Cryptography.Algorithms
- **パッケージ**: `System.Security.Cryptography.Algorithms`
- **用途**: 暗号化通信（将来的な機能拡張用）
- **インストール**: `Install-Package System.Security.Cryptography.Algorithms`

## テスト用パッケージ

### 10. NUnit
- **パッケージ**: `NUnit`, `NUnit3TestAdapter`
- **用途**: 単体テスト
- **インストール**: 
  ```
  Install-Package NUnit
  Install-Package NUnit3TestAdapter
  ```

### 11. Moq
- **パッケージ**: `Moq`
- **用途**: モックオブジェクト作成
- **インストール**: `Install-Package Moq`

## パフォーマンス向上パッケージ

### 12. System.Memory
- **パッケージ**: `System.Memory`
- **用途**: メモリ効率的な操作
- **インストール**: `Install-Package System.Memory`

### 13. System.Buffers
- **パッケージ**: `System.Buffers`
- **用途**: バッファプール管理
- **インストール**: `Install-Package System.Buffers`

## 最低限必要なパッケージ
プロジェクトを動作させるために最低限必要なのは：
- `Newtonsoft.Json`（サーバーリストのJSON処理用）

その他のパッケージは機能拡張や開発効率向上のための推奨パッケージです。