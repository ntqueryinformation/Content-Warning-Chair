using Photon.Pun;
using System;
using System.Collections;
using System.Reflection;
using System.Threading;
using UnityEngine;
using Zorro.Core;
using Zorro.Core.CLI;

namespace ExampleAssembly
{
    public class Cheat : MonoBehaviour
    {
        private PhotonView photonView;
        private int mainWID = 1024;
        private Rect mainWRect = new Rect(5f, 5f, 300f, 150f);
        private bool godmode;
        private bool infBattery;
        private bool drawMenu = true;
        private bool stamina = false;
        private bool oxygen = false;
        private bool superJump;
        private bool rainbowVisor;
        private bool blinkingFace;
        private bool greenScreenSpam;

        private float lastCacheTime = Time.time + 5f;
        private float lastItemCache = Time.time + 1f;

        public static Player[] players;
        public static BombItem[] bombs;
        public static PlayerController controller;
        public static Monster[] monsters;
        public static ItemInstance[] droppedItems;
        private int selectedItemIndex = -1;
        private Vector2 scrollPosition;

        private string[] enemyNames = new string[] { "Ear", "Zombe", "Spider", "Snatcho", "AnglerMimic", "EyeGuy", "Toolkit_Wisk", "Bombs", "Knifo", "Angler", "BigSlap", "Ghost", "BarnacleBall", "Jello", "Weeping", "MimicInfiltrator", "Flicker" }; // Add the rest of your enemy names here
        private int selectedEnemyIndex = -1;
        private bool isEnemyDropdownVisible = false;
        private Vector2 enemyScrollPosition;
        private string enemyButtonText = "Select Enemy";
        // public static Car[] vehicles;

        public void Keyhandler()
        {
            if (!Input.anyKey || !Input.anyKeyDown)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Insert))
            {
                drawMenu = !drawMenu;
            }
            if (Input.GetKeyDown(KeyCode.Space) && Player.localPlayer != null && superJump)
            {
                Player.localPlayer.refs.view.RPC("RPCA_Jump", RpcTarget.All, Array.Empty<object>());
            }
            //if (Input.GetKeyDown(KeyCode.Mouse0) && magicBullet) {
            //    if (players.Length > 0) {
            //        foreach (Player player in players) {
            //            if (player != Player.localPlayer && player != null) {
            //                foreach (ProjectileHit proj in FindObjectsOfType<ProjectileHit>()) {
            //                    player.m_playerDeath.TakeDamage(proj.transform.position, new Vector3());
            //                }
            //            }
            //        }
            //    }
            //}
        }

        //public void Supergun(ref Gun gun) {
        //    gun.bullets = 65535;
        //    gun.bulletsInMag = 65535;
        //    gun.extraSpread = 0f;
        //    gun.hasFullAuto = true;
        //    gun.hipSpreadValue = 0f;
        //    gun.projectileRecoilMultiplier = 0f;
        //    gun.rateOfFire = 0.025f;
        //    gun.currentFireMode = 2; // Full auto.

        //    Destroy(gun.GetComponent<Recoil>());
        //}
        private float nextBlinkTime = 0;

        private float blinkInterval = 5;
        private string currentFace = "0_0";
        private string normalFace = "0_0";
        private string blinkFace = "-_-";
        private string winkFace = "0_-";
        private bool isDropdownVisible = false;
        private bool isDropdownVisible2 = false;
        private string buttonText = "Select Item";

        private IEnumerator ShowExpressionBriefly(float duration)
        {
            Player.localPlayer.refs.visor.RPCA_SetVisorText(currentFace);
            PlayerPrefs.SetString("FaceText", currentFace);
            yield return new WaitForSeconds(duration);
            currentFace = normalFace;
            Player.localPlayer.refs.visor.RPCA_SetVisorText(currentFace);
            PlayerPrefs.SetString("FaceText", currentFace);
        }

        private void EquipItem(Item item)
        {
            Vector3 debugItemSpawnPos = MainCamera.instance.GetDebugItemSpawnPos();
            Player.localPlayer.RequestCreatePickup(item, new ItemInstanceData(Guid.NewGuid()), debugItemSpawnPos, Quaternion.identity);
        }

        public void Update()
        {
            Keyhandler();

            if (Player.localPlayer != null)
            {
                if (blinkingFace)
                {
                    if (Time.time >= nextBlinkTime)
                    {
                        // Decide whether to blink or wink
                        if (UnityEngine.Random.value > 0.5f)
                        {
                            currentFace = blinkFace;
                        }
                        else
                        {
                            currentFace = winkFace;
                        }

                        // Show the blinking or winking face for a brief moment
                        StartCoroutine(ShowExpressionBriefly(0.2f));

                        // Set the time for the next blink or wink
                        nextBlinkTime = Time.time + UnityEngine.Random.Range(2, 5);
                    }
                }
                if (rainbowVisor)
                {
                    float time = Time.time;

                    // Use Mathf.Sin to get a value between -1 and 1, then scale it to 0 to 1 for colors
                    float red = (Mathf.Sin(time) + 1) / 2;
                    float green = (Mathf.Sin(time + 2 * Mathf.PI / 3) + 1) / 2; // Offset by 2/3 π to desynchronize
                    float blue = (Mathf.Sin(time + 4 * Mathf.PI / 3) + 1) / 2; // Offset by 4/3 π to desynchronize

                    // Create a new color with the calculated RGB values
                    Color newColor = new Color(red, green, blue);
                    Player.localPlayer.refs.visor.ApplyVisorColor(newColor);
                }
                if (stamina)
                {
                    Player.localPlayer.data.currentStamina = 100;
                    Player.localPlayer.data.staminaDepleated = false;
                }
                if (oxygen)
                {
                    {
                        Player.localPlayer.data.remainingOxygen = 500;
                    }
                }
                if (godmode)
                {
                    //if (Player.localPlayer.data.dead)
                    //{
                    //    Player.localPlayer.CallRevive();
                    //}
                    Player.localPlayer.data.health = 100f;
                }
                if (infBattery)
                {
                    PlayerInventory playerInventory;
                    Player.localPlayer.TryGetInventory(out playerInventory);
                    if (playerInventory != null)
                    {
                        foreach (InventorySlot inventorySlot in playerInventory.slots)
                        {
                            BatteryEntry batteryEntry;
                            if (inventorySlot.ItemInSlot.item != null && inventorySlot.ItemInSlot.data.TryGetEntry<BatteryEntry>(out batteryEntry) && batteryEntry.m_maxCharge > batteryEntry.m_charge)
                            {
                                batteryEntry.AddCharge(1000);
                            }
                        }
                    }
                }
                if (greenScreenSpam)
                {
                    foreach (ProjectorMachine machine in FindObjectsOfType<ProjectorMachine>())
                    {
                        machine.PressMore();
                    }
                }
            }

            if (Time.time >= lastCacheTime)
            {
                lastCacheTime = Time.time + 3f;

                players = FindObjectsOfType<Player>();

                //controller = base.GetComponent<PlayerController>();
                // vehicles = FindObjectsOfType<Car>();

                ESP.mainCam = Camera.main;
            }

            if (Time.time >= lastItemCache)
            {
                lastItemCache = Time.time + 1f;

                droppedItems = FindObjectsOfType<ItemInstance>();
                bombs = FindObjectsOfType<BombItem>();
            }
        }

        public void OnGUI()
        {
            if (drawMenu)
            {
                mainWRect = GUILayout.Window(mainWID, mainWRect, MainWindow, "Main");
            }
        }

        private int tabSelected = 0;

        private void GooTroll(ItemGooBall gooItem, Player player, bool gooMonsters)
        {
            if (player != null && player != Player.localPlayer)
            {
                if (gooMonsters && !player.ai)
                    return;

                if (!gooMonsters && player.ai)
                    return;

                PhotonNetwork.Instantiate(gooItem.explodedGoopPref.name, player.HeadPosition(), Quaternion.identity, 0, null);
            }
        }

        private void WebTroll(Player player, bool gooMonsters)
        {
            if (player != null && player != Player.localPlayer)
            {
                if (gooMonsters && !player.ai)
                    return;

                if (!gooMonsters && player.ai)
                    return;
                PhotonNetwork.Instantiate("Web", player.HeadPosition(), Quaternion.identity, 0, null);
            }
        }

        private void MainWindow(int id)
        {
            GUILayout.BeginHorizontal();//Tab Selector
            {
                if (GUILayout.Button("Self Tab"))
                {
                    tabSelected = 0;
                }
                if (GUILayout.Button("Lobby Tab"))
                {
                    tabSelected = 1;
                }
                if (GUILayout.Button("Troll Tab"))
                {
                    tabSelected = 2;
                }
                if (GUILayout.Button("Spawn Tab"))
                {
                    tabSelected = 3;
                }
            }
            GUILayout.EndHorizontal();

            if (tabSelected == 0) //Self Tab
            {
                GUILayout.BeginVertical("ESP", GUI.skin.box);
                {
                    GUILayout.Space(20f);

                    GUILayout.BeginHorizontal();
                    {
                        ESP.crosshair = GUILayout.Toggle(ESP.crosshair, "Crosshair");
                        ESP.item = GUILayout.Toggle(ESP.item, "Items");
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        ESP.playerBox = GUILayout.Toggle(ESP.playerBox, "Player Box");
                        if (GUILayout.Button("Chams"))
                        {
                            ESP.DoChams();
                        }
                    }
                    GUILayout.EndHorizontal();
                    ESP.monsterBox = GUILayout.Toggle(ESP.monsterBox, "Monster Boxes");
                    ESP.playerName = GUILayout.Toggle(ESP.playerName, "Names");
                    //  ESP.playerName = GUILayout.Toggle(ESP.playerName, "Player Name");
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical("Self Toggles", GUI.skin.box);
                {
                    GUILayout.Space(20f);
                    GUILayout.BeginHorizontal();
                    {
                        stamina = GUILayout.Toggle(stamina, "Inf. Stamina");
                        oxygen = GUILayout.Toggle(oxygen, "Inf Oxygen");
                        godmode = GUILayout.Toggle(godmode, "God Mode");
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    {
                        superJump = GUILayout.Toggle(superJump, "Inf. Jump");
                        rainbowVisor = GUILayout.Toggle(rainbowVisor, "Rainbow Face");
                        blinkingFace = GUILayout.Toggle(blinkingFace, "Blinking Face");
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        infBattery = GUILayout.Toggle(infBattery, "Inf. Battery");
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Revive Self"))
                    {
                        Player.localPlayer.CallRevive();
                        Player.localPlayer.data.health = 100f;
                    }
                    if (GUILayout.Button("Ragdoll Self"))
                    {
                        Player.localPlayer.RPCA_TakeDamageAndAddForce(0f, new Vector3(UnityEngine.Random.Range(-10f, 10f), UnityEngine.Random.Range(-10f, 10f), UnityEngine.Random.Range(1f, 10f)), 3f);
                    }
                    if (GUILayout.Button("Spawn Player"))
                    {
                        Monster.SpawnMonster("Player");
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Open Console"))
                    {
                        foreach (DebugUIHandler item in FindObjectsOfType<DebugUIHandler>())
                        {
                            item.Show();
                        }
                    }

                    if (GUILayout.Button("Close Console"))
                    {
                        foreach (DebugUIHandler item in FindObjectsOfType<DebugUIHandler>())
                        {
                            item.Hide();
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }//Self Tab
            if (tabSelected == 1)
            {
                GUILayout.BeginVertical("Host Only", GUI.skin.box);
                {
                    GUILayout.Space(20f);

                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("Add $10K"))
                        {
                            SurfaceNetworkHandler.RoomStats.AddMoney(10000);
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical("Offhost", GUI.skin.box);
                {
                    GUILayout.Space(20f);
                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("Revive All Players"))
                        {
                            if (Cheat.players.Length > 0)
                            {
                                foreach (Player player in Cheat.players)
                                {
                                    if (player != null)
                                    {
                                        player.CallRevive();
                                        player.data.health = 100f;
                                    }
                                }
                            }
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }//Lobby Tab
            if (tabSelected == 2)
            {
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Goo All Players"))
                    {
                        var items = SingletonAsset<ItemDatabase>.Instance.lastLoadedItems;
                        for (int i = 0; i < items.Count; i++)
                        {
                            if (items[i].name == "GooBall")
                            {
                                Item GooItem = items[i];
                                ItemGooBall test = GooItem.itemObject.GetComponent<ItemGooBall>();

                                if (Cheat.players.Length > 0)
                                {
                                    foreach (Player player in Cheat.players)
                                    {
                                        GooTroll(test, player, false);
                                    }
                                }
                                break;
                            }
                        }
                    }
                    if (GUILayout.Button("Goo All Monsters"))
                    {
                        var items = SingletonAsset<ItemDatabase>.Instance.lastLoadedItems;
                        for (int i = 0; i < items.Count; i++)
                        {
                            if (items[i].name == "GooBall")
                            {
                                Item GooItem = items[i];
                                ItemGooBall test = GooItem.itemObject.GetComponent<ItemGooBall>();

                                if (Cheat.players.Length > 0)
                                {
                                    foreach (Player player in Cheat.players)
                                    {
                                        GooTroll(test, player, true);
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Web All Players"))
                    {
                        if (Cheat.players.Length > 0)
                        {
                            foreach (Player player in Cheat.players)
                            {
                                WebTroll(player, false);
                            }
                        }
                    }
                    if (GUILayout.Button("Web All Monsters"))
                    {
                        if (Cheat.players.Length > 0)
                        {
                            foreach (Player player in Cheat.players)
                            {
                                WebTroll(player, true);
                            }
                        }
                    }
                }
                GUILayout.EndHorizontal();
                if (GUILayout.Button("Spawn Drone"))
                {
                    foreach (ShopHandler shop in FindObjectsOfType<ShopHandler>())
                    {
                        var fieldInfo = typeof(ShopHandler).GetField("m_PhotonView", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (fieldInfo != null)
                        {
                            PhotonView m_PhotonView = (PhotonView)fieldInfo.GetValue(shop);
                            byte[] itemIDs = new byte[] { 0x1 };
                            m_PhotonView.RPC("RPCA_SpawnDrone", RpcTarget.All, new object[]
            {
                itemIDs
            });
                        }
                    }
                }
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Ragdoll Players"))
                    {
                        if (Cheat.players.Length > 0)
                        {
                            foreach (Player player in Cheat.players)
                            {
                                if (player == Player.localPlayer)
                                    continue;
                                MethodInfo methodInfo = typeof(Player).GetMethod("CallTakeDamageAndAddForceAndFall", BindingFlags.NonPublic | BindingFlags.Instance);
                                if (methodInfo != null)
                                {
                                    methodInfo.Invoke(player, new object[] { 0f, new Vector3(UnityEngine.Random.Range(-10f, 10f), UnityEngine.Random.Range(-10f, 10f), 5f), 2.5f });
                                }
                            }
                        }
                    }
                    if (GUILayout.Button("Kill Players"))
                    {
                        if (Cheat.players.Length > 0)
                        {
                            foreach (Player player in Cheat.players)
                            {
                                if (player == Player.localPlayer)
                                    continue;
                                MethodInfo methodInfo = typeof(Player).GetMethod("CallTakeDamageAndAddForceAndFall", BindingFlags.NonPublic | BindingFlags.Instance);
                                if (methodInfo != null)
                                {
                                    methodInfo.Invoke(player, new object[] { 9999999f, new Vector3(UnityEngine.Random.Range(-15f, 15f), UnityEngine.Random.Range(-15f, 15f), 5f), 500f });
                                }
                            }
                        }
                    }
                }
                GUILayout.EndHorizontal();
                greenScreenSpam = GUILayout.Toggle(greenScreenSpam, "Projector Spam");
            }
            if (tabSelected == 3)
            {
                GUILayout.BeginVertical("Item Spawner", GUI.skin.box);
                {
                    GUILayout.Space(20f);
                    if (GUILayout.Button(buttonText/*, GUILayout.Width(200), GUILayout.Height(40)*/))
                    {
                        isDropdownVisible = !isDropdownVisible;
                    }

                    // This block only appears after clicking the button, acting as the dropdown.
                    if (isDropdownVisible)
                    {
                        // Set a proper height for the dropdown area
                        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(270), GUILayout.Height(200));

                        for (int i = 0; i < SingletonAsset<ItemDatabase>.Instance.lastLoadedItems.Count; i++)
                        {
                            if (GUILayout.Button(SingletonAsset<ItemDatabase>.Instance.lastLoadedItems[i].name /*GUILayout.Width(190), GUILayout.Height(30)*/))
                            {
                                this.selectedItemIndex = i;
                                buttonText = SingletonAsset<ItemDatabase>.Instance.lastLoadedItems[i].name; // Update the button text
                                isDropdownVisible = false;
                            }
                        }

                        GUILayout.EndScrollView();
                    }

                    if (this.selectedItemIndex != -1 && GUILayout.Button("Give Item"/*, GUILayout.Width(200), GUILayout.Height(40)*/))
                    {
                        EquipItem(SingletonAsset<ItemDatabase>.Instance.lastLoadedItems[this.selectedItemIndex]);
                        //this.selectedItemIndex = -1;
                    }
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical("Enemy Spawner", GUI.skin.box);
                {
                    GUILayout.Space(20f);

                    if (GUILayout.Button(enemyButtonText/*, GUILayout.Width(200), GUILayout.Height(40)*/))
                    {
                        isEnemyDropdownVisible = !isEnemyDropdownVisible;
                    }

                    // This block only appears after clicking the button, acting as the dropdown.
                    if (isEnemyDropdownVisible)
                    {
                        // Set a proper height for the dropdown area
                        enemyScrollPosition = GUILayout.BeginScrollView(enemyScrollPosition, GUILayout.Width(270), GUILayout.Height(200));

                        for (int i = 0; i < enemyNames.Length; i++)
                        {
                            if (GUILayout.Button(enemyNames[i] /*GUILayout.Width(190), GUILayout.Height(30)*/))
                            {
                                selectedEnemyIndex = i;
                                enemyButtonText = enemyNames[i]; // Update the button text
                                isEnemyDropdownVisible = false;
                            }
                        }

                        GUILayout.EndScrollView();
                    }

                    if (selectedEnemyIndex != -1 && GUILayout.Button("Spawn Enemy"/*, GUILayout.Width(200), GUILayout.Height(40)*/))
                    {
                        Monster.SpawnMonster(enemyNames[selectedEnemyIndex]);
                        // selectedEnemyIndex = -1; // Uncomment if you want to reset the selection after spawning
                    }
                }
                GUILayout.EndVertical();
            }

            GUI.DragWindow();
        }

        private string MakeEnable(string label, bool toggle)
        {
            string status = toggle ? "<color=green>ON</color>" : "<color=red>OFF</color>";
            return $"{label} {status}";
        }
    }
}