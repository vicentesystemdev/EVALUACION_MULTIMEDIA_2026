using UnityEngine;

namespace CampusGlitch {
/// <summary>Displays actual level progress; no decorative currencies or unavailable abilities.</summary>
public static class CGCampusHUD {
    static GUIStyle small, title, number;
    static readonly Color Ink = new Color(.024f, .035f, .08f, .96f);
    static readonly Color Amber = new Color(1, .62f, .16f);
    static readonly Color Cyan = new Color(.38f, .87f, 1);
    static void Fill(Rect rect, Color color) { var previous = GUI.color; GUI.color = color; GUI.DrawTexture(rect, Texture2D.whiteTexture); GUI.color = previous; }
    static void Card(Rect rect, Color accent) {
        Fill(rect, Ink); Fill(new Rect(rect.x, rect.y, rect.width, 2), accent);
        Fill(new Rect(rect.x, rect.yMax - 2, rect.width, 2), accent);
        Fill(new Rect(rect.x, rect.y, 2, rect.height), accent);
        Fill(new Rect(rect.xMax - 2, rect.y, 2, rect.height), accent);
    }
    static void Styles() {
        if (small != null) return;
        small = new GUIStyle(GUI.skin.label) { font=CGMenuTheme.Body, fontSize = 15, alignment = TextAnchor.MiddleLeft };
        small.normal.textColor = new Color(.82f, .88f, .96f);
        title = new GUIStyle(small) { font=CGMenuTheme.Interface, fontSize = 21, fontStyle = FontStyle.Normal };
        title.normal.textColor = Color.white;
        number = new GUIStyle(title) { fontSize = 25, alignment = TextAnchor.MiddleCenter };
    }
    public static void Draw(CGWorld world, CGHero hero, int chips, bool[] taken, bool terminalSolved, bool bossDead) {
        Styles();
        Color accent = CGStore.Data.purpleEquipped ? new Color(.83f, .33f, 1) : Amber;
        Card(new Rect(18, 18, 392, 94), accent);
        Card(new Rect(27, 27, 76, 76), new Color(.34f, .4f, .57f));
        CGGuardianArt.DrawPortrait(new Rect(29, 29, 72, 72), CGArt.Player(CGStore.Data.purpleEquipped, 0, CGStore.Data.useOriginalWolf));
        GUI.Label(new Rect(118, 26, 280, 25), "LOBO GUARDIÁN  /  " + (world.index == 9 ? "EXTRA" : "N" + (world.index + 1)), title);
        for (int i = 0; i < 3; i++) {
            Rect heart = new Rect(119 + i * 32, 57, 27, 26);
            var icon = CGArt.Get("01_Nuevos_Modulares/Global_UI/" + (i < hero.hearts ? "HeartFull" : "HeartEmpty"));
            Color old = GUI.color;
            if (icon != null) CGGuardianArt.DrawFit(heart, icon); else GUI.Label(heart, "♥", number);
            GUI.color = old;
        }
        GUI.Label(new Rect(229, 60, 170, 23), "VIDA  " + Mathf.Max(0, hero.hearts) + "/3", small);
        Fill(new Rect(119, 93, 266, 4), new Color(.18f, .22f, .33f));
        Fill(new Rect(119, 93, 266 * Mathf.Clamp01(hero.hearts / 3f), 4), accent);

        Card(new Rect(427, 18, 434, 94), Cyan);
        GUI.Label(new Rect(442, 24, 405, 27), CGCampusDepthStage.AreaAt(world.index,hero.transform.position.x), title);
        for (int i = 0; i < 3; i++) {
            Card(new Rect(443 + 34 * i, 65, 25, 25), taken[i] ? Amber : new Color(.28f, .34f, .48f));
            var fragment=CGArt.Get("01_Nuevos_Modulares/Global_UI/CoinIcon");
            if(taken[i]&&fragment!=null)CGGuardianArt.DrawFit(new Rect(447+34*i,68,17,19),fragment);
            else GUI.Label(new Rect(443 + 34 * i, 65, 25, 25), "·", number);
        }
        GUI.Label(new Rect(550, 65, 79, 25), chips + "/3 DATOS", small);
        GUI.Label(new Rect(649, 65, 204, 25), terminalSolved ? "TERMINAL RESTAURADA" : "TERMINAL PENDIENTE", small);

        Card(new Rect(878, 18, 384, 94), accent);
        GUI.Label(new Rect(893, 24, 350, 25), "RUTA DEL CAMPUS", title);
        // Position and checkpoint come from the live world, rather than a decorative minimap.
        Fill(new Rect(896, 67, 346, 4), new Color(.3f, .37f, .51f));
        float checkpoint = 896 + 346 * 35 / 73f;
        Fill(new Rect(checkpoint - 3, 60, 6, 18), world.checkpoint ? Cyan : Color.gray);
        Fill(new Rect(896 + 346 * Mathf.Clamp01(hero.transform.position.x / 73f) - 4, 59, 8, 20), accent);
        GUI.Label(new Rect(893, 82, 350, 21), "E / Y  INTERACTUAR     ESC / START  PAUSA", small);
        int skill=CGStore.Data.equippedAbility;float remaining=hero.Skills.Remaining(skill,Time.time);
        Card(new Rect(18,625,313,72),CGAbilities.ColorFor(CGStore.Data.purpleEquipped?2:0));
        GUI.DrawTexture(new Rect(28,635,48,48),CGAbilityArt.Icon(skill));
        GUI.Label(new Rect(86,632,235,26),CGAbilities.Names[skill],title);
        GUI.Label(new Rect(86,661,235,23),remaining>0?"RECARGA  "+remaining.ToString("0.0")+" s":CGStore.Data.actionKeys[6]+" / "+CGStore.Data.padBindings[6]+"  · LISTA",small);
        if (world.boss != null && !bossDead) {
            Card(new Rect(425, 126, 434, 40), new Color(1, .27f, .34f));
            GUI.Label(new Rect(440, 133, 405, 26), "JEFE: " + world.boss.kind + "  /  VIDA " + world.boss.hp, title);
        }
    }
}
}
