using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using YARG.Gameplay;
using YARG.Player;

namespace YARG.Helpers.MultiDisplay
{
    public class MultiDisplayManager : MonoSingleton<MultiDisplayManager>
    {
        private static GameObject _multiDisplayCanvas;

        public int DisplayCount { get; private set; }

        private Dictionary<int, GameObject> _activeDisplays;

        protected override void SingletonAwake()
        {
#if UNITY_EDITOR
            DisplayCount = 2;
#else
            DisplayCount = Display.displays.Length;
#endif
            _activeDisplays = new Dictionary<int, GameObject>();
            for (int i = 2; i <= DisplayCount; i++)
            {
                _activeDisplays.Add(i, null);
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

            //Selecting display number dropdown is available for players with more than 1 monitor
            dropdown.interactable = DisplayCount > 1;
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

            foreach (var displayNumber in new Dictionary<int, GameObject>(_activeDisplays).Keys)
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

            if (_activeDisplays[displayNumber] == null)
            {
                _activeDisplays[displayNumber] = Instantiate(_multiDisplayCanvas, transform.root);
                _activeDisplays[displayNumber].name += $" {displayNumber}";
                var canvas = _activeDisplays[displayNumber].GetComponent<Canvas>();
                canvas.targetDisplay = displayNumber - 1;
            }
            else
            {
                _activeDisplays[displayNumber].SetActive(true);
            }
        }

        private void DisconnectDisplayTo(int displayNumber)
        {
            if (!_activeDisplays.ContainsKey(displayNumber) || _activeDisplays[displayNumber] == null)
            {
                return;
            }

            _activeDisplays[displayNumber].SetActive(false);
        }

    }
}
