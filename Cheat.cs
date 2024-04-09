using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;
using Zorro.Core;

namespace ExampleAssembly
{
    public class Cheat : MonoBehaviour
    {
        private int mainWID = 1024;
        private Rect mainWRect = new Rect(5f, 5f, 300f, 150f);

        private bool magicBullet;
        private bool godmode;
        private bool infBattery;
        private bool infBatteryEveryone;
        private bool drawMenu = true;
        private bool stamina = false;
        private bool oxygen = false;
        private bool superJump;
        private bool rainbowVisor;
        private bool blinkingFace;
        private float lastCacheTime = Time.time + 5f;
        private float lastItemCache = Time.time + 1f;

        public static Player[] players;
        public static PlayerController controller;
        public static Monster[] monsters;
        public static ItemInstance[] droppedItems;
        private int selectedItemIndex = -1;
        private int selectedItemIndex2 = -1;
        private Rect windowRect = new Rect((Screen.width - 250) / 2, (Screen.height - 500) / 2, 250, 500); // Position the window in the center
        private bool showItemSpawnerWindow = false;
        private Vector2 scrollPosition;
        private Vector2 scrollPosition2;

        private string[] enemyNames = new string[] { "Angler", "AnglerMimic", "BarnacleBall", "BigSlap", "Bombs", "Dog", "Ear", "EyeGuy", "Flicker", "Ghost", "Jello", "Knifo", "Larva", "MimicInfiltrator", "Mouthe", "Slurper", "Snatcho", "Spider", "Snail", "ToolkitBoy", "Toolkit_Fan", "Toolkit_Hammer", "Toolkit_Iron", "Toolkit_Vaccum", "Toolkit_Wisk", "Weeping", "Zombe" }; // Add the rest of your enemy names here
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
                    {
                        // Player.localPlayer.refs.visor.ApplyVisorColor(Color.red);
                        Player.localPlayer.data.health = 100;
                    }
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
               
            }

            if (Time.time >= lastCacheTime)
            {
                lastCacheTime = Time.time + 5f;

                players = FindObjectsOfType<Player>();
                //controller = base.GetComponent<PlayerController>();
                // vehicles = FindObjectsOfType<Car>();

                ESP.mainCam = Camera.main;
            }

            if (Time.time >= lastItemCache)
            {
                lastItemCache = Time.time + 1f;

                droppedItems = FindObjectsOfType<ItemInstance>();
            }
        }

        public void OnGUI()
        {
            if (drawMenu)
            {
                mainWRect = GUILayout.Window(mainWID, mainWRect, MainWindow, "Main");
            }
        }

        private void MainWindow(int id)
        {
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

                GUILayout.BeginHorizontal();
                {
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            if (Player.localPlayer != null)
            {
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Revive yourself"))
                    {
                        Player.localPlayer.CallRevive();
                    }
                    if (GUILayout.Button("Spawn Mimic"))
                    {
                        Monster.SpawnMonster("AnglerMimic");


                        //Ear
                        //Zombe
                        //Spider
                        //Snatcho
                        //(Clone)
                    }
                    if (GUILayout.Button("Spawn Player"))
                    {
                        Monster.SpawnMonster("Player");
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Revive All Players"))
                    {
                        if (Cheat.players.Length > 0)
                        {
                            foreach (Player player in Cheat.players)
                            {
                                if (player != null && player != Player.localPlayer)
                                {
                                    player.CallRevive();
                                    player.data.health = 100;
                                }
                            }
                        }
                    }
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.Space(20f);

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
                //  ESP.playerName = GUILayout.Toggle(ESP.playerName, "Player Name");
            }
            GUILayout.EndVertical();
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
            GUI.DragWindow();
        }

        private string MakeEnable(string label, bool toggle)
        {
            string status = toggle ? "<color=green>ON</color>" : "<color=red>OFF</color>";
            return $"{label} {status}";
        }
    }
}
