# HttpStatusExtention
HttpStatusでは表示できない情報を追加で送信します。  
今のところ下記のオプションが追加されます。  
## 依存MOD  
SiraUtil  
[BeatSaberHttpStatus](https://github.com/denpadokei/beatsaber-http-status)  
SongDataCore  
SongCore  
  
# 注意  
BeatSaberHTTPStatusはデンパ時計が作ったものを使用してください。  
  
### Status object

```js
StatusObject = {
　"performance": null | {
		"current_pp": Double, // スコアに応じた取得可能PP
  },
  "beatmap": null | {
    "customLabel": 譜面の難易度につけられたカスタムラベルです。
    "pp": ランク譜面の時PPが入ります。
    "star": スコアセイバーの星
    "downloadCount": ダウンロード回数
    "upVotes": アップボーテ数
    "downVotes": ダウンボーテ数
    "rating": アップボーテとダウンボーテの比率みたいなもの
  },
}
```
