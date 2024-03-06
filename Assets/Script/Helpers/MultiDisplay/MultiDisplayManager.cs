using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using YARG.Gameplay;
using YARG.Gameplay.HUD;
using YARG.Gameplay.Player;
using YARG.Player;

namespace YARG.Helpers.MultiDisplay
{
    public struct Display
    {
        public GameObject DisplayObject;
        public GameObject TrackViewObject;
        public VocalTrack VocalTrackObject;
        public Camera Camera;
    }

    public class MultiDisplayManager : MonoSingleton<MultiDisplayManager>
    {
        private static GameObject _multiDisplayCanvas;
        private static GameObject _trackViewCanvas;

        [SerializeField]
        private Camera _mainCamera;

        [SerializeField]
        private VocalTrack _vocalTrack;

        public int DisplayCount { get; private set; }

        private Dictionary<int, Display> _activeDisplays;

        private GameManager _gameManager;

        protected override void SingletonAwake()
        {
#if UNITY_EDITOR
            DisplayCount = 2;
#else
            DisplayCount = Display.displays.Length;
#endif
            _activeDisplays = new Dictionary<int, Display>();
            for (int i = 2; i <= DisplayCount; i++)
            {
                _activeDisplays.Add(i, new Display());
            }

            _multiDisplayCanvas = Addressables
                .LoadAssetAsync<GameObject>("MultiDisplayCanvas")
                .WaitForCompletion();

            _trackViewCanvas = Addressables
                .LoadAssetAsync<GameObject>("TrackViewCanvas")
                .WaitForCompletion();
        }

        protected void OnEnable()
        {
            ResetTargetDisplays();
        }

        public List<int> AddOptionsToDropdown(TMP_Dropdown dropdown)
        {
            dropdown.options.Clear();
            var list = new List<int>();

            for(int i = 1; i <= DisplayCount; i++)
            {
                dropdown.options.Add(new($"Display {i}"));
                list.Add(i);
            }

            return list;
        }

        public TrackViewManager GetTrackViewManager(int displayNumber)
        {
            TrackViewManager trackViewManager;
            if (!_activeDisplays.ContainsKey(displayNumber))
            {
                trackViewManager = GetComponentInChildren<TrackViewManager>(true);
            }
            else
            {
                trackViewManager = _activeDisplays[displayNumber].TrackViewObject.GetComponentInChildren<TrackViewManager>(true);
            }

            // Make sure to set up all of the HUD positions
            trackViewManager.SetAllHUDPositions();

            return trackViewManager;
        }

        public GameObject GetLyricBar(int displayNumber)
        {
            LyricBar lyricBar;
            if (!_activeDisplays.ContainsKey(displayNumber))
            {
                lyricBar = GetComponentInChildren<LyricBar>(true);
            }
            else
            {
                lyricBar = _activeDisplays[displayNumber].TrackViewObject.GetComponentInChildren<LyricBar>(true);
            }

            return lyricBar.gameObject;
        }

        public VocalTrack GetVocalTrack(int displayNumber)
        {
            VocalTrack vocalTrack;
            if (!_activeDisplays.ContainsKey(displayNumber))
            {
                vocalTrack = _vocalTrack;
            }
            else
            {
                vocalTrack = _activeDisplays[displayNumber].VocalTrackObject;
            }

            return vocalTrack;
        }

        public void ConnectPlayerToDisplay(YargPlayer player)
        {
#if !UNITY_EDITOR
            Display.displays[player.DisplayNumber - 1].Activate();
#endif
            ConnectDisplayTo(player.DisplayNumber);
        }

        public void DisconnectPlayerFromDisplay(YargPlayer player)
        {
            DisconnectDisplayTo(player.DisplayNumber);
        }

        public void ResetTargetDisplays()
        {
            List<YargPlayer> players;

           _gameManager = FindObjectOfType<GameManager>();
            if (_gameManager == null)
            {
                players = PlayerContainer.Players.ToList();
            }
            else
            {
                players = _gameManager.YargPlayers.ToList();
            }

            foreach (var displayNumber in new Dictionary<int, Display>(_activeDisplays).Keys)
            {
                if (players.Any(player => player.DisplayNumber == displayNumber))
                {
                    ConnectDisplayTo(displayNumber);
                }
                else
                {
                    DisconnectDisplayTo(displayNumber);
                }
            }
        }

        private void ConnectDisplayTo(int displayNumber)
        {
            if (!_activeDisplays.ContainsKey(displayNumber))
            {
                return;
            }

            var activeDisplay = _activeDisplays[displayNumber];
            if (activeDisplay.DisplayObject == null)
            {
                activeDisplay.DisplayObject = Instantiate(_multiDisplayCanvas, transform.root);
                activeDisplay.DisplayObject.name += $" {displayNumber}";

                activeDisplay.Camera = Instantiate(_mainCamera, _mainCamera.transform);
                activeDisplay.Camera.targetDisplay = displayNumber - 1;

                var displayCanvas = activeDisplay.DisplayObject.GetComponent<Canvas>();
                displayCanvas.worldCamera = activeDisplay.Camera;

                if (_gameManager != null)
                {
                    activeDisplay.TrackViewObject = Instantiate(_trackViewCanvas, transform);
                    var trackViewCanvas = activeDisplay.TrackViewObject.GetComponent<Canvas>();
                    trackViewCanvas.worldCamera = activeDisplay.Camera;

                    activeDisplay.VocalTrackObject = Instantiate(_vocalTrack, _vocalTrack.transform.parent);
                    activeDisplay.VocalTrackObject.gameObject.SetActive(false);
                }

                _activeDisplays[displayNumber] = activeDisplay;
            }
            else
            {
                activeDisplay.DisplayObject.SetActive(true);
                activeDisplay.Camera.gameObject.SetActive(true);
            }
        }

        private void DisconnectDisplayTo(int displayNumber)
        {
            if (!_activeDisplays.ContainsKey(displayNumber) || _activeDisplays[displayNumber].DisplayObject == null)
            {
                return;
            }

            _activeDisplays[displayNumber].DisplayObject.SetActive(false);
            _activeDisplays[displayNumber].Camera.gameObject.SetActive(false);
        }

    }
}
