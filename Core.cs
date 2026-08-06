using FasterDealers.Utils;
using MelonLoader;
using S1API.Entities;
using S1API.Entities.NPCs.Docks;
using S1API.Entities.NPCs.Downtown;
using S1API.Entities.NPCs.Northtown;
using S1API.Entities.NPCs.Suburbia;
using S1API.Entities.NPCs.Uptown;
using S1API.Entities.NPCs.Westville;
using S1API.Lifecycle;
using S1API.Logging;

[assembly: MelonInfo(typeof(FasterDealers.Core), Constants.MOD_NAME, Constants.MOD_VERSION, Constants.MOD_AUTHOR)]
[assembly: MelonGame(Constants.Game.GAME_STUDIO, Constants.Game.GAME_NAME)]
[assembly: MelonAuthorColor(1, 68, 2, 152)]
[assembly: MelonColor(1, 68, 2, 152)]

namespace FasterDealers
{
    public class Core : MelonMod
    {
        public static Core? Instance { get; private set; }

        private static MelonPreferences_Category? _fasterDealersCategory;
        private static MelonPreferences_Entry<bool>? _modEnabled;
        private static MelonPreferences_Entry<float>? _speedMultiplier;

        private Log _logger = new Log(Constants.PREFERENCES_CATEGORY);

        public override void OnInitializeMelon()
        {
            Instance = this;
            LoadConfig();

            if (!_modEnabled!.Value)
            {
                _logger.Msg($"{Constants.MOD_NAME} is disabled, skipping.");
                return;
            }

            GameLifecycle.OnLoadComplete += ApplyDealerSpeedMultiplier;
            _logger.Msg($"{Constants.MOD_NAME} initialized and config loaded. Waiting for Main scene to load.");
        }

        private void ApplyDealerSpeedMultiplier()
        {
            NPC?[] dealers =
            {
                NPC.Get<BenjiColeman>(),
                NPC.Get<MollyPresley>(),
                NPC.Get<BradCrosby>(),
                NPC.Get<JaneLucero>(),
                NPC.Get<WeiLong>(),
                NPC.Get<LeoRivers>()
            };

            try
            {
                foreach (var dealer in dealers)
                {
                    dealer?.Movement.SpeedMultiplier = _speedMultiplier!.Value;
                }

                _logger.Msg("Dealer speed multiplier applied successfully.");
            }
            catch
            {
                _logger.Msg("Failed to apply dealer speed multiplier.");
            }
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