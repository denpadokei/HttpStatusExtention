# HttpStatusExtention

[![Build and Release](https://github.com/denpadokei/HttpStatusExtention/actions/workflows/build-and-release.yml/badge.svg)](https://github.com/denpadokei/HttpStatusExtention/actions/workflows/build-and-release.yml)

HttpSiraStatus が送信するステータスに、ランク譜面の情報とプレイ中の PP / AP を追加する MOD です。外部のオーバーレイや配信ツールから、難易度ラベル・Star・取得可能 PP を参照できます。

## 機能

- ステータスの `beatmap` に難易度ラベル (`customLabel`)、Star、譜面の PP、投票数などを追加します
- プレイ中の精度に応じた ScoreSaber の取得 PP を `performance.current_pp` に追加します
- 同じ精度から BeatLeader の取得 PP を `performance.current_bl_pp` に追加します
- 同じ精度から AccSaber の AP を `performance.current_acc_saber_ap` に追加します
- スコアが変わるたびに精度を再計算して送信します
- ゲームオーバーになった場合は失敗として PP を計算し直します
- 曲開始時に全項目、ポーズからの再開時に譜面情報を再送信します
- SongRequestManagerV2 が入っている場合は `other.srm_queue_status` にリクエストキューの開閉状態を追加します

## 動作環境

| 項目 | 内容 |
| --- | --- |
| Beat Saber | 1.40.8 |
| 依存 MOD | BSIPA ^4.3.6 / SongDetailsCache ^1.4.0 / SiraUtil ^3.2.1 / SongCore ^3.15.3 / HttpSiraStatus ^14.0.0 |

HttpSiraStatus は denpadokei 版を使用してください。他作者の BeatSaberHTTPStatus では追加情報を受け取れません。

## インストール

1. Beat Saber を 1.40.8 に合わせる (BSManager などのバージョン管理ツールを推奨)
2. BSIPA と上記の依存 MOD をインストールする
3. [Releases](https://github.com/denpadokei/HttpStatusExtention/releases) から `HttpStatusExtention-11.0.1-bs1.40.8.zip` をダウンロードし、`Plugins` フォルダをゲームフォルダへ上書きコピーする

## 使い方

HttpSiraStatus の WebSocket に接続するだけで、追加情報が同じステータスに含まれます。カスタム譜面をプレイしたとき、次のキーが追加されます。

```json
{
    "performance": {
        "current_pp": 0.0,
        "current_bl_pp": 0.0,
        "current_acc_saber_ap": 0.0
    },
    "beatmap": {
        "customLabel": "難易度ラベル",
        "pp": 0.0,
        "bl_pp": 0.0,
        "acc_saber_ap": 0.0,
        "star": 0.0,
        "downloadCount": 0,
        "upVotes": 0,
        "downVotes": 0,
        "rating": 0.0
    },
    "other": {
        "srm_queue_status": false
    }
}
```

- `beatmap` の各値は譜面選択時の情報で、PP は精度 95% として計算した値です。ランク外の譜面では入りません。
- `customLabel` は SongCore に難易度ラベルが設定されている譜面のみ入ります。
- `performance` の値はスコアが変わるたびに更新されます。
- `srm_queue_status` は SongRequestManagerV2 が入っている場合のみ送信されます。

PP 計算用のデータは起動時に次の URL から取得します。

- `https://cdn.pulselane.dev/raw_pp.json` (ScoreSaber の PP)
- `https://cdn.pulselane.dev/curves.json` (PP カーブ)
- `https://api.accsaber.com/ranked-maps` (AccSaber のランク譜面)

取得した ScoreSaber の PP とカーブは `UserData/HttpStatusExtention/pp.json` と `UserData/HttpStatusExtention/curves.json` にキャッシュされ、次回起動時はキャッシュが先に読み込まれます。BeatLeader の PP は BeatLeader MOD のキャッシュ (`UserData/BeatLeader/LeaderboardsCache`) から読み込むため、BeatLeader 側でキャッシュが作成されている必要があります。取得に失敗した項目は送信されません。

この MOD 自体に設定項目はありません。

## ビルド

```powershell
git clone https://github.com/denpadokei/HttpStatusExtention.git
cd HttpStatusExtention
dotnet build .\HttpStatusExtention\HttpStatusExtention.csproj -c Release
```

`BeatSaberDir` (または `GameDirectory`) に Beat Saber のインストール先を設定すると、ゲームの参照アセンブリを解決できます。

GitHub Actions の `build-and-release.yml` が push ごとにビルドし、`X.X.X` 形式のタグを push するとリリースを作成します。

## ライセンス

MIT
