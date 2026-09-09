# hw — Horizon Walker Free Camera MOD

[Horizon Walker](https://store.steampowered.com/app/3279780/Horizon_Walker/)(Steam版, Unity製)向けの非公式MODです。
BepInEx + Harmony の実行環境上で動作し、**いつでもトグルキーで有効/無効にできるフリーカメラ**を追加します。

- カメラの追従・回転を行っているゲーム側のスクリプトを実行時に一時無効化してから、
  独自のフリーフライ操作(WASD移動 + マウス視点 + マウスホイールで速度調整)を乗せる方式です。
- ゲーム内部のクラス名に依存しないため、アップデートによる互換性の影響を受けにくい設計です。
- 経済・戦闘バランス・ガチャ確率などゲームプレイの根幹には一切干渉しません(カメラのみ)。

## 動作要件

- Horizon Walker (Steam版)
- [BepInEx](https://github.com/BepInEx/BepInEx) 5.x (Unity Mono 版)
  - **注意:** 本作はモバイル版からの移植のため、IL2CPPビルドの可能性があります。
    `<インストール先>\Horizon Walker_Data\il2cpp_data` や `GameAssembly.dll` が存在する場合はIL2CPPビルドです。
    その場合は本プロジェクトをそのまま使えません。下記「IL2CPPの場合」を参照してください。

## ビルド済みDLLの入手

- 本リポジトリのビルドはGitHub Actions等でも再現できる、ゲーム本体不要のビルド構成です
  (Unity公式APIはNuGetの `UnityEngine.Modules` パッケージ、BepInEx本体は `libs/BepInEx/` に同梱した
  公式リリースバイナリを参照するため、手元にゲームが無くても `dotnet build` だけで動作するDLLが生成できます)。
- 自分でビルドする場合は [.NET SDK](https://dotnet.microsoft.com/) を入れた上で、以下を実行してください。

  ```bash
  cd src/HorizonWalkerFreeCam
  dotnet build -c Release
  ```

  成功すると `bin/Release/net472/HorizonWalkerFreeCam.dll` が生成されます。

## セットアップ (Mono版 BepInEx の場合)

1. [BepInEx 5.4.x (x64)](https://github.com/BepInEx/BepInEx/releases) をダウンロードし、
   Horizon Walker のインストールフォルダ(`Horizon Walker.exe` があるフォルダ)に展開する。
2. ゲームを一度起動して終了し、`BepInEx` フォルダが生成されることを確認する。
3. 上記の手順でビルドした(または配布された)`HorizonWalkerFreeCam.dll` を
   `<インストール先>\BepInEx\plugins\` にコピーする。
4. ゲームを起動し、デフォルトでは **F9キー** でフリーカメラをON/OFFできる。

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

## IL2CPPの場合

ゲームがIL2CPPビルドだった場合、通常の `UnityEngine.dll` を直接参照する本プロジェクトのままではロードできません。
以下のいずれかで対応してください。

1. [BepInEx 6 (IL2CPP版)](https://builds.bepinex.dev/projects/bepinex_be) を導入し、
   自動生成される `interop` アセンブリ(`BepInEx/interop/*.dll`)を本プロジェクトの参照先に差し替える。
2. `Camera` / `Input` 等のAPI自体は Il2CppInterop 経由でもほぼ同じシグネチャで使えるため、
   `FreeCamController.cs` のロジックはほぼそのまま流用可能です(`.csproj` の参照先とターゲットの調整が中心)。

手元の実機バイナリが無いと確実な判定・検証ができないため、実際の配布パッケージ構成に合わせて
`HorizonWalkerFreeCam.csproj` の参照を調整してください。

## 免責事項

- 本MODは私的な単一プレイヤー体験の改善(カメラ操作性の向上)のみを目的としています。
- オンライン対戦・ランキング等がある場合は、そうした要素での使用は避けてください。
- 利用は自己責任でお願いします。ゲームの利用規約を確認の上でご利用ください。

## プロジェクト構成

```
libs/BepInEx/                   # BepInEx公式リリース同梱バイナリ (BepInEx.dll, 0Harmony.dll)
src/HorizonWalkerFreeCam/
  HorizonWalkerFreeCam.csproj   # ビルド設定 (NuGetのUnityEngine.Modules + libs/BepInExを参照)
  Plugin.cs                     # BepInEx プラグインのエントリポイント・設定項目
  FreeCamController.cs          # フリーカメラの本体ロジック
```
