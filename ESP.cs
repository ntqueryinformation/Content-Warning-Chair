using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ExampleAssembly {
    class ESP : MonoBehaviour {
        public static bool playerBox = true;
        public static bool monsterBox = true;
        public static bool playerName = true;
        public static bool crosshair = true;
        public static bool item = true;
        public static bool vehicle = true;

        private static bool chamsEnabled = false;
        private static Dictionary<Renderer, Material[]> originalMaterialsDict = new Dictionary<Renderer, Material[]>();

        private static readonly float crosshairScale = 7f;
        private static readonly float lineThickness = 1.75f;

        private static Material chamsMaterial;

        public static Camera mainCam;

        public void Start() {
            chamsMaterial = new Material(Shader.Find("Hidden/Internal-Colored"))
            {
                hideFlags = HideFlags.DontSaveInEditor | HideFlags.HideInHierarchy
            };

            chamsMaterial.SetInt("_SrcBlend", 5);
            chamsMaterial.SetInt("_DstBlend", 10);
            chamsMaterial.SetInt("_Cull", 0);
            chamsMaterial.SetInt("_ZTest", 8); // 8 = see through walls.
            chamsMaterial.SetInt("_ZWrite", 0);
            chamsMaterial.SetColor("_Color", Color.magenta);

            mainCam = Camera.main;
        }

            public static void DoChams() {
            chamsEnabled = !chamsEnabled;

            foreach (Player player in FindObjectsOfType<Player>()) {
                if (player == null) {
                    continue;
                }

                foreach (Renderer renderer in player?.gameObject?.GetComponentsInChildren<Renderer>()) {
                    if (chamsEnabled) {
                        // Store original materials before applying chams
                        if (!originalMaterialsDict.ContainsKey(renderer)) {
                            originalMaterialsDict.Add(renderer, renderer.materials);
                        }
                        renderer.material = chamsMaterial;
                    } else {
                        // Restore original materials when chams are disabled
                        if (originalMaterialsDict.ContainsKey(renderer)) {
                            renderer.materials = originalMaterialsDict[renderer];
                            originalMaterialsDict.Remove(renderer); // Clean up dictionary
                        }
                    }
                }
            }
        
                /*Highlighter h = player.GetOrAddComponent<Highlighter>();
                
                if (h) {
                    h.FlashingOff();
                    h.ConstantOnImmediate(Color.red);
                }*/
            }

        

        public void OnGUI() {
            if (Event.current.type != EventType.Repaint) {
                return;
            }

            Items();
            PlayerName();
            PlayerBox();
            Crosshair();
        }

        private static void Items()
        {
            if (!item)
            {
                return;
            }

            if (Cheat.droppedItems.Length > 0)
            {
                foreach (ItemInstance item in Cheat.droppedItems)
                {
                    if (item == null)
                    {
                        continue;
                    }

                    Vector3 w2s = mainCam.WorldToScreenPoint(item.transform.position);
                    w2s.y = Screen.height - (w2s.y + 1f);

                    if (ESPUtils.IsOnScreen(w2s))
                    {
                        ESPUtils.DrawString(w2s, item.item.displayName, Color.green, true, 12, FontStyle.BoldAndItalic, 1);
                    }
                }
            }
        }


        private static void PlayerBox()
        {
            if (!playerBox)
            {
                return;
            }

            if (Cheat.players.Length > 0)
            {
                foreach (Player player in Cheat.players)
                {
                    if (player != null && player != Player.localPlayer)
                    {
                        if (player.ai && !monsterBox)
                            continue;
                        Vector3 w2sHead = mainCam.WorldToScreenPoint(player.HeadPosition());
                        Vector3 w2sBottom = mainCam.WorldToScreenPoint(player.data.groundPos);
                        
                        float height = Mathf.Abs(w2sHead.y - w2sBottom.y);

                        if (ESPUtils.IsOnScreen(w2sHead))
                        {
                            if (!player.ai)
                            {
                                ESPUtils.CornerBox(new Vector2(w2sHead.x, Screen.height - w2sHead.y - 20f), height / 2f, height + 20f, 2f, Color.cyan, true);
                            }
                            else
                                ESPUtils.CornerBox(new Vector2(w2sHead.x, Screen.height - w2sHead.y - 20f), height / 2f, height + 20f, 2f, Color.red, true);
                        }
                    }
                }
            }
        }

        private static void PlayerName()
        {
            if (!playerName)
            {
                return;
            }

            if (Cheat.players.Length > 0)
            {
                foreach (Player player in Cheat.players)
                {
                    if (player != null && player != Player.localPlayer)
                    {
                        if (player.ai && !monsterBox)
                            continue;
                        Vector3 w2s = mainCam.WorldToScreenPoint(player.data.groundPos);
                        w2s.y = Screen.height - (w2s.y + 1f);

                        if (ESPUtils.IsOnScreen(w2s))
                        {
                            if (!player.ai)
                            ESPUtils.DrawString(w2s, player.name.Replace("(Clone)",""), Color.cyan, true, 12, FontStyle.Bold, 1);
                            else
                                ESPUtils.DrawString(w2s, player.gameObject.name.Replace("(Clone)", ""), Color.red, true, 12, FontStyle.Bold, 1);
                        }
                    }
                }
            }
        }

        private static void Crosshair() {
            if (!crosshair) {
                return;
            }

            Color32 col = new Color32(30, 144, 255, 255);

            Vector2 lineHorizontalStart = new Vector2(Screen.width / 2 - crosshairScale, Screen.height / 2);
            Vector2 lineHorizontalEnd = new Vector2(Screen.width / 2 + crosshairScale, Screen.height / 2);

            Vector2 lineVerticalStart = new Vector2(Screen.width / 2, Screen.height / 2 - crosshairScale);
            Vector2 lineVerticalEnd = new Vector2(Screen.width / 2, Screen.height / 2 + crosshairScale);

            ESPUtils.DrawLine(lineHorizontalStart, lineHorizontalEnd, col, lineThickness);
            ESPUtils.DrawLine(lineVerticalStart, lineVerticalEnd, col, lineThickness);
        }
    }
}
