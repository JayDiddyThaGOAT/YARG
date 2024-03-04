using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using YARG.Gameplay;
using YARG.Player;

namespace YARG.Helpers.MultiDisplay
{
    public struct Display
    {
        public GameObject DisplayObject;
        public Camera Camera;
    }

    public class MultiDisplayManager : MonoSingleton<MultiDisplayManager>
    {
        private static GameObject _multiDisplayCanvas;

        [SerializeField]
        private Camera _mainCamera;

        public int DisplayCount { get; private set; }

        private Dictionary<int, Display> _activeDisplays;

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

            var gameManager = FindObjectOfType<GameManager>();
            if (gameManager == null)
            {
                players = PlayerContainer.Players.ToList();
            }
            else
            {
                players = gameManager.YargPlayers.ToList();
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
                var canvas = activeDisplay.DisplayObject.GetComponent<Canvas>();
                canvas.targetDisplay = displayNumber - 1;

                activeDisplay.Camera = Instantiate(_mainCamera, _mainCamera.transform);
                activeDisplay.Camera.targetDisplay = displayNumber - 1;

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
