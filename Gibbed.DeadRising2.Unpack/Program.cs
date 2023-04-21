using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;
using Gibbed.DeadRising2.FileFormats;
using Gibbed.Helpers;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using NDesk.Options;

namespace Gibbed.DeadRising2.Unpack
{
	public class Program
	{
		public static List<string> AreaFiles = new List<string>
		{
			"americana_casino.txt", "arena_backstage.txt", "atlantica_casino.txt", "food_barn.txt", "fortune_exterior.txt", "palisades.txt", "rooftop_atlantica.txt", "rooftop_hotel.txt", "rooftop_royal.txt", "rooftop_safehouse.txt",
			"rooftop_theater.txt", "rooftop_yucatan.txt", "royal_flush.txt", "safehouse.txt", "south_plaza.txt", "underground.txt", "yucatan_casino.txt", "laboratory.txt", "tkot_battle.txt"
		};

		public static List<string> DontRandomize = new List<string> { "Pad", "pad", "ArenaArrow", " arrow", "Floor", "floor", "Door", "door", "crane" };

		public static List<string> DR2ItemList = new List<string>
		{
			"MagicianSword", "BigDrillMotor", "LongStick", "ShowerHead", "RotatingDisplay", "VelvetPole", "VelvetRope", "LargeShoppingBoxes", "ZombieThrowerSpear", "NoveltyBeerMug",
			"LibertyTorch", "TikiMask", "TreasureChest", "MilitaryCrate", "HeadStatue", "PatioTable", "Bench_2", "PottedPlant_8", "PortableLawnMower", "PowerDrill",
			"MiningPick", "Drum", "GarbageCan_1", "HockeyStick", "Barrel_Large", "Pallet", "BowlingBall", "Hanger", "HunkOfMeat", "Scissors",
			"SmallSuitcase", "NewspaperBox", "DonkeyLamp", "SawBlade", "Crowbar", "SandwichBoard", "ProtesterSign", "LizardMask", "TwoByFour", "Football",
			"BowieKnife", "SpoolWire", "Generator", "Parasol", "GumballMachine", "PaintCan", "FunnyPainting", "PeaceArt", "Dumbbell", "BeachBall",
			"MedicineBall", "SledgeHammer", "LeadPipe", "WaterBottle", "WaterBottle_fromcooler", "WaterCooler", "GiantStuffedDonkey", "GiantStuffedElephant", "GiantStuffedBull", "Tomahawk",
			"ComputerCase", "Keyboard", "Mannequin_Female", "Mannequin_Male", "Swordfish", "BoxingGloves", "ServbotMask", "LawnDart", "SoccerBall", "Golfball",
			"Machete", "KnifeGloves", "Coffeepot", "BeerHat", "ConstructionHat", "Lamp", "MetalBarricade", "GoblinMask", "ZombieMask", "WackyHammer",
			"KatanaSword", "Basketball", "Spear", "GarbageCan_2", "Chair_1", "Chair_2", "Chair_3", "Painting3", "Painting2", "PottedPlant_1",
			"PottedPlant_2", "PottedPlant_3", "PottedPlant_4", "GarbageBag", "Mailbox", "PottedPlant_5", "PottedPlant_6", "CookingPot", "GarbageCan_3", "Chair_4",
			"PlasticBin", "CementSaw", "NoveltyPokerChip", "FoamHand", "Vase2", "SquareSign", "ComedyTrophy", "Chair_5", "Chair_6", "Chair_7",
			"Chair_8", "Chair_9", "RCHelicopter", "Bench", "LcdMonitor", "MoneyCase", "SC37_ShoppingBoxes", "GarbageCan_4", "NoveltyCellPhone", "Shampoo",
			"CementSaw_combo", "PottedPlant_7", "FlamingGloves", "HeliBlade", "GiftShopLamp", "LaserLightSword", "SpikedBat", "Chair_10", "GiantPinkChainsaw", "FountainLizard",
			"Flashlight", "BingoBall", "ParaBlower", "SuperSlicer", "VacuumCleaner", "SpeakerWeapon", "Tikitorch", "NoveltyBottle", "AcousticGuitar", "ElectricThunder",
			"PowerExsanguinator", "WoodSwordAndShield", "SpikedSwordAndShield", "FlamingSwordAndShield", "NoveltyPerfume", "PoleWeapon", "CardboardCutout", "GiantDice", "Particle_Board", "Amp",
			"HotelLamp", "BullSkull", "MMAGloves", "MMAGloves_spiked", "TeslaBall", "WrenchLarge", "Defiler", "Vase3", "BattleAxe", "FlamingTennisBall",
			"BobsToy", "Newspaper", "BurningSkull", "CarTire", "SteelShelving", "WingManJar", "Driller", "Auger", "ElectricRake", "HandleBar",
			"BikeForks", "WheelPawn", "BikeEngine", "BowlingPin", "FuelTank", "BassGuitar", "ElectricGuitar", "BaseballBat", "PitchFork", "CashRegister",
			"CardboardBox", "BroomHandle", "PushBroom", "HandBag", "Suitcase", "RouletteWheel", "Broadsword", "MooseHead", "Bucket", "Combo_BucketDrill",
			"MicStand", "CroupierStick", "PonyOnStick", "NightStick", "Pan", "Painting", "Pylon", "Lance", "Battery", "LeafRake",
			"MeatCleaver", "ChefKnife", "GiantStuffedBear", "GiantStuffedRabbit", "FireAxe", "Paddle", "CookingOil", "MotorOil", "spar_dummy", "Pan_heat",
			"GasBarrel", "Baseball", "Brick", "ATM", "ATM_bankrun", "StepLadder", "AdBoard", "ChainLink", "SpotLight", "Bust_Centurion",
			"MercAssaultRifle", "SixShooter", "Shotgun", "M16", "M249", "WaterGun", "HandGun", "Barrett50Caliber", "PitchForkShotgun", "TeddyBearSentryGun",
			"SpitballGun", "FireSpitter", "MinigameSniper", "BFG", "SBFG", "SnowballCannon", "DrinkCocktail", "HotDog", "Hamburger", "Vodka",
			"Steak", "Lobster", "Pizza", "Taco", "Burrito", "OnionRing", "Chili", "Sushi", "Fish", "BBQChicken",
			"BBQRibs", "Pasta", "Fries", "BakedPotato", "Beans", "Bacon", "Pineapple", "Snack", "Brownie", "Cookies",
			"IceCream", "Pie", "Cake", "Donuts", "OrangeJuice", "Milk", "CoffeeCreamer", "LargeSoda", "Melon", "Coffee",
			"Wine", "Beer", "EnergizerJuice", "NectarJuice", "QuickStepJuice", "RandomizerJuice", "SpitfireJuice", "UntouchableJuice", "ZombaitJuice", "PainKillerJuice",
			"Apple", "RepulseJuice", "SpoiledBacon", "SpoiledBBQChicken", "SpoiledBBQRibs", "SpoiledFish", "SpoiledLobster", "SpoiledHamburger", "SpoiledHotDog", "SpoiledSteak",
			"SpoiledSushi", "Whiskey", "Jellybeans", "EquipmentBox", "GasolineCanister", "PropaneTank", "Grenade", "BoykinGrenade", "Combo_PropaneTankNails", "MolotovBottle",
			"QueenBeeJar", "Dynamite", "FireCracker", "RomanCandle", "Flare", "StickyBomb", "HailMary", "BagofMarbles", "BazookaRocket", "FreezerBomb",
			"FireworkRockets", "AcetyleneTank", "Keg", "TrainThrowTank", "Dynameat", "GasCanister", "FireExtinguisher", "FlameThrower", "AirHorn", "LeafBlower",
			"Ketchup", "Mustard", "ATMHacker", "Spraypaint", "WhippedCream", "Mayonnaise", "PowerGuitar", "FireworksBazooka", "BowandArrow", "Plates",
			"BaseballBat_Metal", "TennisRacket", "FlamingAces", "GolfClub", "VinylRecords", "MusicDiscs", "PlateLauncher", "NineIron", "ZombieThrower", "FireCrackers",
			"Gems", "PlayingCards", "CasinoChips", "BoxOfNails", "GemBlower"
		};

		public static List<string> OTRItemList = new List<string>
		{
			"FireCrackers", "Gems", "PlayingCards", "CasinoChips", "BoxOfNails", "GemBlower", "Combo_RemoteControlBucketDrill1", "Combo_RemoteControlBucketDrill2", "Combo_RemoteControlBucketDrill3", "Combo_RemoteControlBucketDrill4",
			"GasolineCanister", "PropaneTank", "Grenade", "BoykinGrenade", "Combo_PropaneTankNails", "MolotovBottle", "PostmanMailbomb", "QueenBeeJar", "Dynamite", "FireCracker",
			"RomanCandle", "SullivanFlare_PlayerVersion", "Flare", "StickyBomb", "HailMary", "GunshipMinorExplosion", "KcFireworkExplosion", "GunshipMajorExplosion", "HatRemoverBlue", "BagofMarbles",
			"BazookaRocket", "BazookaRocket_Bomber", "MascotPaintBall", "SGSpecialZombieEffect", "FreezerBomb", "FireworkRockets_single", "FireworkRockets", "AcetyleneTank", "Keg", "Arrow_explosive",
			"Dynameat", "Snowball", "GasCanister", "Bazooka_Bike_Rocket", "CageFireworkExplosion", "FlashGrenade", "LiquidNitrogen", "StaceyAcetyleneTank", "LaserGunExplosion", "Protoball",
			"RemoteMine", "Pegasus", "ChuckMolotov", "ChuckMolotovLeft", "StaceyGrenade", "BouncingBeauty", "CryoPod", "StaceyMissile", "CryoPod_dmg", "MercGrenade",
			"StaceyMissileHeatSeeker", "SnowballStilts", "LiquidNitrogenStilts", "MercAssaultRifle", "SixShooter", "HangmanGun", "TrainThugMachinegun", "ThugMachinegun", "BoykinMachinegun", "HelicopterMiniGun",
			"MilitiaSniperRifle", "PostmanShotgun", "GunshipAsFirearm", "KCGun", "Shotgun", "MercArmyM16", "M16", "M249", "M249BikeGun", "WaterGun",
			"HandGun", "Barrett50Caliber", "SullivanHandGun", "PitchForkShotgun", "MechanicPitchForkShotgun", "TeddyBearSentryGun", "WheelChairTankGun", "RebeccasGun", "SpitballGun", "FireSpitter",
			"MinigameSniper", "BFG", "SBFG", "SnowballCannon", "HelicopterMiniGun_PROLOG", "StaceyGun", "LaserGun", "LightningGun", "RayGun", "TennisBallLauncher",
			"SawLauncher", "MoltenCannon", "ProtomanBlaster", "Theremin", "StiltsSnowballCannon", "EnhancedM249", "EnhancedM16", "EnhancedMAR", "DrinkCocktail", "HotDog",
			"Hamburger", "Vodka", "Steak", "Lobster", "Pizza", "Taco", "Burrito", "OnionRing", "Chili", "Sushi",
			"Fish", "BBQChicken", "BBQRibs", "Pasta", "Fries", "BakedPotato", "Beans", "Bacon", "Pineapple", "Snack",
			"Brownie", "Cookies", "IceCream", "Pie", "Cake", "Donuts", "OrangeJuice", "Milk", "CoffeeCreamer", "LargeSoda",
			"Melon", "Coffee", "Wine", "Beer", "EnergizerJuice", "NectarJuice", "QuickStepJuice", "RandomizerJuice", "SpitfireJuice", "UntouchableJuice",
			"ZombaitJuice", "PainKillerJuice", "Apple", "AppleMoney", "RepulseJuice", "SpoiledBacon", "SpoiledBBQChicken", "SpoiledBBQRibs", "SpoiledFish", "SpoiledLobster",
			"SpoiledHamburger", "SpoiledHotDog", "SpoiledSteak", "SpoiledSushi", "Whiskey", "Jellybeans", "ChuckWhiskey", "Pretzel", "Popcorn", "CottonCandy",
			"SpaceSteak", "Magazine_Alcohol_Hangover_Cures", "Magazine_Food_Healthy_Choices", "Magazine_Food_The_World_Chef", "Magazine_Juice_Top_Ten_Drink_Mixes", "Magazine_Gambling_Wealth", "Magazine_PP_Weapon_Armed_And_Awesome", "Magazine_Shotokan_Karate_Made_Easy", "Magazine_Leadership_For_Losers", "Magazine_PP_Psycho_Dangerous_People_Weekly",
			"Magazine_PP_Survivor_Angel_Prince", "Magazine_Gambling_Luck_You", "Magazine_PP_Weapon_Basic_Training_Monthly", "Magazine_Item_Edged_Lone_Blade", "Mgz_Item_Construction_Jackhammer_Weekly", "Magazine_Item_Toy_Toyapalooza", "Magazine_Item_Furniture_Designer_For_Homes", "Magazine_Item_Sports_Total_Sporting_Weekly", "Mgz_Item_Entertainment_Video_Game_Weekly", "Magazine_Sports_To_The_Extreme",
			"Magazine_Four_Wheel_Fun", "Magazine_Stunt_Devils", "Magazine_Gambling_Fortune_City_Riches", "Magazine_PP_Zombie_Spectral_Talkers", "Magazine_PP_Zombie_Undead_Solutions", "Magazine_PP_Weapon_Fortune_Fighter", "Magazine_Economy_Thrifty_Trader", "Magazine_Economy_Lords_Of_Cash", "Magazine_PP_Female_Survivor_Playboy", "MagicianSword",
			"BigDrillMotor", "LongStick", "ShowerHead", "RotatingDisplay", "VelvetPole", "VelvetRope", "LargeShoppingBoxes", "BagofMarbles_debris", "ZombieThrowerSpear", "NoveltyBeerMug",
			"LibertyTorch", "TikiMask", "TreasureChest", "MilitaryCrate", "HeadStatue", "PatioTable", "Bench_2", "PottedPlant_8", "SuperBike_dmg", "ChopperBikeSingle_dmg",
			"GirlsBike_dmg", "Shovel", "Microscope", "Sickle", "PopCan", "defibrillator_l", "defibrillator_r", "Defibrillator", "Katana", "CinderBlock",
			"ElectricProd", "MedicalTray", "Crate", "PoolBall", "PoolCue_dmg", "PoolCue", "shocker_l", "shocker_r", "Shocker", "Reaper",
			"PortableLawnMower", "PowerDrill", "MiningPick", "Drum", "Drum_dmg", "GarbageCan_1", "HockeyStick", "Barrel_Large", "Barrel_Large_dmg", "Pallet",
			"BowlingBall", "Hanger", "HunkOfMeat", "Scissors", "SmallSuitcase", "NewspaperBox", "DonkeyLamp", "SawBlade", "Crowbar", "SandwichBoard",
			"ProtesterSign", "LizardMask", "TwoByFour", "Football", "BowieKnife", "SpoolWire", "Generator", "Intestine", "Gems_Pieces", "Plate_Single",
			"Parasol", "GumballMachine", "GumballMachine_debris", "PaintCan", "FunnyPainting", "PeaceArt", "Dumbbell", "BeachBall", "MedicineBall", "SledgeHammer",
			"LeadPipe", "WaterBottle", "WaterBottle_fromcooler", "WaterCooler", "GiantStuffedDonkey", "GiantStuffedDonkey_dmg", "GiantStuffedElephant", "GiantStuffedElephant_dmg", "GiantStuffedBull", "GiantStuffedBull_dmg",
			"Tomahawk", "ComputerCase", "Keyboard", "MusicDisc_Single", "MannequinFemaleRightArm", "MannequinFemaleLeftArm", "MannequinFemaleRightLeg", "MannequinFemaleLeftLeg", "MannequinFemaleHead", "MannequinFemaleHeadTorso",
			"MannequinFemaleWaistRLegRFoot", "MannequinFemaleLLegLFoot", "Mannequin_Female", "MannequinMaleRightLeg", "MannequinMaleLeftLeg", "MannequinMaleTorsoLLegRLeg", "MannequinMaleRightArm", "MannequinMaleLeftArm", "MannequinMaleHead", "Mannequin_Male",
			"Swordfish", "BoxingGloves", "VinylRecord_Single", "VinylRecord_disc", "ServbotMask", "LawnDart", "SoccerBall", "FunnyPainting_dmg", "Golfball", "DeadTarget",
			"Machete", "KnifeGloves", "Coffeepot", "BeerHat", "ConstructionHat", "Lamp", "MetalBarricade", "GoblinMask", "ZombieMask", "Painting3_dmg",
			"Painting2_dmg", "WackyHammer", "KatanaSword", "Basketball", "Spear", "rknifeglove", "lknifeglove", "GarbageCan_2", "PlayingCards_Pieces", "CasinoChips_Pieces",
			"Chair_1", "Chair_2", "Chair_3", "Painting3", "Painting2", "NinjaSword", "NinjaBroadSword", "NinjaKatanaSword", "PottedPlant_1", "PottedPlant_2",
			"PottedPlant_3", "PottedPlant_4", "GarbageBag", "Mailbox", "PottedPlant_5", "PottedPlant_6", "CookingPot", "GarbageCan_3", "Chair_4", "PlasticBin",
			"CementSaw", "NoveltyPokerChip", "FoamHand", "Vase2", "SquareSign", "ComedyTrophy", "Chair_5", "Chair_6", "Chair_7", "Chair_8",
			"Chair_9", "Chair_9_dmg", "RCHelicopter", "Nails", "Bench", "LcdMonitor", "MoneyCase", "SC37_ShoppingBoxes", "PlateLauncherPlate", "GarbageCan_4",
			"NoveltyCellPhone", "Shampoo", "CementSaw_combo", "TIR_chair", "TIR_chair_dmg", "MoneyCase_TK", "MoneyCase_Americana", "MoneyCase_SlotRanch", "MoneyCase_Yucatan", "MoneyCase_Atlantica",
			"MoneyCase_Protester", "MoneyCase_Helicopter", "MoneyCase_Scoops", "PottedPlant_7", "FlamingGloves", "lflameglove", "rflameglove", "HeliBlade", "GiftShopLamp", "LaserLightSword",
			"SpikedBat", "Chair_10", "GiantPinkChainsaw", "FountainLizard", "Flashlight", "BingoBall_broken", "BingoBall", "ParaBlower", "SuperSlicer", "VacuumCleaner",
			"SpeakerWeapon", "Tikitorch", "NoveltyBottle", "AcousticGuitar", "AcousticGuitar_dmg", "ElectricThunder", "Arrow", "PowerExsanguinator", "WoodSwordAndShield", "WoodShield",
			"WoodSword", "SpikedSwordAndShield", "SpikedShield", "SpikedSword", "FlamingSwordAndShield", "FlamingShield", "FlamingSword", "NoveltyPerfume", "PoleWeapon", "SpitballGun_ball",
			"FireSpitter_ball", "CardboardCutout", "GiantDice", "Gemblower_Pieces", "Particle_Board", "Amp", "HotelLamp", "BullSkull", "MMAGloves", "lmmaglove",
			"rmmaglove", "MMAGloves_spiked", "lmmaglove_spiked", "rmmaglove_spiked", "TeslaBall", "WrenchLarge", "Defiler", "Vase3", "BattleAxe", "TennisBall",
			"FlamingTennisBall", "BobsToy", "Newspaper", "BurningSkull", "CarTire", "SteelShelving", "WingManJar", "Driller", "Auger", "ElectricRake",
			"lwn_flower", "MinigameMooseHeadRed", "SGProjectileBlue", "MinigameMooseHeadBlue", "MinigameMooseHeadGreen", "MinigameMooseHeadYellow", "MinigamePike", "SGProjectileRed", "SGProjectileYellow", "SGProjectileGreen",
			"StickIt_PonyOnStick", "stickin_painting", "stickit_ServbotMask", "stickit_ServbotMask_dmg", "stickin_painting_dmg", "robotclaw_2", "robotclaw_1", "robotclaw_3", "missile_crate", "GrassTrimmer",
			"RayGunDart", "Detonator", "TennisBallNew", "AluminumTennisRacket", "WeedTendonizer", "AlienHead", "EscapePod", "GiantSpaceshipToy", "ToyRocketShip", "AlienProbe",
			"TIR_Sign", "TIRTwoByFour", "TIRStepLadder", "FlamingTennisBallDC", "SawBladeProjectile", "TIRPan", "ProtomanShield", "ProtomanBlasterAndShield", "EscapePod_dmg", "MascotMask",
			"Thor", "LaserEyes", "SpaceHammer", "Mole", "Wormhole", "Spacerescueprop", "Spacerescueprop_1", "MassagerProjectile", "SpaceWorm", "SpaceWorm_Target",
			"Shootinggallery_prop1", "Shootinggallery_prop2", "Shootinggallery_prop3", "Spacebaseball_Target1", "Spacebaseball_Target2", "Spacebaseball_Target3", "TIRBaseballBat", "SpaceBench", "RobotBox", "Spacerescueprop_2",
			"StaceyMissileCrate", "robotarm_1", "Shootinggallery_prop4", "Chair_Casino", "TIODPlywood1", "TIODPlywood2", "SnowballBigStilts", "Sbaseball", "Knife", "FrankStandee",
			"ChuckStandee", "FiremanFireAxe", "GreenHunkOfMeat", "BowlingPin", "PsychoMachete", "PsychoMeatCleaver", "PsychoMiningPick", "HandleBar", "BikeForks", "WheelPawn",
			"BikeEngine", "FuelTank", "BassGuitar", "ElectricGuitar", "BaseballBat", "PitchFork", "CashRegister", "CashRegister_dmg", "CardboardBox", "BroomHandle",
			"PushBroom", "HandBag", "Suitcase", "RouletteWheel", "Broadsword", "MooseHead", "KCMooseHead", "Bucket", "Combo_BucketDrill", "MicStand",
			"CroupierStick", "PonyOnStick", "NightStick", "Pan", "ServingTray", "Painting", "Pylon", "Lance", "Painting_dmg", "Battery",
			"LeafRake", "MeatCleaver", "BBQChef_MeatCleaver", "ChefKnife", "GiantStuffedBear", "GiantStuffedBear_dmg", "GiantStuffedRabbit", "GiantStuffedRabbit_dmg", "FireAxe", "Paddle",
			"CookingOil", "MotorOil", "spar_dummy", "Pan_heat", "GasBarrel", "Baseball", "Brick", "ATM", "ATM_bankrun", "StepLadder",
			"AdBoard", "ChainLink", "RopeLink", "SpotLight", "Bust_Centurion", "lboxglove", "rboxglove", "EquipmentBox", "RB_couch", "tree_pine",
			"Vase", "explodable_head", "explodable_leg", "explodable_arm", "explodable_upper_body", "explodable_full_body", "frozen_leg", "frozen_upper_body", "intestine_organ", "FireworksBazooka",
			"FireworksBazooka_Bomber", "BowandArrow", "BowandArrow_explosive", "Plates", "BaseballBat_Metal", "TennisRacket", "FlamingAces", "GolfClub", "VinylRecords", "MusicDiscs",
			"PlateLauncher", "SGBallLauncherBlue", "SGBallLauncherRed", "SGBallLauncherGreen", "SGBallLauncherYellow", "FireworksBazooka_reward", "NineIron", "ZombieThrower", "MascotMurderballLauncher", "boss_tankrack",
			"StaceyMissileLauncher", "SuperMassager", "SpaceballBat", "CannedDrinks", "QueenBee", "WingmanBee", "Skateboard", "FireExtinguisher", "FlameThrower", "AirHorn",
			"LeafBlower", "Ketchup", "Mustard", "ATMHacker", "Spraypaint", "WhippedCream", "Mayonnaise", "PowerGuitar"
		};

		public static List<string> ProgressMessage = new List<string>
		{
			"Americana Randomized!", "Arena Randomized!", "Atlantica Randomized!", "Food Court Randomized!", "Strip Randomized!", "Palisades Randomized!", "Atlantica Rooftop Randomized!", "Hotel Rooftop Randomized!", "Royal Flush Rooftop Randomized!", "Safehouse Rooftop Randomized!",
			"Movie Theatre Rooftop Randomized!", "Yucatan Rooftop Randomized!", "Royal Flush Randomized!", "Safehouse Randomized!", "South Plaza Randomized!", "Tunnels Randomized!", "Yucatan Randomized!", "Laboratory Randomized!", "TK Arena Randomized!"
		};

		public static List<string> TempList = new List<string>();

		public static List<string> UsableList = new List<string>();

		public static string randomitem;

		public static void Main(string[] args)
		{
			bool showHelp = false;
			OptionSet optionSet = new OptionSet { 
			{
				"h|help",
				"show this message and exit",
				delegate(string v)
				{
					showHelp = v != null;
				}
			} };
			List<string> list;
			try
			{
				list = optionSet.Parse(args);
			}
			catch (OptionException ex)
			{
				Console.Write("{0}: ", GetExecutableName());
				Console.WriteLine(ex.Message);
				Console.WriteLine("Try `{0} --help' for more information.", GetExecutableName());
				return;
			}
			if (list.Count < 1 || list.Count > 2 || showHelp)
			{
				string text;
				while (true)
				{
					Console.WriteLine("No datafile specified.\n\nPlease add a path for the datafile you want to randomize.\n");
					text = Console.ReadLine();
					if (File.Exists(text) && Path.GetExtension(text) == ".big")
					{
						break;
					}
					Console.WriteLine("\nInvalid path specificed.\n");
				}
				list.Add(text);
			}
			string path = list[0];
			string text2 = "RandomizedFiles";
			using FileStream fileStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			BigFile bigFile = new BigFile();
			bigFile.Deserialize(fileStream);
			foreach (BigFile.Entry entry in bigFile.Entries)
			{
				if (entry.CompressionScheme == BigFile.CompressionScheme.XBox)
				{
					Console.Write("Warning: this archive contains XBox compressed data which won't be uncompressed");
					break;
				}
			}
			Directory.CreateDirectory(text2);
			XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
			xmlWriterSettings.Indent = true;
			using XmlWriter xmlWriter = XmlWriter.Create(Path.Combine(text2, "bigfile.xml"), xmlWriterSettings);
			xmlWriter.WriteStartDocument();
			xmlWriter.WriteStartElement("files");
			xmlWriter.WriteAttributeString("version", "2");
			byte[] array = new byte[16384];
			long num = 0L;
			foreach (BigFile.Entry entry2 in bigFile.Entries)
			{
				xmlWriter.WriteStartElement("entry");
				xmlWriter.WriteAttributeString("name", entry2.Name);
				if (entry2.CompressionScheme != 0)
				{
					if (entry2.CompressionScheme == BigFile.CompressionScheme.ZLib)
					{
						xmlWriter.WriteAttributeString("scheme", "zlib");
					}
					else
					{
						if (entry2.CompressionScheme != BigFile.CompressionScheme.XBox)
						{
							throw new InvalidOperationException("unsupported compression scheme");
						}
						xmlWriter.WriteAttributeString("scheme", "xbox");
					}
				}
				xmlWriter.WriteAttributeString("alignment", entry2.Alignment.ToString());
				string name = entry2.Name;
				name = name.Replace('/', Path.DirectorySeparatorChar);
				name = Path.GetFileName(name);
				if (bigFile.DuplicateNames)
				{
					name = $"{num:D4}_{name}";
				}
				string path2 = Path.Combine(text2, name);
				Directory.CreateDirectory(Path.GetDirectoryName(path2));
				using (Stream stream = File.Open(path2, FileMode.Create, FileAccess.Write, FileShare.Read))
				{
					if (entry2.CompressionScheme == BigFile.CompressionScheme.None || entry2.CompressionScheme == BigFile.CompressionScheme.XBox)
					{
						fileStream.Seek(entry2.Offset, SeekOrigin.Begin);
						stream.WriteFromStream(fileStream, entry2.UncompressedSize);
					}
					else
					{
						if (entry2.CompressionScheme != BigFile.CompressionScheme.ZLib)
						{
							throw new NotSupportedException("unhandled compression scheme");
						}
						fileStream.Seek(entry2.Offset, SeekOrigin.Begin);
						uint num2 = fileStream.ReadValueU32(littleEndian: false);
						if (num2 != entry2.UncompressedSize)
						{
							throw new InvalidOperationException("entry size mismatch for decompression");
						}
						int num3 = (int)num2;
						InflaterInputStream inflaterInputStream = new InflaterInputStream(fileStream, new Inflater(noHeader: false));
						while (num3 > 0)
						{
							int num4 = inflaterInputStream.Read(array, 0, Math.Min(array.Length, num3));
							if (num4 < 0)
							{
								throw new InvalidOperationException("zlib error");
							}
							if (num4 == 0)
							{
								throw new InvalidOperationException("zero read");
							}
							stream.Write(array, 0, num4);
							num3 -= num4;
						}
					}
				}
				xmlWriter.WriteString(name);
				xmlWriter.WriteEndElement();
				num++;
			}
			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndDocument();
			xmlWriter.Flush();
			if (File.Exists("RandomizedFiles\\theme_park.txt"))
			{
				DR2ItemList = OTRItemList;
				AreaFiles.Add("theme_park.txt");
				ProgressMessage.Add("Uranus Zone Randomized!");
			}
			Randomize();
		}

		public static string GetExecutableName()
		{
			return Path.GetFileName(Assembly.GetExecutingAssembly().CodeBase);
		}

		public static void Randomize()
		{
			try
			{
				string path = "RandomizedFiles\\" + AreaFiles.ElementAt(0);
				string[] array = File.ReadAllLines(path);
				for (int i = 0; i < array.Length; i++)
				{
					string text = array[i];
					if (text.Contains("cItemPlacement") && !DontRandomize.Any(text.Contains))
					{
						string text2 = text.Split(' ')[1];
						if (!TempList.Contains(text2) && !DR2ItemList.Contains(text2) && AreaFiles.ElementAt(0) != "south_plaza.txt")
						{
							TempList.Add(text2);
						}
						Random random = new Random();
						UsableList.AddRange(TempList);
						UsableList.AddRange(DR2ItemList);
						int index = random.Next(UsableList.Count);
						randomitem = UsableList.ElementAt(index);
						text = text.Replace(text2 ?? "", randomitem ?? "");
					}
					if (text.Contains("ItemName") && !DontRandomize.Any(text.Contains))
					{
						text = string.Format("ItemName = \"" + randomitem + "\"");
					}
					array[i] = text;
				}
				File.WriteAllLines(path, array);
				Console.WriteLine(ProgressMessage.ElementAt(0));
				ProgressMessage.RemoveAt(0);
				AreaFiles.RemoveAt(0);
				TempList.Clear();
				UsableList.Clear();
				Randomize();
			}
			catch (FileNotFoundException)
			{
				AreaFiles.RemoveAt(0);
				ProgressMessage.RemoveAt(0);
				Randomize();
			}
			catch (ArgumentOutOfRangeException)
			{
				Repack();
			}
		}

		public static void Repack()
		{
			string text = "RandomizedFiles";
			string path = Path.ChangeExtension("datafile", ".big");
			if (Directory.Exists(text))
			{
				string text2 = Path.Combine(text, "bigfile.xml");
				if (File.Exists(text2))
				{
					text = text2;
				}
			}
			List<MyEntry> list = new List<MyEntry>();
			using (FileStream stream = File.Open(text, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				XPathNavigator xPathNavigator = new XPathDocument(stream).CreateNavigator().SelectSingleNode("/files");
				string attribute = xPathNavigator.GetAttribute("version", "");
				uint result = 1u;
				if (!string.IsNullOrEmpty(attribute) && !uint.TryParse(attribute, out result))
				{
					throw new FormatException("invalid XML version");
				}
				if (result < 1 || result > 2)
				{
					throw new FormatException("unsupported XML version");
				}
				XPathNodeIterator xPathNodeIterator = xPathNavigator.Select("entry");
				while (xPathNodeIterator.MoveNext())
				{
					XPathNavigator current = xPathNodeIterator.Current;
					string attribute2 = current.GetAttribute("name", "");
					if (string.IsNullOrEmpty(attribute2))
					{
						throw new FormatException("entry name cannot be null or empty");
					}
					BigFile.CompressionScheme compressionScheme = BigFile.CompressionScheme.None;
					if (result == 1)
					{
						if (!uint.TryParse(current.GetAttribute("flags", ""), out var result2))
						{
							throw new FormatException("entry has an invalid flags value");
						}
						if (1 == 0)
						{
						}
						BigFile.CompressionScheme compressionScheme2 = result2 switch
						{
							0u => BigFile.CompressionScheme.None, 
							1u => BigFile.CompressionScheme.ZLib, 
							_ => throw new FormatException("entry has an unsupported flags value"), 
						};
						if (1 == 0)
						{
						}
						compressionScheme = compressionScheme2;
					}
					else
					{
						string attribute3 = current.GetAttribute("scheme", "");
						if (!string.IsNullOrEmpty(attribute3))
						{
							string text3 = attribute3.ToLowerInvariant();
							if (1 == 0)
							{
							}
							BigFile.CompressionScheme compressionScheme2 = text3 switch
							{
								"none" => BigFile.CompressionScheme.None, 
								"zlib" => BigFile.CompressionScheme.ZLib, 
								"xbox" => BigFile.CompressionScheme.XBox, 
								_ => throw new FormatException("entry has an unsupported compression scheme"), 
							};
							if (1 == 0)
							{
							}
							compressionScheme = compressionScheme2;
						}
					}
					if (!uint.TryParse(current.GetAttribute("alignment", ""), out var result3))
					{
						throw new FormatException("entry has an invalid alignment value");
					}
					string text4 = current.Value;
					if (string.IsNullOrEmpty(text4))
					{
						throw new FormatException("entry path cannot be null or empty");
					}
					if (!Path.IsPathRooted(text4))
					{
						text4 = Path.Combine(Path.GetDirectoryName(text), text4);
						text4 = Path.GetFullPath(text4);
					}
					list.Add(new MyEntry
					{
						Name = attribute2,
						NameHash = Hash.Calculate(attribute2.ToLowerInvariant()),
						CompressedSize = 0u,
						UncompressedSize = 0u,
						Offset = 0u,
						Alignment = result3,
						CompressionScheme = compressionScheme,
						Path = text4
					});
				}
			}
			BigFile bigFile = new BigFile();
			bigFile.Version = 2u;
			foreach (MyEntry item in list)
			{
				bigFile.Entries.Add(item);
			}
			using (FileStream fileStream = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
			{
				fileStream.Seek(0L, SeekOrigin.Begin);
				bigFile.Serialize(fileStream);
				uint num = (uint)fileStream.Length;
				foreach (MyEntry item2 in list)
				{
					using FileStream fileStream2 = File.Open(item2.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
					uint num2 = (uint)fileStream2.Length;
					item2.Offset = num;
					item2.UncompressedSize = num2;
					fileStream.Seek(num, SeekOrigin.Begin);
					if (item2.CompressionScheme == BigFile.CompressionScheme.None)
					{
						item2.CompressedSize = num2;
						fileStream.WriteFromStream(fileStream2, num2);
						num += num2;
						num = num.Align(item2.Alignment);
						continue;
					}
					if (item2.CompressionScheme == BigFile.CompressionScheme.ZLib)
					{
						fileStream.WriteValueU32(num2, littleEndian: false);
						Deflater deflater = new Deflater(9, noZlibHeaderOrFooter: false);
						deflater.SetStrategy(DeflateStrategy.Default);
						DeflaterOutputStream deflaterOutputStream = new DeflaterOutputStream(fileStream, deflater)
						{
							IsStreamOwner = false
						};
						deflaterOutputStream.WriteFromStream(fileStream2, num2);
						deflaterOutputStream.Close();
						uint num3 = 0u;
						num3 += 4;
						num += (item2.CompressedSize = num3 + (uint)(int)deflater.TotalOut);
						num = num.Align(item2.Alignment);
						continue;
					}
					throw new NotSupportedException("unhandled compression scheme");
				}
				fileStream.Seek(0L, SeekOrigin.Begin);
				bigFile.Serialize(fileStream);
				fileStream.Seek(8L, SeekOrigin.Begin);
				fileStream.WriteValueU32((uint)fileStream.Length);
				Console.WriteLine("\nRandomization is finished. Press any key to continue.");
				Console.ReadKey();
				AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
			}
			static void OnProcessExit(object sender, EventArgs e)
			{
				Directory.Delete("RandomizedFiles", recursive: true);
			}
		}
	}
}
