# 同梱バイナリについて

- `BepInEx.dll`, `0Harmony.dll`
- 取得元: https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.5
  (`BepInEx_win_x64_5.4.23.5.zip` 内の `BepInEx/core/` から抽出)
- ライセンス: BepInEx本体は LGPL-2.1、同梱の Harmony (0Harmony.dll) は MIT
- 用途: 本プロジェクトのビルド時参照 (`<Private>false</Private>` のためDLL自体は成果物に含まれません)

ゲームに実際に導入する BepInEx 本体は、必ず上記リリースページから別途入手してインストールしてください。
