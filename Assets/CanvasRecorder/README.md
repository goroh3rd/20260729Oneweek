# CanvasRecorder

Unity の Web(WebGL) ビルドで、ランタイムにゲーム画面を録画し、プレビューして、ファイルとして保存するためのパッケージです。

実体はブラウザの `canvas.captureStream()` と `MediaRecorder` API で、Unity Recorder パッケージ（エディタ専用）とは無関係です。

## 動作要件

- Unity 6000.0 以降（6000.0.58f2 で確認）
- 対象プラットフォーム: **Web(WebGL) のみ**
- ブラウザ: `MediaRecorder` と `canvas.captureStream()` に対応していること（Chrome / Edge で確認）
- プレビュー機能を使う場合は `com.unity.modules.video` が必要です

エディタの Play モードでは動作しません。各メソッドは何もせず `false` を返します。動作確認は必ず Web ビルドして行ってください。

## 導入

`Assets/CanvasRecorder/` をプロジェクトにコピーするだけです。

| フォルダ | 内容 | アセンブリ |
|---|---|---|
| `Runtime/` | `ScreenRecorder.cs`、`RecordingPreview.cs` | `CanvasRecorder` |
| `Plugins/WebGL/` | `Recorder.jslib`（ブラウザ API の呼び出し本体） | なし（プラグイン） |
| `Samples/` | サンプルシーンとサンプルスクリプト。不要なら削除して構いません | `CanvasRecorder.Samples` |

### 名前空間とアセンブリ定義

すべての型は `CanvasRecorder` 名前空間にあります。

```csharp
using CanvasRecorder;
```

ランタイムは `CanvasRecorder.asmdef`（アセンブリ名 `CanvasRecorder`）にまとまっています。
`autoReferenced` が有効なので、アセンブリ定義を使っていないコード（`Assembly-CSharp`）からは
`using` を書くだけで参照できます。

自分のコードをアセンブリ定義に分けている場合は、その `.asmdef` の Assembly Definition References に
`CanvasRecorder` を追加してください。

`Samples/` は `CanvasRecorder.Samples` として別アセンブリに分かれているので、
サンプルを削除してもランタイムのビルドには影響しません。

## セットアップ

シーンに **`ScreenRecorder` という名前の空の GameObject** を作り、`ScreenRecorder` コンポーネントだけをアタッチしてください。

GameObject の名前は必須です。jslib から Unity への通知に `SendMessage` を使っており、その宛先がこの名前になっているためです。

また **`ScreenRecorder` は他のコンポーネントと同居させないでください**。`SendMessage` は同じ GameObject の全コンポーネントに配送されるため、受け取れない側で `MissingMethodException` が発生します。

プレビューを使う場合は、**別の** GameObject に `RecordingPreview` をアタッチしてください。

## 使い方

### 最小構成

```csharp
using CanvasRecorder;
using UnityEngine;

public class MyRecorderUI : MonoBehaviour
{
    [SerializeField] private ScreenRecorder _screenRecorder;

    public void OnClickStart() => _screenRecorder.StartRecording();

    public void OnClickStop() => _screenRecorder.StopRecording();

    // 停止しただけでは保存されない。保存は明示的に呼ぶ。
    public void OnClickSave() => _screenRecorder.SaveRecording();
}
```

停止は非同期です。保存できる状態になると `RecordingReady` が発火します。

```csharp
private void OnEnable() => _screenRecorder.RecordingReady += OnReady;
private void OnDisable() => _screenRecorder.RecordingReady -= OnReady;

private void OnReady(long sizeBytes)
{
    Debug.Log($"{sizeBytes / 1024f:F1} KB の録画を保存できます");
}
```

### プレビュー

`RecordingPreview.Open()` を呼ぶと、保持中の録画を `VideoPlayer` でデコードします。準備が完了すると `Prepared` が発火し、`Texture` プロパティから映像を取得できます。

```csharp
[SerializeField] private RecordingPreview _preview;
[SerializeField] private RawImage _rawImage;

private void OnEnable() => _preview.Prepared += OnPrepared;
private void OnDisable() => _preview.Prepared -= OnPrepared;

public void OnClickPreview() => _preview.Open();

private void OnPrepared() => _rawImage.texture = _preview.Texture;

// 閉じるときは必ず Close を呼ぶ。内部で blob URL を解放している。
public void OnClickClose() => _preview.Close();
```

### X などへの共有

`ShareRecording()` は Web Share API で録画ファイルを OS の共有シートに渡します。
モバイルなら共有先に X アプリが並び、動画が添付された状態で投稿画面に入れます。

**X の Web Intent は仕様上、動画や画像を添付できません。** 添付まで行うには Web Share API か、
サーバを立てて X API を使うかのどちらかが必要です。本パッケージは前者を提供しています。

#### デスクトップでは X が共有先に出ません（実測）

`navigator.share({files})` はデスクトップでも動作しますが、Windows では **OS の共有シート**に処理が渡り、
そこに並ぶのは共有ターゲットとして登録された Windows アプリだけです。X は通常登録されていないため、
共有シートは開くものの X が選べません。

したがって**動画を添付した投稿は事実上モバイル限定**です。

| 環境 | 動画添付 |
|---|---|
| Android Chrome / iOS Safari（X アプリあり） | できる。共有シートから X を選ぶと動画付きで投稿画面に入る |
| デスクトップ | できない。共有シートに X が並ばない |

デスクトップ向けには、動画を保存させてから X の投稿画面を開き、ユーザーに手動で添付してもらう形に
フォールバックしてください。クリップボード経由は動画が非対応のため代替になりません。
ワンクリックでの動画付き投稿をデスクトップでも実現するには、サーバを立てて X API を使う必要があります。

判定には `CanShareToApps`（`CanShare` かつ `IsLikelyMobile`）を使えます。
`IsLikelyMobile` は UA ベースの推測なので確実ではありません。両方の導線をユーザーに見せておくのが安全です。

`CanShare` が `false` になる環境（`allow="web-share"` の無い cross-origin iframe など）でも同じく
フォールバックしてください。

```csharp
// モバイルなら共有シート、デスクトップなら保存 + 投稿画面。
// 判定を外した場合に行き止まりにならないよう、両方の導線を出しておくのが安全。
public void OnClickShare()
{
    if (!_screenRecorder.ShareRecording("スコア更新しました！")) SaveAndOpenX();
}

public void OnClickSaveAndOpenX() => SaveAndOpenX();

private void SaveAndOpenX()
{
    // 保存させてから投稿画面を開く（添付はユーザーが手動で行う）
    _screenRecorder.SaveRecording();
    XPost.OpenPostIntent("スコア更新しました！", "https://unityroom.com/games/xxxx");
}

private void OnEnable() => _screenRecorder.ShareCompleted += OnShareCompleted;
private void OnDisable() => _screenRecorder.ShareCompleted -= OnShareCompleted;

private void OnShareCompleted(RecordingShareResult result)
{
    // Shared / Cancelled / Unsupported / Failed
}
```

`ShareRecording()` も `SaveRecording()` と同様に、ユーザー操作のハンドラから直接呼んでください。

## API

### ScreenRecorder

| メンバー | 説明 |
|---|---|
| `bool StartRecording(int fps = 30, int bitsPerSecond = 8_000_000, bool includeAudio = true)` | 録画を開始する。保持中の前回結果は破棄される |
| `void StopRecording()` | 録画を停止する。**保存は行わない** |
| `bool SaveRecording(string fileName = null)` | 保持中の結果をダウンロードさせる。省略時は日時ベースのファイル名 |
| `void DiscardRecording()` | 保持中の結果を保存せずに破棄する |
| `bool RequestPreviewUrl()` | 再生用の blob URL を要求する。通常は `RecordingPreview` 経由で使う |
| `void ReleasePreviewUrl()` | 再生用 URL を解放する |
| `bool IsRecording` | 録画中かどうか |
| `bool HasRecording` | 保存できる結果を保持しているかどうか |
| `bool IsAudioAvailable` | 音声を録音できる状態かどうか |
| `bool ShareRecording(string text = null, string fileName = null)` | 録画ファイルを OS の共有シートに渡す（Web Share API） |
| `bool CanShare` | ファイル共有に対応しているかどうか |
| `bool IsLikelyMobile` | モバイル環境らしいかどうか（UA ベースの推測） |
| `bool CanShareToApps` | 動画添付での SNS 共有が見込めるか（`CanShare` かつ `IsLikelyMobile`） |
| `event Action<RecordingShareResult> ShareCompleted` | 共有の結果を通知（`Shared` / `Cancelled` / `Unsupported` / `Failed`） |

### XPost

| メンバー | 説明 |
|---|---|
| `static void OpenPostIntent(string text, string url, params string[] hashtags)` | X の投稿画面を本文入りで開く。**動画は添付されない** |
| `event Action<long> RecordingReady` | 停止が完了し保存可能になったときにバイト数を通知 |
| `event Action<string> PreviewUrlReady` | 再生用 URL が用意できたときに通知 |

### RecordingPreview

| メンバー | 説明 |
|---|---|
| `bool Open()` | プレビューを開く。準備完了は非同期 |
| `void Close()` | プレビューを閉じ、URL を解放する |
| `void TogglePlay()` | 再生と一時停止を切り替える |
| `void Seek(double seconds)` | 再生位置を秒で指定する |
| `Texture Texture` | デコードされた映像。準備完了前は `null` |
| `bool IsOpen` / `bool IsPrepared` / `bool IsPlaying` | 状態 |
| `double Length` / `double Time` | 動画の長さと現在位置（秒） |
| `event Action Prepared` | 再生準備が完了したときに発火 |
| `event Action<string> Failed` | 再生に失敗したときにメッセージ付きで発火 |

## サンプル

`Samples/CanvasRecorderSample.unity` を開いて Web ビルドしてください。
録画開始 → 停止 → プレビュー → 保存 / 破棄 の一連の流れを IMGUI で実装してあります。

ローカルで確認する場合は、ビルド結果を静的サーバで配信してください。`file://` では動作しません。

```bash
python -m http.server 8000 --directory <ビルド出力先>
```

## 制約と注意点

### 録画対象は Unity のキャンバスのみ

キャンバス外の HTML 要素は録画されません。ブラウザ UI ごと録りたい場合は `getDisplayMedia` を使う別の実装が必要です。

### 音声について

Unity が再生している音声を録音できます。`StartRecording` の `includeAudio` は既定で `true` です。

Unity 6 の WebGL 実装は、各サウンドチャンネルの gain ノードを個別に `audioContext.destination` へ
直結しており、まとめて取得できるマスターノードがありません。そのため本パッケージは
`AudioNode.prototype.connect` にフックを仕掛け、`destination` へ接続されるノードを
録音用の `MediaStreamAudioDestinationNode` にも分岐させています。

この方式には次の制約があります。

- **フックを仕掛けるより前から鳴り続けている音は録音されません。** フックは `ScreenRecorder.Awake`
  で仕掛けているため、シーン開始時から途切れずループしている BGM などが該当する可能性があります。
  Unity は再生のたびに接続をやり直すので、鳴り直された時点で拾われるようになります。
- ブラウザの `AudioContext` はユーザー操作があるまで suspended になります。録画開始を
  クリック起点にしていれば問題になりません。
- マイク入力は含みません。録音されるのは Unity が出力している音だけです。

音声を含めたくない場合は明示的に無効化してください。

```csharp
_screenRecorder.StartRecording(includeAudio: false);
```

音声を取得できる状態かどうかは `IsAudioAvailable` で確認できます。
`includeAudio: true` でも音声トラックを取得できなかった場合は、警告を出して映像のみで録画を続行します。

### 解像度はキャンバスのバッキングストアサイズ

CSS 上の表示サイズではなく、`devicePixelRatio` を掛けた実解像度で録画されます。

### 録画中はフレームレートが落ちます

エンコードで CPU を消費します。`StartRecording` の `fps` を下げると軽くなります。

### 長時間録画のメモリ

停止するまで全データがブラウザのメモリ上に蓄積されます。数分を超える録画を想定する場合は、File System Access API へ逐次書き出す実装への変更を検討してください。

### 保存されたファイルの再生時間

`MediaRecorder` の MP4 出力はフラグメント MP4 です。録画が複数フラグメントに分かれると `mvhd.duration` が 0 になり、シーク索引（`mfra`）も付かないため、再生時間が表示されずシークできないファイルになることがあります。

`MediaRecorder.start()` に timeslice を渡さないことで発生頻度は下がりますが、完全には保証できません。なお Discord や X に投稿すると再エンコードされて再生時間が正しく付きます。

確実にシーク可能なファイルが必要な場合は、`MediaRecorder` ではなく WebCodecs (`VideoEncoder`) と JS の muxer で自前に多重化する実装が必要です。

### 日本語フォント

ビルド後のランタイムでは組み込みフォントに日本語グリフが無く、`GUI.Label` などの日本語が表示されません。日本語を出す場合は TextMeshPro などで日本語フォントアセットを用意してください。

### iframe 内での配布

iframe に埋め込まれるサイト（unityroom など）では、`sandbox` 属性や user activation の条件によりダウンロードがブロックされることがあります。
`SaveRecording()` はユーザーのクリックハンドラから直接呼ぶようにしてください。停止と保存を分けてあるのはこのためです。

### モバイル

iOS Safari の `MediaRecorder` と `canvas.captureStream()` の組み合わせは不安定です。`StartRecording()` が `false` を返した場合に録画 UI を隠すなどのフォールバックを用意してください。

### preserveDrawingBuffer は不要

`canvas.captureStream()` は `preserveDrawingBuffer` の有無に影響されません（Chromium で実測）。カスタム WebGL テンプレートは不要です。
`preserveDrawingBuffer` が必要になるのは `drawImage` や `toDataURL` による同期リードバックの場合です。

## 発展させる場合の候補

- WebCodecs ベースの実装への差し替え（シーク可能なファイルを保証したい場合）
- 音声の取り込み（Unity の WebAudio マスターノードを `MediaStreamAudioDestinationNode` に接続する）
