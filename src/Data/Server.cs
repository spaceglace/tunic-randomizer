using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using static TunicRandomizer.SaveFlags;

namespace TunicRandomizer
{
    public class Server : MonoBehaviour
    {
        public static int TIMEOUT = 1000;
        public static string VERSION = "Collection";

        private static Dictionary<(string, string), string> Codes = new Dictionary<(string, string), string>
        {
            { ("Overworld", "SV_Overworld Redux_Obelisk_Solved"), "Golden Obelisk Page" },
            { ("Overworld", "SV_Fairy_3_Overworld_Moss_Revealed"), "Moss Wall Holy Cross" },
            { ("Overworld", "SV_Overworld Redux_Filigree_Door_Basic (1)"), "Fountain Cross Door" },
            { ("Overworld", "SV_Overworld Redux_Filigree_Door_Basic"), "Southeast Cross Door" },
            { ("Overworld", "SV_Overworld Redux_Windchime Chest - Environmental Filigree Safe (Non Fairy)"), "Windchimes Holy Cross" },
            { ("Overworld", "SV_Overworld Redux_Starting Island Trophy Chest"), "Starting Platform Holy Cross" },
            { ("Overworld", "SV_Fairy_11_WeatherVane_Revealed"), "Weathervane Holy Cross" },
            { ("Overworld", "SV_Fairy_1_Overworld_Flowers_Upper_Revealed"), "Northeast Flowers Holy Cross" },
            { ("Overworld", "SV_Overworld Redux_Windmill Chest - Environmental Filigree Safe (Non Fairy)"), "Windmill Holy Cross" },
            { ("Overworld", "SV_Fairy_16_Fountain_Revealed"), "Fountain Holy Cross" },
            { ("Overworld", "SV_Fairy_2_Overworld_Flowers_Lower_Revealed"), "Southwest Flowers Holy Cross" },
            { ("Overworld", "SV_Overworld Redux_Tropical Secret Chest - Environmental Filigree Safe (Non Fairy)"), "Haiku Holy Cross" },
            { ("Cube Cave", "SV_Fairy_14_Cube_Revealed"), "Holy Cross Chest" },
            { ("Caustic Light Cave", "SV_Fairy_4_Caustics_Revealed"), "Holy Cross Chest" },
            { ("Ruined Passage", "SV_Ruins Passage_secret filigree door"), "Holy Cross Door" },
            { ("Patrol Cave", "SV_Fairy_13_Patrol_Revealed"), "Holy Cross Chest" },
            { ("Secret Gathering Place", "SV_Fairy_5_Waterfall_Revealed"), "Holy Cross Chest" },
            { ("Hourglass Cave", "SV_Town Basement_secret filigree door"), "Holy Cross Door" },
            { ("Hourglass Cave", "SV_Fairy_10_3DPillar_Revealed"), "Holy Cross Chest" },
            { ("Maze Cave", "SV_Fairy_15_Maze_Revealed"), "Maze Room Holy Cross" },
            { ("Old House", "SV_Fairy_12_House_Revealed"), "Holy Cross Chest" },
            { ("Old House", "SV_Overworld Interiors_Filigree_Door"), "Holy Cross Door" },
            { ("Sealed Temple", "SV_Fairy_6_Temple_Revealed"), "Holy Cross Chest" },
            { ("East Forest", "SV_Fairy_8_Dancer_Revealed"), "Dancing Fox Spirit Holy Cross" },
            { ("East Forest", "SV_Fairy_20_ForestMonolith_Revealed"), "Golden Obelisk Holy Cross" },
            { ("West Garden", "SV_Archipelagos Redux_Filigree_Door_Basic"), "Holy Cross Door" },
            { ("West Garden", "SV_Fairy_18_GardenCourtyard_Revealed"), "Holy Cross (Blue Lines)" },
            { ("West Garden", "SV_Fairy_17_GardenTree_Revealed"), "Tree Holy Cross Chest" },
            { ("Library Hall", "SV_Fairy_9_Library_Rug_Revealed"), "Holy Cross Chest" },
            { ("Eastern Vault Fortress", "SV_Fairy_19_FortressCandles_Revealed"), "Candles Holy Cross" },
            { ("Quarry", "SV_Fairy_7_Quarry_Revealed"), "Bushes Holy Cross" },
            { ("Lower Mountain", "SV_Mountain___Final Door Spell Listener"), "Top Of Mountain Door" },
            { ("Swamp", "SV_Cathedral Redux_secret filigree door"), "Secret Legend Door" },
            { ("Cathedral", "SV_Cathedral Redux_secret filigree door"), "Secret Legend Door" },
        };
        private static Dictionary<string, string> GlobalCodes = new Dictionary<string, string>
        {
            { "Granted Firecracker", "Firecracker" },
            { "Granted Firebomb", "Firebomb" },
            { "Granted Icebomb", "Icebomb" },
        };
        private static Dictionary<string, Dictionary<string, string>> ImportantItems = new Dictionary<string, Dictionary<string, string>>
        {
            {
                "Unlocks", new Dictionary<string, string>
                {
                    { "Hyperdash", "Hero's Laurels" },
                    { "Lantern", "Lantern" },
                    { "Vault Key (Red)", "Fortress Vault Key" },
                    { "Trinket Slot", "Trinket Slot" },
                    { "Hexagon Red", "Red Questagon" },
                    { "Hexagon Green", "Green Questagon" },
                    { "Hexagon Blue", "Blue Questagon" },
                    { "Hexagon Gold", "Gold Questagon" },
                    { "Relic - Hero Sword", "Hero Relic - ATT" },
                    { "Relic - Hero Crown", "Hero Relic - DEF" },
                    { "Relic - Hero Water", "Hero Relic - POTION" },
                    { "Relic - Hero Pendant HP", "Hero Relic - HP" },
                    { "Relic - Hero Pendant MP", "Hero Relic - MP" },
                    { "Relic - Hero Pendant SP", "Hero Relic - SP" },
                }
            },
            {
                "Equipment", new Dictionary<string, string>
                {
                    { "Stick", "Stick" },
                    { "Sword", "Sword" },
                    { "Librarian Sword", "Librarian Sword" },
                    { "Heir Sword", "Heir Sword" },
                    { "Shield", "Shield" },
                    { "Stundagger", "Magic Dagger" },
                    { "Techbow", "Magic Wand" },
                    { "Wand", "Magic Orb" },
                    { "Shotgun", "Gun" },
                    { "SlowmoItem", "Hourglass" },
                    { "Torch", "Torch" },
                    { "Dath Stone", "Dath Stone" },
                    { "Flask Container", "Flask" },
                }
            },
            {
                "Trinkets", new Dictionary<string, string>
                {
                    { "Trinket - Walk Speed Plus", "Anklet" },
                    { "Trinket - Stamina Recharge Plus", "Perfume" },
                    { "Trinket - Sneaky", "Muffling Bell" },
                    { "Trinket - RTSR", "Orange Peril Ring" },
                    { "Trinket - Parry Window", "Aura's Gem" },
                    { "Trinket - MP Flasks", "Inverted Ash" },
                    { "Trinket - IFrames", "Bone" },
                    { "Trinket - Heartdrops", "Lucky Cup" },
                    { "Trinket - Glass Cannon", "Glass Cannon" },
                    { "Trinket - Fast Icedagger", "Daggerstrap" },
                    { "Trinket - Bloodstain Plus", "Louder Echo" },
                    { "Trinket - Bloodstain MP", "Magic Echo" },
                    { "Trinket - Block Plus", "Bracer" },
                    { "Trinket - Attack Up Defense Down", "Tincture" },
                    { "Trinket - BTSR", "Cyan Peril Ring" },
                    { "Mask", "Mask" },
                }
            },
            {
                "GoldenTrophies", new Dictionary<string, string>
                {
                    { "GoldenTrophy_1", "Mr Mayor" },
                    { "GoldenTrophy_2", "A Secret Legend" },
                    { "GoldenTrophy_3", "Sacred Geometry" },
                    { "GoldenTrophy_4", "Vintage" },
                    { "GoldenTrophy_5", "Just Some Pals" },
                    { "GoldenTrophy_6", "Regal Weasel" },
                    { "GoldenTrophy_7", "Sprinng Falls" },
                    { "GoldenTrophy_8", "Power Up" },
                    { "GoldenTrophy_9", "Back to Work" },
                    { "GoldenTrophy_10", "Phonomath" },
                    { "GoldenTrophy_11", "Dusty" },
                    { "GoldenTrophy_12", "Forever Friend" },
                }
            },
            {
                "Stats", new Dictionary<string, string>
                {
                    { "Level Up - Attack", "Attack" },
                    { "Level Up - DamageResist", "Defense" },
                    { "Level Up - PotionEfficiency", "Potion" },
                    { "Level Up - Health", "Health" },
                    { "Level Up - Magic", "Magic" },
                    { "Level Up - Stamina", "Stamina" },
                }
            },
            {
                "Ladders", new Dictionary<string, string>
                {
                    { "Ladders in Overworld Town", "Overworld Town" },
                    { "Ladders near Weathervane", "Near Weathervane" },
                    { "Ladders near Overworld Checkpoint", "By Overworld Checkpoint" },
                    { "Ladder to East Forest", "To East Forest" },
                    { "Ladders to Lower Forest", "To Lower East Forest" },
                    { "Ladders near Patrol Cave", "Near Patrol Cave" },
                    { "Ladders in Well", "Ladders in Well" },
                    { "Ladders to West Bell", "To West Bell" },
                    { "Ladder to Quarry", "To Quarry" },
                    { "Ladder in Dark Tomb", "In Dark Tomb" },
                    { "Ladders near Dark Tomb", "Near Dark Tomb" },
                    { "Ladder near Temple Rafters", "Near Temple Rafters" },
                    { "Ladder to Swamp", "Ladder to Swamp" },
                    { "Ladders in Swamp", "Ladders in Swamp" },
                    { "Ladder to Ruined Atoll", "To Ruined Atoll" },
                    { "Ladders in South Atoll", "South Atoll" },
                    { "Ladders to Frog's Domain", "Frog's Domain" },
                    { "Ladders in Hourglass Cave", "Hourglass Cave Tower" },
                    { "Ladder to Beneath the Vault", "Beneath the Vault" },
                    { "Ladders in Lower Quarry", "Lower Quarry" },
                    { "Ladders in Library", "Library Ladders" },
                }
            }
        };

        private class RemoteHint
        {
            public string FindingPlayer;
            public string ReceivingPlayer;
            public string Item;
            public string Location;
            public string Entrance;
            public bool Found;

            public RemoteHint(string findingPlayer, string receivingPlayer, string item, string location, string entrance, bool found)
            {
                FindingPlayer = findingPlayer;
                ReceivingPlayer = receivingPlayer;
                Item = item;
                Location = location;
                Entrance = entrance;
                Found = found;
            }
        }

        private class AllHints
        {
            public Dictionary<string, string> LocalHints;
            public List<RemoteHint> RemoteHints;

            public AllHints()
            {
                LocalHints = new Dictionary<string, string>();
                RemoteHints = new List<RemoteHint>();
            }
        }

        private class ErrorResponse
        {
            public string error;

            public ErrorResponse(string error)
            {
                this.error = error;
            }
        }

        private class Current
        {
            public string scene;
            public int seed;
            public int items;
            public int entrances;
            public int localHints;
            public int remoteHints;
            public Dictionary<string, Code> codes;
            public Dictionary<string, Dictionary<string, int>> inventory;

            public Current()
            {
                codes = new Dictionary<string, Code>();
                inventory = new Dictionary<string, Dictionary<string, int>>();
            }

            public void AddCode(string name, float distance, bool global, bool inRange)
            {
                codes[name] = new Code(distance, global, inRange);
            }
        }
        private class Code
        {
            public float Distance;
            public bool Global;
            public bool InRange;

            public Code(float distance, bool global, bool inRange)
            {
                Distance = distance;
                Global = global;
                InRange = inRange;
            }
        }
        private class Item
        {
            public string name;
            public string owner;
            public bool collected;

            public Item(string name, string owner, bool collected)
            {
                this.name = name;
                this.owner = owner;
                this.collected = collected;
            }
        }
        private class Door
        {
            public string scene;
            public string door;

            public Door(string scene, string door)
            {
                this.scene = scene;
                this.door = door;
            }
        }
        private class ItemScene
        {
            public int found;
            public int remaining;
            public int collected;
            public int total;
            public SortedDictionary<string, Item> checks;

            public ItemScene()
            {
                found = 0;
                collected = 0;
                remaining = 0;
                total = 0;
                checks = new SortedDictionary<string, Item>();
            }

            public void AddItem(string description, string name, string owner, bool wasCollected)
            {
                total++;
                if (name == "") remaining++;
                else if (wasCollected) collected++;
                else found++;
                checks[description] = new Item(name, owner, wasCollected);
            }
        }
        private class ItemScenes
        {
            public int found;
            public int collected;
            public int remaining;
            public int total;
            public SortedDictionary<string, ItemScene> scenes;

            public ItemScenes()
            {
                found = 0;
                collected = 0;
                remaining = 0;
                total = 0;
                scenes = new SortedDictionary<string, ItemScene>();
            }

            public void AddScene(string scene)
            {
                scenes[scene] = new ItemScene();
            }

            public void AddItem(string scene, string description, string name, string owner, bool wasCollected)
            {
                total++;
                if (name == "") remaining++;
                else if (wasCollected) collected++;
                else found++;
                scenes[scene].AddItem(description, name, owner, wasCollected);
            }
        }
        private class DoorScene
        {
            public int found;
            public int remaining;
            public int total;
            public SortedDictionary<string, Door> doors;

            public DoorScene()
            {
                found = 0;
                remaining = 0;
                total = 0;
                doors = new SortedDictionary<string, Door>();
            }

            public void AddDoor(string entrance, string exit, string exitRegion)
            {
                total++;
                if (exit == "") remaining++;
                else found++;
                doors[entrance] = new Door(exitRegion, exit);
            }
        }
        private class DoorScenes
        {
            public int found;
            public int remaining;
            public int total;
            public SortedDictionary<string, DoorScene> scenes;

            public DoorScenes()
            {
                found = 0;
                remaining = 0;
                total = 0;
                scenes = new SortedDictionary<string, DoorScene>();
            }

            public void AddScene(string scene)
            {
                scenes[scene] = new DoorScene();
            }

            public void AddDoor(string scene, string entrance, string exit, string exitRegion)
            {
                total++;
                if (exit == "") remaining++;
                else found++;
                scenes[scene].AddDoor(entrance, exit, exitRegion);
            }
        }
        public static Server instance { get; set; }

        public enum ServerState
        {
            NOT_LISTENING,
            WAITING_FOR_REQUEST,
            PENDING_PROCESSING,
            RESPONSE_READY,
        };

        private HttpListener Listener;
        public ServerState State = ServerState.NOT_LISTENING;
        private string Type;
        private string Payload;

        public bool Running { get; private set; }
        public string Port { get; private set; }

        public void Start()
        {
            if (TunicRandomizer.Settings.ServerSettings.Autoconnect) instance.Connect();
        }

        public void Update()
        {
            // short circuit if there's nothing waiting for us to handle
            if (State != ServerState.PENDING_PROCESSING) return;

            // to replicate current functionality, just don't do anything if there's no player
            PlayerCharacter __instance = PlayerCharacter.instance;
            if (__instance == null)
            {
                Payload = JsonConvert.SerializeObject(new ErrorResponse("There is no fox."));
                State = ServerState.RESPONSE_READY;
                return;
            }

            try
            {
                if (Type == "ITEMS")
                {
                    ItemScenes output = new ItemScenes();

                    foreach (string scene in Locations.SimplifiedSceneNames.Values)
                    {
                        output.AddScene(scene);
                    }

                    if (IsArchipelago())
                    {
                        foreach (KeyValuePair<string, ArchipelagoItem> check in ItemLookup.ItemList)
                        {
                            var slices = Locations.LocationIdToDescription[check.Key].Split(new[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
                            string scene = slices[0];
                            string description = slices[1];
                            string name = "";
                            string owner = "";
                            bool wasCollected = false;

                            if (Locations.CheckedLocations[check.Key])
                            {
                                ArchipelagoItem curItem = ItemLookup.ItemList[check.Key];
                                name = curItem.ItemName;
                                owner = Archipelago.instance.GetPlayerName(curItem.Player);
                            }

                            // if archipelago reflect collected is on, check if the check was collected
                            else if (TunicRandomizer.Settings.CollectReflectsInWorld && SaveFile.GetInt($"randomizer {check.Key} was collected") == 1)
                            {
                                ArchipelagoItem curItem = ItemLookup.ItemList[check.Key];
                                name = curItem.ItemName;
                                owner = Archipelago.instance.GetPlayerName(curItem.Player);
                                wasCollected = true;
                            }

                            // homologate discrepencies between the rando's internal names and archipelago's internal names
                            if (scene == "Southeast Cross Door") scene = "Southeast Cross Room";
                            else if (scene == "Fountain Cross Door") scene = "Fountain Cross Room";

                            output.AddItem(scene, description, name, owner, wasCollected);
                        }
                    }
                    else
                    {
                        foreach (KeyValuePair<string, Check> check in Locations.RandomizedLocations)
                        {
                            string scene = Locations.SimplifiedSceneNames[check.Value.Location.SceneName];
                            string description = Locations.LocationIdToDescription[check.Key].Split('-')[1].Trim(' ');
                            string name = "";

                            if (Locations.CheckedLocations[check.Key])
                            {
                                name = ItemLookup.GetItemDataFromCheck(check.Value).Name;
                            }

                            output.AddItem(scene, description, name, "", false);
                        }
                    }

                    Payload = JsonConvert.SerializeObject(output);
                }
                else if (Type == "DOORS")
                {
                    DoorScenes output = new DoorScenes();

                    foreach (string scene in Locations.SimplifiedSceneNames.Values)
                    {
                        output.AddScene(scene);
                    }

                    HashSet<string> visited = new HashSet<string>(SaveFile.GetString("RandoVisitedDoors").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                    int shop = 1;
                    foreach (KeyValuePair<string, PortalCombo> pc in TunicPortals.RandomizedPortals)
                    {
                        bool mapped = visited.Contains(pc.Key);
                        string scene1 = Locations.SimplifiedSceneNames[pc.Value.Portal1.Scene];
                        string scene2 = Locations.SimplifiedSceneNames[pc.Value.Portal2.Scene];
                        string name1 = pc.Value.Portal1.Name;
                        string name2 = pc.Value.Portal2.Name;

                        if (name1 == "Shop Portal" || name1 == "Shop")
                        {
                            name1 = $"Shop Portal {shop}";
                            shop++;
                        }

                        if (name2 == "Shop Portal" || name2 == "Shop")
                        {
                            name2 = $"Shop Portal {shop}";
                            shop++;
                        }

                        if (mapped)
                        {
                            output.AddDoor(scene1, name1, name2, scene2);
                            output.AddDoor(scene2, name2, name1, scene1);
                        }
                        else
                        {
                            output.AddDoor(scene1, name1, "", "");
                            output.AddDoor(scene2, name2, "", "");
                        }
                    }

                    Payload = JsonConvert.SerializeObject(output);
                }
                else if (Type == "OVERVIEW")
                {
                    Current output = new Current();

                    foreach (var category in ImportantItems)
                    {
                        output.inventory[category.Key] = new Dictionary<string, int>();
                        foreach (var item in category.Value)
                        {
                            output.inventory[category.Key][item.Value] = Inventory.GetItemByName(item.Key).Quantity;
                        }
                    }
                    if (output.inventory.ContainsKey("Stats")) {
                        foreach (string stat in ImportantItems["Stats"].Values)
                        {
                            output.inventory["Stats"][stat]++;
                        }
                    }
                    if (output.inventory.ContainsKey("Unlocks") && output.inventory["Unlocks"].ContainsKey("Trinket Slot"))
                    {
                        output.inventory["Unlocks"]["Trinket Slot"]++;
                    }

                    string currentScene = SceneManager.GetActiveScene().name;
                    if (Locations.SimplifiedSceneNames.ContainsKey(currentScene))
                    {
                        currentScene = Locations.SimplifiedSceneNames[currentScene];
                    }

                    Dictionary<string, ToggleObjectBySpell> codes = new Dictionary<string, ToggleObjectBySpell>();
                    foreach (var code in Resources
                        .FindObjectsOfTypeAll<ToggleObjectBySpell>()
                        .Where(x => x.isActiveAndEnabled && x.stateVar != null && !x.stateVar.BoolValue))
                    {
                        if (!codes.ContainsKey(code.stateVar.name)) codes.Add(code.stateVar.name, code);
                    }

                    foreach (var code in codes)
                    {
                        if (Codes.ContainsKey((currentScene, code.Key)))
                        {
                            float distance = Vector3.Distance(code.Value.transform.position, __instance.transform.position);
                            output.AddCode(Codes[(currentScene, code.Key)], distance, false, code.Value.distanceOK(__instance.transform.position));
                        }
                    }

                    foreach (var code in GlobalCodes)
                    {
                        if (SaveFile.GetInt(code.Key) == 0)
                            output.AddCode(code.Value, 0f, true, true);
                    }

                    char[] separators = { ',' };
                    output.seed = SaveFile.GetInt("seed");
                    output.scene = currentScene;
                    // if auto collect is on, also include any collected checks in this total
                    output.items = Locations.VanillaLocations.Keys.Where(loc => Locations.CheckedLocations[loc] || (SaveFlags.IsArchipelago() && TunicRandomizer.Settings.CollectReflectsInWorld && SaveFile.GetInt($"randomizer {loc} was collected") == 1)).ToList().Count;
                    output.entrances = SaveFile.GetString("RandoVisitedDoors").Split(separators, StringSplitOptions.RemoveEmptyEntries).Length;
                    output.localHints = SaveFile.GetString("RandoVisitedFoxes").Split(separators, StringSplitOptions.RemoveEmptyEntries).Length +
                        SaveFile.GetString("RandoVisitedHints").Split(separators, StringSplitOptions.RemoveEmptyEntries).Length +
                        SaveFile.GetString("RandoVisitedGraves").Split(separators, StringSplitOptions.RemoveEmptyEntries).Length;
                    if (IsArchipelago())
                    {
                        output.remoteHints = Archipelago.instance.integration.session.DataStorage.GetHints().Length;
                    }

                    Payload = JsonConvert.SerializeObject(output);
                }
                else if (Type == "HINTS")
                {
                    AllHints payload = new AllHints();

                    char[] separators = { ',' };
                    string[] foxes = SaveFile.GetString("RandoVisitedFoxes").Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    string[] hints = SaveFile.GetString("RandoVisitedHints").Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    string[] graves = SaveFile.GetString("RandoVisitedGraves").Split(separators, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string fox in foxes)
                    {
                        payload.LocalHints[fox] = Regex.Replace(GhostHints.HintGhosts[fox].Hint, @"(""[\n\r]""| ?[\n\r])", " ");
                    }
                    foreach (string hint in hints)
                    {
                        payload.LocalHints[hint] = Regex.Replace(Hints.HintMessages[hint], @"(""[\n\r]""| ?[\n\r])", " ");
                    }
                    foreach (string grave in graves)
                    {
                        string[] slices = grave.Split(':');
                        if (slices[1] == "R")
                            payload.LocalHints[slices[0] + " - Relic"] = Regex.Replace(Hints.HeroGraveHints[slices[0]].RelicHint, @"(""[\n\r]""| ?[\n\r])", " ");
                        else
                            payload.LocalHints[slices[0] + " - Path"] = Regex.Replace(Hints.HeroGraveHints[slices[0]].PathHint, @"(""[\n\r]""| ?[\n\r])", " ");
                    }

                    if (IsArchipelago())
                    {
                        foreach (var hint in Archipelago.instance.integration.session.DataStorage.GetHints())
                        {
                            payload.RemoteHints.Add(new RemoteHint(
                                Archipelago.instance.GetPlayerName(hint.FindingPlayer),
                                Archipelago.instance.GetPlayerName(hint.ReceivingPlayer),
                                Archipelago.instance.GetItemName(hint.ItemId),
                                Archipelago.instance.GetLocationName(hint.LocationId),
                                hint.Entrance,
                                hint.Found
                            ));
                        }
                    }

                    Payload = JsonConvert.SerializeObject(payload);
                }
                else if (Type == "STATS")
                {
                }

                State = ServerState.RESPONSE_READY;
            }
            catch (Exception e)
            {
                TunicRandomizer.Logger.LogInfo("Hit exception in handler: " + e.Message);
                TunicRandomizer.Logger.LogError(e.StackTrace);
            }

            //TunicRandomizer.Logger.LogInfo("Done handling");
        }

        public void OnDestroy()
        {
            instance.Disconnect();
        }

        public void Connect()
        {
            Port = TunicRandomizer.Settings.ServerSettings.Port;
            Listener = new HttpListener();
            Listener.Prefixes.Add($"http://*:{Port}/");
            Listener.Start();
            Running = true;
            TunicRandomizer.Logger.LogInfo("API server \"" + VERSION + "\" started on port " + Port);

            State = ServerState.WAITING_FOR_REQUEST;
            Receive();
        }

        public void Disconnect()
        {
            Listener.Stop();
            Running = false;
            TunicRandomizer.Logger.LogInfo("API server halted");
        }

        private void Receive()
        {
            // TunicRandomizer.Logger.LogInfo("Starting Receive");
            Listener.BeginGetContext(new AsyncCallback(ListenerCallback), Listener);
        }

        private void ListenerCallback(IAsyncResult result)
        {
            // TunicRandomizer.Logger.LogInfo("Received callback");
            if (!Listener.IsListening) return;

            var context = Listener.EndGetContext(result);
            var request = context.Request;
            var response = context.Response;

            // TunicRandomizer.Logger.LogInfo("  Request being processed: " + request.HttpMethod + " " + request.RawUrl);

            response.AppendHeader("Access-Control-Allow-Origin", "*");
            if (request.HttpMethod == "OPTIONS")
            {
                response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept, X-Requested-With");
                response.AddHeader("Access-Control-Allow-Methods", "GET, POST");
                response.AddHeader("Access-Control-Max-Age", "1728000");
                State = ServerState.RESPONSE_READY;
            }

            if (request.RawUrl == "/ruok")
            {
                Payload = "i am ok";
                State = ServerState.RESPONSE_READY;
            }
            else if (request.RawUrl == "/items" && request.HttpMethod == "GET")
            {
                Type = "ITEMS";
                State = ServerState.PENDING_PROCESSING;
            }
            else if (request.RawUrl == "/doors")
            {
                Type = "DOORS";
                State = ServerState.PENDING_PROCESSING;
            }
            else if (request.RawUrl == "/overview" && request.HttpMethod == "GET")
            {
                Type = "OVERVIEW";
                State = ServerState.PENDING_PROCESSING;
            }
            else if (request.RawUrl == "/hints" && request.HttpMethod == "GET")
            {
                Type = "HINTS";
                State = ServerState.PENDING_PROCESSING;
            }
            else if (request.RawUrl == "/bogus")
            {
                State = ServerState.PENDING_PROCESSING;
            }

            // don't wait if our request didn't match anything
            if (State == ServerState.WAITING_FOR_REQUEST)
            {
                Payload = JsonConvert.SerializeObject(new ErrorResponse("Unrecognized path: " + request.RawUrl));
                State = ServerState.RESPONSE_READY;
            }

            // Timeouts
            DateTime start = DateTime.UtcNow;
            DateTime timeout = start.AddMilliseconds(TIMEOUT);
            while (State != ServerState.RESPONSE_READY) {
                if (DateTime.UtcNow.CompareTo(timeout) > 0)
                {
                    TunicRandomizer.Logger.LogInfo("API Server hit " + TIMEOUT + "ms timeout");
                    Payload = JsonConvert.SerializeObject(new ErrorResponse("Hit 1000ms timeout waiting for handler response."));
                    break;
                }
            }
            TimeSpan elapsed = DateTime.UtcNow.Subtract(start);
            // TunicRandomizer.Logger.LogInfo("  SERVER-02: " + request.RawUrl + " handled in " + elapsed.TotalMilliseconds + "ms");

            byte[] output = Encoding.ASCII.GetBytes(Payload);
            response.StatusCode = (int)HttpStatusCode.OK;
            response.ContentType = "application/json";
            response.OutputStream.Write(output, 0, output.Length);
            response.OutputStream.Close();

            State = ServerState.WAITING_FOR_REQUEST;
            // TunicRandomizer.Logger.LogInfo("  Done with processing");
            Receive();
        }
    }
}
