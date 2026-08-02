using FasterDealers.Integrations;
using FasterDealers.Utils;
using MelonLoader;
using S1API.Entities;
using S1API.Lifecycle;
using S1API.Logging;
using System.Collections;
using UnityEngine;

[assembly: MelonInfo(typeof(FasterDealers.Core), Constants.MOD_NAME, Constants.MOD_VERSION, Constants.MOD_AUTHOR)]
[assembly: MelonGame(Constants.Game.GAME_STUDIO, Constants.Game.GAME_NAME)]
[assembly: MelonAuthorColor(1, 68, 2, 152)]
[assembly: MelonColor(1, 0, 223, 255)]

namespace FasterDealers
{
    public class Core : MelonMod
    {
        public static Core? Instance { get; private set; }

        private static MelonPreferences_Category? _fasterDealersCategory;
        private static MelonPreferences_Entry<bool>? _modEnabled;
        private static MelonPreferences_Entry<float>? _speedMultiplier;

        private Log _logger = new Log(Constants.PREFERENCES_CATEGORY);
        private bool _speedsBeingSet = false;
        private int _completedDealers = 0;
        private const int TOTAL_DEALERS = 6;
        private readonly object _lock = new object();
        private HashSet<string> _waitingDealers = new();

        public override void OnInitializeMelon()
        {
            Instance = this;
            LoadConfig();
            HarmonyPatches.Initialize(this);
            _logger.Msg($"{Constants.MOD_NAME} initialized and config loaded. Waiting for Main scene to load.");
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            base.OnSceneWasInitialized(buildIndex, sceneName);

            if (!_modEnabled.Value || sceneName != "Main")
            {
                return;
            }

            lock (_lock)
            {
                if (_speedsBeingSet)
                {
                    _logger.Msg("Dealer speed setting already in progress, skipping...");
                    return;
                }
                _speedsBeingSet = true;
                _waitingDealers.Clear();
            }

            MelonCoroutines.Start(SetAllDealerSpeeds());
        }

        private IEnumerator SetAllDealerSpeeds()
        {
            _completedDealers = 0;

            MelonCoroutines.Start(WaitAndSetDealerSpeed<S1API.Entities.NPCs.Northtown.BenjiColeman>(_speedMultiplier.Value));
            MelonCoroutines.Start(WaitAndSetDealerSpeed<S1API.Entities.NPCs.Westville.MollyPresley>(_speedMultiplier.Value));
            MelonCoroutines.Start(WaitAndSetDealerSpeed<S1API.Entities.NPCs.Downtown.BradCrosby>(_speedMultiplier.Value));
            MelonCoroutines.Start(WaitAndSetDealerSpeed<S1API.Entities.NPCs.Docks.JaneLucero>(_speedMultiplier.Value));
            MelonCoroutines.Start(WaitAndSetDealerSpeed<S1API.Entities.NPCs.Suburbia.WeiLong>(_speedMultiplier.Value));
            MelonCoroutines.Start(WaitAndSetDealerSpeed<S1API.Entities.NPCs.Uptown.LeoRivers>(_speedMultiplier.Value));

            // Wait for all dealers to be processed
            while (_completedDealers < TOTAL_DEALERS)
            {
                yield return new WaitForSeconds(0.5f);
            }

            _logger.Msg($"Speed multiplier of {_speedMultiplier.Value} set for all dealer NPCs!");

            lock (_lock)
            {
                _speedsBeingSet = false;
                _waitingDealers.Clear();
            }
        }

        private IEnumerator WaitAndSetDealerSpeed<T>(float speed) where T : NPC
        {
            string dealerName = typeof(T).Name;
            NPC? dealer = null;
            bool loggedWaiting = false;

            while (dealer == null)
            {
                dealer = NPC.Get<T>();

                if (dealer == null)
                {
                    // Only log once per dealer when we start waiting
                    lock (_lock)
                    {
                        if (!loggedWaiting && _waitingDealers.Add(dealerName))
                        {
                            _logger.Msg($"Waiting for {dealerName} instance to be created...");
                            loggedWaiting = true;
                        }
                    }

                    yield return new WaitForSeconds(0.5f);
                }
            }

            dealer.Movement.SpeedMultiplier = speed;

            _logger.Msg($"Set speed multiplier to {speed} for {dealerName}");

            _completedDealers++;
        }

        private void LoadConfig()
        {
            _fasterDealersCategory = MelonPreferences.CreateCategory(Constants.PREFERENCES_CATEGORY);
            _modEnabled = _fasterDealersCategory.CreateEntry<bool>("Enabled", true, "Enable Faster Dealers", "Enables or disables Faster Dealers mod.");
            _speedMultiplier = _fasterDealersCategory.CreateEntry<float>("SpeedMultiplier", 3.0f, "Dealer Speed Multiplier", "Speed multiplier for all dealers as a float value. Default is 3.0f. Multiplies base dealer speed by this amount.");
            _fasterDealersCategory.SaveToFile(false);
        }
    }
}