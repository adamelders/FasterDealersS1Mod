using FasterDealers.Integrations;
using FasterDealers.Utils;
using MelonLoader;
using S1API.Entities;
using S1API.Logging;
using System.Collections;
using UnityEngine;

[assembly: MelonInfo(typeof(FasterDealers.Core), Constants.MOD_NAME, Constants.MOD_VERSION, Constants.MOD_AUTHOR)]
[assembly: MelonGame(Constants.Game.GAME_STUDIO, Constants.Game.GAME_NAME)]
[assembly: VerifyLoaderVersion(0, 7, 0, true)]
[assembly: MelonAuthorColor(1, 68, 2, 152)]
[assembly: MelonColor(1, 0, 223, 255)]

namespace FasterDealers
{
    public class Core : MelonMod
    {
        public static Core? Instance { get; private set; }

        private static MelonPreferences_Category fasterDealersCategory;
        private static MelonPreferences_Entry<bool> modEnabled;
        private static MelonPreferences_Entry<float> speedMultiplier;

        private Log _logger = new Log("FasterDealers");
        private bool _speedsBeingSet = false;
        private int _completedDealers = 0;
        private const int TOTAL_DEALERS = 6;
        private readonly object _lock = new object();
        private HashSet<string> _waitingDealers = new();

        public override void OnInitializeMelon()
        {
            Instance = this;
            HarmonyPatches.SetModInstance(this);
            LoadConfig();
            _logger.Msg("Mod initialized and config loaded. Waiting for Main scene to load.");
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            base.OnSceneWasInitialized(buildIndex, sceneName);

            if (!modEnabled.Value || sceneName != "Main")
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

            MelonCoroutines.Start(WaitAndSetDealerSpeed<S1API.Entities.NPCs.Northtown.BenjiColeman>(speedMultiplier.Value));
            MelonCoroutines.Start(WaitAndSetDealerSpeed<S1API.Entities.NPCs.Westville.MollyPresley>(speedMultiplier.Value));
            MelonCoroutines.Start(WaitAndSetDealerSpeed<S1API.Entities.NPCs.Downtown.BradCrosby>(speedMultiplier.Value));
            MelonCoroutines.Start(WaitAndSetDealerSpeed<S1API.Entities.NPCs.Docks.JaneLucero>(speedMultiplier.Value));
            MelonCoroutines.Start(WaitAndSetDealerSpeed<S1API.Entities.NPCs.Suburbia.WeiLong>(speedMultiplier.Value));
            MelonCoroutines.Start(WaitAndSetDealerSpeed<S1API.Entities.NPCs.Uptown.LeoRivers>(speedMultiplier.Value));

            // Wait for all dealers to be processed
            while (_completedDealers < TOTAL_DEALERS)
            {
                yield return new WaitForSeconds(0.5f);
            }

            _logger.Msg($"Speed multiplier of {speedMultiplier.Value} set for all dealer NPCs!");

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
            fasterDealersCategory = MelonPreferences.CreateCategory("FasterDealers");
            modEnabled = fasterDealersCategory.CreateEntry<bool>("Enabled", true);
            speedMultiplier = fasterDealersCategory.CreateEntry<float>("SpeedMultiplier", 3.0f);
        }
    }
}