using System;
using UnityEngine;
using UnityEngine.Video;

namespace CanvasRecorder
{
    /// <summary>
    /// 保存前の録画結果を Unity 内で再生するプレビュー。
    /// <see cref="ScreenRecorder"/> から受け取った blob URL を <see cref="VideoPlayer"/> に渡し、
    /// デコード結果を <see cref="Texture"/> として公開する。
    /// 描画は呼び出し側の責務で、RawImage でも OnGUI でも好きな方法で表示してよい。
    ///
    /// <see cref="ScreenRecorder"/> は SendMessage の宛先なので専用の GameObject に載せる必要がある。
    /// このコンポーネントはそれとは別の GameObject に置くこと。
    /// </summary>
    public class RecordingPreview : MonoBehaviour
    {
        /// <summary>
        /// 対象の <see cref="ScreenRecorder"/>。未設定ならシーンから探す。
        /// </summary>
        [SerializeField]
        private ScreenRecorder _screenRecorder;

        private VideoPlayer _videoPlayer;

        /// <summary>プレビューを開いているかどうか。</summary>
        public bool IsOpen { get; private set; }

        /// <summary>再生準備が完了しているかどうか。</summary>
        public bool IsPrepared => _videoPlayer != null && _videoPlayer.isPrepared;

        /// <summary>再生中かどうか。</summary>
        public bool IsPlaying => _videoPlayer != null && _videoPlayer.isPlaying;

        /// <summary>デコードされた映像。準備完了前は null。</summary>
        public Texture Texture => IsPrepared ? _videoPlayer.texture : null;

        /// <summary>動画の長さ（秒）。</summary>
        public double Length => _videoPlayer != null ? _videoPlayer.length : 0d;

        /// <summary>現在の再生位置（秒）。</summary>
        public double Time => _videoPlayer != null ? _videoPlayer.time : 0d;

        /// <summary>再生準備が完了したときに発火する。</summary>
        public event Action Prepared;

        /// <summary>再生に失敗したときにメッセージ付きで発火する。</summary>
        public event Action<string> Failed;

        private void Awake()
        {
            if (_screenRecorder == null) _screenRecorder = FindAnyObjectByType<ScreenRecorder>();

            if (_screenRecorder == null)
            {
                Debug.LogError($"{nameof(ScreenRecorder)} がシーンに存在しません。");
                enabled = false;
                return;
            }

            _videoPlayer = gameObject.AddComponent<VideoPlayer>();
            _videoPlayer.playOnAwake = false;
            _videoPlayer.source = VideoSource.Url;
            // APIOnly なら RenderTexture のサイズを事前に決める必要がなく、
            // 準備完了後に VideoPlayer.texture をそのまま描画できる。
            _videoPlayer.renderMode = VideoRenderMode.APIOnly;
            // 音声付きで録画された場合はプレビューでも再生する。
            // 音声トラックが無い動画でも Direct のままで問題ない。
            _videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
            _videoPlayer.isLooping = true;
            _videoPlayer.skipOnDrop = true;
            // とりあえずミュート
            _videoPlayer.SetDirectAudioMute(0, true);
            _videoPlayer.prepareCompleted += OnPrepareCompleted;
            _videoPlayer.errorReceived += OnErrorReceived;
        }

        private void OnEnable()
        {
            if (_screenRecorder != null) _screenRecorder.PreviewUrlReady += OnPreviewUrlReady;
        }

        private void OnDisable()
        {
            if (_screenRecorder != null) _screenRecorder.PreviewUrlReady -= OnPreviewUrlReady;
        }

        private void OnDestroy()
        {
            if (_videoPlayer == null) return;
            _videoPlayer.prepareCompleted -= OnPrepareCompleted;
            _videoPlayer.errorReceived -= OnErrorReceived;
        }

        /// <summary>
        /// 保持している録画結果のプレビューを開く。
        /// URL の取得と準備は非同期なので、再生可能になると <see cref="Prepared"/> が発火する。
        /// </summary>
        /// <returns>要求できたら true。保持している録画が無ければ false。</returns>
        public bool Open() => _screenRecorder.RequestPreviewUrl();

        /// <summary>
        /// プレビューを閉じ、再生用 URL を解放する。
        /// </summary>
        public void Close()
        {
            if (_videoPlayer != null) _videoPlayer.Stop();
            IsOpen = false;
            // VideoPlayer が参照を手放してから URL を解放する。
            _screenRecorder.ReleasePreviewUrl();
        }

        /// <summary>再生と一時停止を切り替える。</summary>
        public void TogglePlay()
        {
            if (!IsPrepared) return;

            if (_videoPlayer.isPlaying) _videoPlayer.Pause();
            else _videoPlayer.Play();
        }

        /// <summary>再生位置を秒で指定する。</summary>
        public void Seek(double seconds)
        {
            if (!IsPrepared) return;
            _videoPlayer.time = Math.Clamp(seconds, 0d, _videoPlayer.length);
        }

        private void OnPreviewUrlReady(string url)
        {
            _videoPlayer.url = url;
            _videoPlayer.Prepare();
        }

        private void OnPrepareCompleted(VideoPlayer source)
        {
            IsOpen = true;
            source.Play();
            Prepared?.Invoke();
        }

        private void OnErrorReceived(VideoPlayer source, string message)
        {
            IsOpen = false;
            Debug.LogError($"プレビューの再生に失敗しました: {message}");
            Failed?.Invoke(message);
        }
    }
}
