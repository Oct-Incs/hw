# hw — Horizon Walker Free Camera MOD

[Horizon Walker](https://store.steampowered.com/app/3279780/Horizon_Walker/)(Steam版, Unity/IL2CPP製)向けの非公式MODです。
BepInEx 6 (IL2CPP版) + Harmony の実行環境上で動作し、**いつでもトグルキーで有効/無効にできるフリーカメラ**を追加します。

- カメラの追従・回転を行っているゲーム側のスクリプトを実行時に一時無効化してから、
  独自のフリーフライ操作(WASD移動 + マウス視点 + マウスホイールで速度調整)を乗せる方式です。
- ゲーム内部のクラス名に依存しないため、アップデートによる互換性の影響を受けにくい設計です。
- 経済・戦闘バランス・ガチャ確率などゲームプレイの根幹には一切干渉しません(カメラのみ)。

> **本作はIL2CPPビルドです。** (`GameAssembly.dll` の存在で確認済み)
> BepInEx 5 (Mono版) は動作しません。必ず下記の **BepInEx 6 (IL2CPP版)** を使ってください。

## 動作要件

- Horizon Walker (Steam版, IL2CPPビルド)
- [BepInEx 6 (IL2CPP版, Bleeding Edge)](https://builds.bepinex.dev/projects/bepinex_be)
- [.NET SDK](https://dotnet.microsoft.com/) (ビルド時のみ)

## セットアップ手順

### 1. BepInEx 6 (IL2CPP版) の導入

1. https://builds.bepinex.dev/projects/bepinex_be を開き、
   `Unity.IL2CPP-win-x64`(64bit実行ファイルの場合。32bitなら `-win-x86`)の最新ビルドをダウンロードする。
2. Horizon Walker のインストールフォルダ(`Horizon Walker.exe` があるフォルダ)に展開する。
3. ゲームを一度起動する。**初回はInterop用アセンブリの生成のため時間がかかります**(数十秒〜数分、フリーズしたように見えても待つ)。
4. 起動後、以下が生成されていることを確認する。
   - `BepInEx\core\` … BepInEx本体一式
   - `BepInEx\interop\` … このゲーム専用に生成されたUnity/ゲームスクリプトの相互運用アセンブリ
   - `BepInEx\LogOutput.log`

   これらが生成されていなければ、アーキテクチャ(x64/x86)の選択ミスか展開先が誤っている可能性があります。

### 2. MODのビルド

Interop アセンブリはゲームのバージョンごとに変わるため、汎用パッケージとして同梱できません。
**必ず手順1を済ませた自分の環境で本プロジェクトをビルドしてください。**

1. `src/HorizonWalkerFreeCam/GameDir.user.props.sample` を
   `src/HorizonWalkerFreeCam/GameDir.user.props` としてコピーし、
   中の `GameDir` を自分の環境のインストール先に書き換える。
2. ビルドする。

   ```bash
   cd src/HorizonWalkerFreeCam
   dotnet build -c Release
   ```

3. 生成された `bin/Release/net6.0/HorizonWalkerFreeCam.dll` を
   `<インストール先>\BepInEx\plugins\` にコピーする。
4. ゲームを再起動する。デフォルトでは **F9キー** でフリーカメラをON/OFFできる。

## うまく動かないときの確認方法

1. `BepInEx\config\BepInEx.cfg` の `[Logging.Console]` → `Enabled = true` にしてゲームを起動し、
   コンソールに `Horizon Walker Free Camera 1.1.0 loaded.` のようなログが出るか確認する。
2. 出ない場合は `BepInEx\LogOutput.log` の末尾にエラーが出ていないか確認する
   (`HorizonWalkerFreeCam.dll` が `BepInEx\plugins\` 直下に置かれているか、
   ビルド時のInteropアセンブリと実際にインストールされているBepInExのバージョンが一致しているか、なども見てください)。

## 操作方法(デフォルト設定)

| 操作 | キー |
| --- | --- |
| フリーカメラ ON/OFF | F9 |
| 移動 | W / A / S / D |
| 上昇 / 下降 | Space or E / Ctrl or Q |
| 視点回転 | マウス移動 |
| 移動速度の調整 | マウスホイール |
| 高速移動 | Shift 押しながら移動 |

すべて `BepInEx/config/oct-incs.horizonwalker.freecam.cfg` から変更できます
(トグルキー、移動速度、感度、Y軸反転、カーソルロックの有無など)。

## 免責事項

- 本MODは私的な単一プレイヤー体験の改善(カメラ操作性の向上)のみを目的としています。
- オンライン対戦・ランキング等がある場合は、そうした要素での使用は避けてください。
- 利用は自己責任でお願いします。ゲームの利用規約を確認の上でご利用ください。

## プロジェクト構成

```
src/HorizonWalkerFreeCam/
  HorizonWalkerFreeCam.csproj   # ビルド設定 (GameDir を自環境のBepInEx導入先に設定)
  Plugin.cs                     # BepInEx (IL2CPP) プラグインのエントリポイント・設定項目
  FreeCamController.cs          # フリーカメラの本体ロジック
  GameDir.user.props.sample     # ローカルパス設定のサンプル
```

## 実装上の注意 (開発者向け)

このリポジトリのビルド環境には実機のIL2CPP Interopアセンブリが存在しないため、
`FreeCamController.cs` / `Plugin.cs` はBepInEx公式ドキュメントのAPI仕様に基づいて実装していますが、
**実機ビルドでのコンパイル確認はできていません**。導入時にビルドエラーが出た場合は、
`BepInEx\interop\` 内の実際のDLL名やAPIシグネチャを見ながら調整が必要な場合があります。
