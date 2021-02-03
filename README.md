# HttpStatusExtention
HttpStatusでは表示できない情報を追加で送信します。  
  
### Status object

```js
StatusObject = {
  "beatmap": null | {
    "difficulty": "Easy" | "Normal" | "Hard" | "Expert" | "ExpertPlus", // Beatmap difficultyに加えてラベルがついてきます。（後で別パラメーターとして分離させるかも）
    "pp": ランク譜面の時PPが入ります。
    "star": スコアセイバーの星
    "downloadCount": ダウンロード回数
    "upVotes": アップボーテ数
    "downVotes": ダウンボーテ数
    "rating": アップボーテとダウンボーテの比率みたいなもの
  },
}
```
