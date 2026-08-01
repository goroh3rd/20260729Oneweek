using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CanvasRecorder.Samples
{
    /// <summary>
    /// CanvasRecorder �̎g��������ʂ莦���T���v���B
    /// �^��J�n �� ��~ �� �v���r���[ �� �ۑ� / �j�� �̗���� IMGUI �őg��ł���B
    /// ���ۂ̃v���W�F�N�g�ł� uGUI �Ȃǂɒu�������Ďg�����Ƃ�z�肵�Ă���B
    ///
    /// ����: �r���h�����u���E�U��ł̂ݓ��삷��B�G�f�B�^�� Play �ł�
    /// <see cref="ScreenRecorder"/> �̊e���\�b�h�͉������� false ��Ԃ��B
    /// �܂��g�ݍ��݃t�H���g�ɓ��{��O���t���������߁A��ʂɏo��������� ASCII �Ɍ��肵�Ă���B
    /// </summary>
    public class Recorder : MonoBehaviour
    {
        [SerializeField]
        private ScreenRecorder _screenRecorder;

        [SerializeField]
        private RecordingPreview _recordingPreview;

        private long _readyBytes = -1;
        private string _previewError;

        // �������^���ł��Ă��邩�m�F���邽�߂́A���s���ɐ������� 440Hz �̃g�[���B
        private AudioSource _toneSource;
        private bool _includeAudio = true;

        // �f���Ɖ����̃Y�����������邽�߂̓����}�[�J�[�B
        // ���Ԋu�Łu��ʑS�̂̔��t���b�V���v�Ɓu�Z���N���b�N���v�𓯂��t���[���Ŕ���������B
        // �ۑ������t�@�C����Ńt���b�V���ƃN���b�N���̈ʒu���ׂ�΁A
        // �Y���̑傫���ƁA���ꂪ���Ȃ̂����ԂƂƂ��ɍL����̂���������B
        private AudioSource _clickSource;
        private bool _syncMarkers;
        private float _markerTimer;
        private int _flashFramesLeft;
        private int _markerCount;

        private const float MarkerIntervalSeconds = 2f;

        // X �֓��e����Ƃ��̖{���� URL�B���ۂ̃v���W�F�N�g�ł͍����ւ��Ďg���B
        private const string PostText = "Recorded with CanvasRecorder";
        private const string GameUrl = "";

        private string _shareStatus;

        [SerializeField]
        private TextMeshProUGUI _statusTextMesh;
        [SerializeField]
        private RawImage previewImage;
        [SerializeField]
        private GameObject previewPanel;
        [SerializeField]
        private Button previewButton;

        private void Awake()
        {
            if (_screenRecorder == null) _screenRecorder = FindAnyObjectByType<ScreenRecorder>();
            if (_recordingPreview == null) _recordingPreview = FindAnyObjectByType<RecordingPreview>();

            if (_screenRecorder == null)
            {
                Debug.LogError($"{nameof(ScreenRecorder)} ���V�[���ɑ��݂��܂���B" +
                               $"\"{ScreenRecorder.GameObjectName}\" �Ƃ������O�� GameObject �����A" +
                               $"{nameof(ScreenRecorder)} ���A�^�b�`���Ă��������B");
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (_screenRecorder != null)
            {
                _screenRecorder.RecordingReady += OnRecordingReady;
                _screenRecorder.ShareCompleted += OnShareCompleted;
            }

            if (_recordingPreview != null) _recordingPreview.Failed += OnPreviewFailed;
        }

        private void OnDisable()
        {
            if (_screenRecorder != null)
            {
                _screenRecorder.RecordingReady -= OnRecordingReady;
                _screenRecorder.ShareCompleted -= OnShareCompleted;
            }

            if (_recordingPreview != null) _recordingPreview.Failed -= OnPreviewFailed;
        }

        private void OnShareCompleted(RecordingShareResult result)
        {
            _shareStatus = result.ToString();

            // ���L�ł��Ȃ����ł́A�ۑ������������� X �̓��e��ʂ��J���B
            // ����̓Y�t�̓��[�U�[�̎蓮����ɂȂ�B
            if (result == RecordingShareResult.Unsupported) FallBackToDownloadAndIntent();
        }

        private void FallBackToDownloadAndIntent()
        {
            _screenRecorder.SaveRecording();
            XPost.OpenPostIntent(PostText, GameUrl);
        }

        private void OnRecordingReady(long sizeBytes) => _readyBytes = sizeBytes;

        private void OnPreviewFailed(string message) => _previewError = message;

        private void Update()
        {
            /*
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // �X�y�[�X�L�[�������ꂽ�Ƃ��̏����������ɒǉ�
                if (_screenRecorder.IsRecording)
                {
                    _screenRecorder.StopRecording();
                    previewButton.gameObject.SetActive(true);
                }
                else
                {
                    _screenRecorder.StartRecording(includeAudio: true);
                }
            }

            if (_screenRecorder.HasRecording)
            {
                previewImage.texture = _recordingPreview.Texture;
            }

            if (_screenRecorder.IsRecording)
            {
                _statusTextMesh.text = $"RECORDING  {Time.time:F1}s";
            }
            else if (_screenRecorder.HasRecording)
            {
                _statusTextMesh.text = $"END  {Time.time:F1}s";
            }
            else
            {
                _statusTextMesh.text = $"IDLE  {Time.time:F1}s";
            }
            */
        }

        IEnumerator OpenPreviewPanel()
        {
            while (!_screenRecorder.HasRecording)
            {
                yield return null; // Wait until a recording is available
            }
            _previewError = null;
            _recordingPreview.Open();
            previewPanel.SetActive(true);
        }

        /*
        private void OnGUI()
        {
            var width = Screen.width;
            var height = Screen.height;

            GUI.skin.label.fontSize = Mathf.RoundToInt(height * 0.035f);
            GUI.skin.button.fontSize = Mathf.RoundToInt(height * 0.035f);

            if (_recordingPreview != null && _recordingPreview.IsOpen)
            {
                DrawPreview(width, height);
                return;
            }

            DrawRecorder(width, height);
            DrawSyncFlash(width, height);
        }
        */

        private void DrawRecorder(int width, int height)
        {
            /*
            // �^�悳�ꂽ�f�����^�����łȂ����Ƃ𔻕ʂł���悤�A���邢�w�i��~���B
            DrawSolid(new Rect(0, 0, width, height), new Color(0.16f, 0.42f, 0.70f));

            // ���������邱�Ƃ��m�F���邽�߂̉�������o�[�B
            var t = Mathf.PingPong(Time.time * 0.35f, 1f);
            var boxSize = height * 0.18f;
            DrawSolid(new Rect(t * (width - boxSize), height * 0.45f, boxSize, boxSize), new Color(1f, 0.78f, 0.16f));
            */

            var isRecording = _screenRecorder.IsRecording;
            var hasRecording = _screenRecorder.HasRecording;

            GUI.Label(new Rect(20, 20, width - 40, height * 0.06f),
                isRecording ? $"RECORDING  {Time.time:F1}s" : $"IDLE  {Time.time:F1}s");

            if (_readyBytes >= 0)
            {
                GUI.Label(new Rect(20, 20 + height * 0.07f, width - 40, height * 0.06f),
                    hasRecording ? $"Ready to save: {_readyBytes / 1024f:F1} KB" : "No recording held");
            }

            GUI.Label(new Rect(20, 20 + height * 0.14f, width - 40, height * 0.06f),
                $"Audio available: {_screenRecorder.IsAudioAvailable}   Include audio: {_includeAudio}" +
                (_syncMarkers ? $"   Markers: {_markerCount}" : string.Empty));

            GUI.Label(new Rect(20, 20 + height * 0.21f, width - 40, height * 0.06f),
                $"Can share: {_screenRecorder.CanShare}   Mobile: {_screenRecorder.IsLikelyMobile}" +
                (string.IsNullOrEmpty(_shareStatus) ? string.Empty : $"   Share: {_shareStatus}"));

            if (!string.IsNullOrEmpty(_previewError))
            {
                GUI.Label(new Rect(20, 20 + height * 0.28f, width - 40, height * 0.06f),
                    $"Preview error: {_previewError}");
            }

            var buttonWidth = width * 0.22f;
            var buttonHeight = height * 0.09f;
            var buttonY = height - height * 0.14f;

            // �����܂��̃g�O���B�^�撆�ł�����ł���悤�ɂ��Ă����B
            var toggleY = buttonY - buttonHeight - height * 0.02f;
            if (GUI.Button(new Rect(20, toggleY, buttonWidth, buttonHeight),
                    _toneSource.isPlaying ? "TONE: ON" : "TONE: OFF"))
            {
                if (_toneSource.isPlaying) _toneSource.Stop();
                else _toneSource.Play();
            }

            if (GUI.Button(new Rect(30 + buttonWidth, toggleY, buttonWidth, buttonHeight),
                    _includeAudio ? "REC AUDIO: ON" : "REC AUDIO: OFF"))
            {
                _includeAudio = !_includeAudio;
            }

            if (GUI.Button(new Rect(40 + buttonWidth * 2, toggleY, buttonWidth, buttonHeight),
                    _syncMarkers ? "SYNC MARK: ON" : "SYNC MARK: OFF"))
            {
                _syncMarkers = !_syncMarkers;
                _markerTimer = 0f;
                _markerCount = 0;
            }

            if (isRecording)
            {
                if (GUI.Button(new Rect(20, buttonY, buttonWidth, buttonHeight), "STOP"))
                {
                    _screenRecorder.StopRecording();
                }

                return;
            }

            if (GUI.Button(new Rect(20, buttonY, buttonWidth, buttonHeight), "START"))
            {
                _screenRecorder.StartRecording(includeAudio: _includeAudio);
                _readyBytes = -1;
                _previewError = null;
            }

            // ��~�E�v���r���[�E�ۑ��͂��ꂼ��Ɨ���������B
            if (!hasRecording) return;

            if (GUI.Button(new Rect(30 + buttonWidth, buttonY, buttonWidth, buttonHeight), "PREVIEW"))
            {
                _previewError = null;
                _recordingPreview.Open();
            }

            if (GUI.Button(new Rect(40 + buttonWidth * 2, buttonY, buttonWidth, buttonHeight), "SAVE"))
            {
                _screenRecorder.SaveRecording();
            }

            // ���L�Ɓu�ۑ� + ���e��ʁv�͕ʂ̃{�^���ɂ��Ă����B
            // �f�X�N�g�b�v�̋��L�V�[�g�ɂ� X �����΂Ȃ����߁A�Е��������ƍs���~�܂�ɂȂ�B
            // �ǂ�������[�U�[�̃N���b�N�̒��Ŋ���������K�v������B
            var recommended = _screenRecorder.CanShareToApps;
            if (GUI.Button(new Rect(50 + buttonWidth * 3, buttonY, buttonWidth, buttonHeight),
                    recommended ? "SHARE *" : "SHARE"))
            {
                _shareStatus = null;
                if (!_screenRecorder.ShareRecording(PostText)) FallBackToDownloadAndIntent();
            }

            var upperY = buttonY - buttonHeight * 2 - height * 0.04f;
            if (GUI.Button(new Rect(20, upperY, buttonWidth, buttonHeight), "DISCARD"))
            {
                _screenRecorder.DiscardRecording();
                _readyBytes = -1;
                _shareStatus = null;
            }

            if (GUI.Button(new Rect(30 + buttonWidth, upperY, buttonWidth * 2, buttonHeight),
                    recommended ? "SAVE & OPEN X" : "SAVE & OPEN X *"))
            {
                _shareStatus = null;
                FallBackToDownloadAndIntent();
            }
        }

        /// <summary>
        /// �����}�[�J�[�̔��t���b�V���B�N���b�N���Ɠ����t���[���ŏo���K�v�����邽�߁A
        /// ���̕`������ׂĉB���悤�ɍŌ�ɑS��ʂ֕`���B
        /// </summary>
        private void DrawSyncFlash(int width, int height)
        {
            if (_flashFramesLeft <= 0) return;
            DrawSolid(new Rect(0, 0, width, height), Color.white);
        }

        private void DrawPreview(int width, int height)
        {
            DrawSolid(new Rect(0, 0, width, height), new Color(0.08f, 0.08f, 0.10f));

            var texture = _recordingPreview.Texture;
            if (texture == null)
            {
                GUI.Label(new Rect(20, 20, width - 40, height * 0.06f), "Preparing...");
                return;
            }

            // ����̃A�X�y�N�g���ۂ����܂ܒ����Ɏ��߂�B
            var area = new Rect(0, height * 0.06f, width, height * 0.68f);
            var scale = Mathf.Min(area.width / texture.width, area.height / texture.height);
            var drawWidth = texture.width * scale;
            var drawHeight = texture.height * scale;
            GUI.DrawTexture(new Rect(area.x + (area.width - drawWidth) * 0.5f,
                                     area.y + (area.height - drawHeight) * 0.5f,
                                     drawWidth, drawHeight), texture);

            var length = _recordingPreview.Length;
            var time = _recordingPreview.Time;
            GUI.Label(new Rect(20, 20, width - 40, height * 0.06f),
                $"PREVIEW  {time:F1}s / {length:F1}s  ({texture.width}x{texture.height})");

            // �V�[�N�o�[�Blength �� 0 �̓���iduration �s���j�ł̓V�[�N�ł��Ȃ��B
            var sliderRect = new Rect(20, height - height * 0.22f, width - 40, height * 0.05f);
            if (length > 0d)
            {
                var seeked = GUI.HorizontalSlider(sliderRect, (float)time, 0f, (float)length);
                if (!Mathf.Approximately(seeked, (float)time)) _recordingPreview.Seek(seeked);
            }
            else
            {
                GUI.Label(sliderRect, "duration unknown - cannot seek");
            }

            var buttonWidth = width * 0.22f;
            var buttonHeight = height * 0.09f;
            var buttonY = height - height * 0.14f;

            if (GUI.Button(new Rect(20, buttonY, buttonWidth, buttonHeight),
                    _recordingPreview.IsPlaying ? "PAUSE" : "PLAY"))
            {
                _recordingPreview.TogglePlay();
            }

            if (GUI.Button(new Rect(30 + buttonWidth, buttonY, buttonWidth, buttonHeight), "SAVE"))
            {
                _screenRecorder.SaveRecording();
            }

            if (GUI.Button(new Rect(40 + buttonWidth * 2, buttonY, buttonWidth, buttonHeight), "CLOSE"))
            {
                _recordingPreview.Close();
            }
        }

        private static void DrawSolid(Rect rect, Color color)
        {
            var previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }

        public void SaveRecording()
        {
            _screenRecorder.SaveRecording();
        }

        public void StartRecording()
        {
            _screenRecorder.StartRecording(includeAudio: _includeAudio);
            _readyBytes = -1;
            _previewError = null;
        }

        public void StopRecording()
        {
            _screenRecorder.StopRecording();
        }

        public void OpenPreview()
        {
            StartCoroutine(OpenPreviewPanel());
        }

        public void ClosePreview()
        {
            _recordingPreview.Close();
            previewPanel.SetActive(false);
        }
    }
}
