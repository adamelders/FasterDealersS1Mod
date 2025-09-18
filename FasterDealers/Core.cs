using MelonLoader;
using ScheduleOne.NPCs;
using UnityEngine;

[assembly: MelonInfo(typeof(FasterDealers.Core), "FasterDealers", "1.0.0", "Riccaforte", null)]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace FasterDealers
{
    public class Core : MelonMod
    {
        private static MelonPreferences_Category fasterDealersCategory;
        private static MelonPreferences_Entry<bool> modEnabled;
        private static MelonPreferences_Entry<float> speedMultiplier;

        public override void OnInitializeMelon() {
            LoadConfig();
            LoggerInstance.Msg("Mod initialized and config loaded. Waiting for Main scene to load.");
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName) {
            base.OnSceneWasLoaded(buildIndex, sceneName);

            if (!modEnabled.Value) {
                LoggerInstance.Msg("FasterDealers mod is disabled, skipping.");
                return;
            }

            if (sceneName != "Main") {
                return; // Only run on the Main scene
            }

            string[] dealerList = ["Benji", "Molly", "Brad", "Jane", "Wei", "Leo"]; // TODO: would be nice to get all dealer "types" or base class

            foreach (string dealerName in dealerList) {
                SetDealerSpeedMultiplier(dealerName, speedMultiplier.Value);
            }

            LoggerInstance.Msg("Speed multiplier of " + speedMultiplier.Value.ToString() + " set for all dealer NPCs!");
        }

        private void SetDealerSpeedMultiplier(string dealerName, float speed) {
            GameObject dealer = GameObject.Find(dealerName);

            if (dealer != null) {
                NPCSpeedController component = dealer.GetComponent<NPCSpeedController>();
                if (component != null) {
                    component.SpeedMultiplier = speed;
                }
            } else {
                Melon<Core>.Logger.Error($"Dealer {dealerName} not found!");
            }
        }

        private void LoadConfig() {
            fasterDealersCategory = MelonPreferences.CreateCategory("FasterDealers");
            modEnabled = fasterDealersCategory.CreateEntry<bool>("Enabled", true);
            speedMultiplier = fasterDealersCategory.CreateEntry<float>("SpeedMultiplier", 3.0f);
        }
    }
}