using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace CanvasRecorder
{
    /// <summary>
    /// Web ビルドのランタイムで、Unity のキャンバス映像を録画してファイルとして保存する。
    /// 実体はブラウザの MediaRecorder API で、Plugins/WebGL/Recorder.jslib を呼び出す。
    ///
    /// 録画の停止（<see cref="StopRecording"/>）とファイルの保存（<see cref="SaveRecording"/>）は
    /// 分かれている。停止した時点で結果はブラウザ側に保持され、
    /// <see cref="SaveRecording"/> を呼ぶまでダウンロードは発生しない。
    /// 保存しない場合は <see cref="DiscardRecording"/> で破棄すること。
    ///
    /// JS から SendMessage を受け取るため、このコンポーネントが載る GameObject の名前は
    /// "ScreenRecorder" である必要がある。
    /// また SendMessage は同じ GameObject の全コンポーネントに配送されるので、
    /// このコンポーネントは他のコンポーネントと同居させず専用の GameObject に載せること。
    /// （同居させると受け取れない側で MissingMethodException が出る）
    ///
    /// 使い方: シーンに "ScreenRecorder" という名前の空の GameObject を作り、
    /// このコンポーネントだけをアタッチする。あとは任意の UI から各メソッドを呼ぶ。
    /// </summary>
    public class ScreenRecorder : MonoBehaviour
    {
        /// <summary>この GameObject の名前。jslib 側の SendMessage の宛先と一致させる。</summary>
        public const string GameObjectName = "ScreenRecorder";

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern int CanvasRecorder_Start(int fps, int bitsPerSecond, int includeAudio);
        [DllImport("__Internal")] private static extern void CanvasRecorder_Stop();
        [DllImport("__Internal")] private static extern int CanvasRecorder_InstallAudioTap();
        [DllImport("__Internal")] private static extern int CanvasRecorder_HasAudio();
        [DllImport("__Internal")] private static extern int CanvasRecorder_CanShare();
        [DllImport("__Internal")] private static extern int CanvasRecorder_Share(string text, string fileName);
        [DllImport("__Internal")] private static extern int CanvasRecorder_IsLikelyMobile();
        [DllImport("__Internal")] private static extern int CanvasRecorder_Save(string fileName);
        [DllImport("__Internal")] private static extern void CanvasRecorder_Discard();
        [DllImport("__Internal")] private static extern int CanvasRecorder_IsRecording();
        [DllImport("__Internal")] private static extern int CanvasRecorder_HasRecording();
        [DllImport("__Internal")] private static extern int CanvasRecorder_RequestPreviewUrl();
        [DllImport("__Internal")] private static extern void CanvasRecorder_ReleasePreviewUrl();
#else
        // エディタや他プラットフォームではブラウザ API が無いのでダミー実装にする。
        private static int CanvasRecorder_Start(int fps, int bitsPerSecond, int includeAudio) => 0;
        private static void CanvasRecorder_Stop() { }
        private static int CanvasRecorder_InstallAudioTap() => 0;
        private static int CanvasRecorder_HasAudio() => 0;
        private static int CanvasRecorder_CanShare() => 0;
        private static int CanvasRecorder_Share(string text, string fileName) => 0;
        private static int CanvasRecorder_IsLikelyMobile() => 0;
        private static int CanvasRecorder_Save(string fileName) => 0;
        private static void CanvasRecorder_Discard() { }
        private static int CanvasRecorder_IsRecording() => 0;
        private static int CanvasRecorder_HasRecording() => 0;
        private static int CanvasRecorder_RequestPreviewUrl() => 0;
        private static void CanvasRecorder_ReleasePreviewUrl() { }
#endif

        /// <summary>
        /// 録画が停止し、保存できる状態になったときにバイト数付きで発火する。
        /// この時点ではまだファイルは保存されていない。
        /// </summary>
        public event Action<long> RecordingReady;

        /// <summary>
        /// <see cref="RequestPreviewUrl"/> で再生用 URL が用意できたときに発火する。
        /// </summary>
        public event Action<string> PreviewUrlReady;

        /// <summary>録画中かどうか。</summary>
        public bool IsRecording => CanvasRecorder_IsRecording() != 0;

        /// <summary>保存できる録画結果を保持しているかどうか。</summary>
        public bool HasRecording => CanvasRecorder_HasRecording() != 0;

        /// <summary>
        /// 音声を録音できる状態かどうか。
        /// Unity の音声が初期化される前は false になることがある。
        /// </summary>
        public bool IsAudioAvailable => CanvasRecorder_HasAudio() != 0;

        /// <summary>
        /// 録画結果をファイルとして共有できるかどうか（Web Share API の対応状況）。
        /// cross-origin の iframe 内では allow="web-share" が無いと false になる。
        /// </summary>
        public bool CanShare => CanvasRecorder_CanShare() != 0;

        /// <summary>
        /// モバイル環境らしいかどうか。
        ///
        /// Web Share API はデスクトップでも動作するが、デスクトップの共有シートには
        /// X などの SNS アプリが並ばないため、動画を添付した投稿は事実上モバイル限定になる。
        /// <see cref="ShareRecording"/> と「保存 + 投稿画面」のどちらを既定にするかの判断に使う。
        /// UA ベースの推測なので確実ではない。
        /// </summary>
        public bool IsLikelyMobile => CanvasRecorder_IsLikelyMobile() != 0;

        /// <summary>
        /// 動画を添付した状態で SNS へ共有できる見込みがあるかどうか。
        /// <see cref="CanShare"/> かつ <see cref="IsLikelyMobile"/> のときに true になる。
        /// </summary>
        public bool CanShareToApps => CanShare && IsLikelyMobile;

        /// <summary>
        /// <see cref="ShareRecording"/> の結果を通知する。
        /// </summary>
        public event Action<RecordingShareResult> ShareCompleted;

        private void Awake()
        {
            if (name != GameObjectName)
            {
                Debug.LogWarning($"{nameof(ScreenRecorder)} の GameObject 名は \"{GameObjectName}\" である必要があります。" +
                                 "現在の名前では録画完了の通知を受け取れません。");
            }

            // 音声フックは早く仕掛けるほど取りこぼしが減る。
            // フックより前から鳴り続けている音は録音されないため、ここで仕掛けておく。
            CanvasRecorder_InstallAudioTap();
        }

        private void Start()
        {
            // Awake の時点ではまだ音声が初期化されていないことがあるので、一度だけ再試行する。
            if (!IsAudioAvailable) CanvasRecorder_InstallAudioTap();
        }

        /// <summary>
        /// 録画を開始する。保持している前回の録画結果は破棄される。
        /// </summary>
        /// <param name="fps">キャプチャするフレームレート。</param>
        /// <param name="bitsPerSecond">映像ビットレート。</param>
        /// <param name="includeAudio">
        /// Unity が再生している音声も録音するかどうか。
        /// 音声を取得できない場合は警告を出して映像のみで録画を続行する。
        /// </param>
        /// <returns>開始できたら true。</returns>
        public bool StartRecording(int fps = 30, int bitsPerSecond = 8_000_000, bool includeAudio = true)
        {
            if (includeAudio && !IsAudioAvailable) CanvasRecorder_InstallAudioTap();

            if (CanvasRecorder_Start(fps, bitsPerSecond, includeAudio ? 1 : 0) == 0)
            {
                Debug.LogWarning("録画を開始できませんでした。Web ビルドのブラウザ上でのみ動作します。");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 録画を停止する。結果は保持されるだけで、ファイルの保存は行わない。
        /// 停止処理は非同期なので、保存できるようになると <see cref="RecordingReady"/> が発火する。
        /// </summary>
        public void StopRecording() => CanvasRecorder_Stop();

        /// <summary>
        /// 保持している録画結果をファイルとしてダウンロードさせる。
        /// </summary>
        /// <param name="fileName">
        /// 保存するファイル名。null または空なら日時ベースの名前を自動生成する。
        /// 拡張子は録画に使われたコンテナに合わせること（通常は .mp4）。
        /// </param>
        /// <returns>ダウンロードを開始できたら true。保持している録画が無ければ false。</returns>
        public bool SaveRecording(string fileName = null)
        {
            if (CanvasRecorder_Save(fileName ?? string.Empty) == 0)
            {
                Debug.LogWarning("保存できる録画結果がありません。");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 保持している録画結果を保存せずに破棄する。プレビュー用 URL も解放される。
        /// </summary>
        public void DiscardRecording() => CanvasRecorder_Discard();

        /// <summary>
        /// 保持している録画結果の再生用 URL を要求する。
        /// 取得できると <see cref="PreviewUrlReady"/> が発火する。
        /// 使い終わったら <see cref="ReleasePreviewUrl"/> を呼ぶこと。
        /// </summary>
        /// <returns>要求できたら true。保持している録画が無ければ false。</returns>
        public bool RequestPreviewUrl()
        {
            if (CanvasRecorder_RequestPreviewUrl() == 0)
            {
                Debug.LogWarning("プレビューできる録画結果がありません。");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 再生用 URL を解放する。
        /// </summary>
        public void ReleasePreviewUrl() => CanvasRecorder_ReleasePreviewUrl();

        /// <summary>
        /// 保持している録画結果を OS の共有機能に渡す（Web Share API）。
        /// モバイルでは共有先に X アプリなどが並ぶ。
        /// ユーザー操作のハンドラから直接呼ぶこと。そうしないとブラウザに拒否される。
        /// 結果は <see cref="ShareCompleted"/> で通知される。
        /// </summary>
        /// <param name="text">共有時に添える本文。共有先によっては無視される。</param>
        /// <param name="fileName">共有するファイル名。null または空なら自動生成する。</param>
        /// <returns>共有を開始できたら true。</returns>
        public bool ShareRecording(string text = null, string fileName = null)
        {
            return CanvasRecorder_Share(text ?? string.Empty, fileName ?? string.Empty) != 0;
        }

        /// <summary>
        /// jslib から SendMessage 経由で呼び出される。直接呼ばないこと。
        /// </summary>
        public void OnShareResult(string result)
        {
            var parsed = result switch
            {
                "shared" => RecordingShareResult.Shared,
                "cancelled" => RecordingShareResult.Cancelled,
                "unsupported" => RecordingShareResult.Unsupported,
                _ => RecordingShareResult.Failed,
            };

            ShareCompleted?.Invoke(parsed);
        }

        /// <summary>
        /// jslib から SendMessage 経由で呼び出される。直接呼ばないこと。
        /// </summary>
        /// <param name="sizeBytes">録画結果のバイト数。SendMessage の制約で float で渡ってくる。</param>
        public void OnRecordingReady(float sizeBytes)
        {
            var bytes = (long)sizeBytes;
            Debug.Log($"録画を停止しました（保存可能）: {bytes} bytes");
            RecordingReady?.Invoke(bytes);
        }

        /// <summary>
        /// jslib から SendMessage 経由で呼び出される。直接呼ばないこと。
        /// </summary>
        /// <param name="url">録画結果の blob URL。</param>
        public void OnPreviewUrlReady(string url)
        {
            Debug.Log($"プレビュー用 URL を取得しました: {url}");
            PreviewUrlReady?.Invoke(url);
        }
    }
}
