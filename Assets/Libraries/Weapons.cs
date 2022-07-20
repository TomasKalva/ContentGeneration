using Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Factions;
using ContentGeneration.Assets.UI.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Weapons : ScriptableObject
{
#if UNITY_EDITOR
    [MenuItem("Assets/Create/Weapons")]
    public static void CreateMyAsset()
    {
        Weapons asset = ScriptableObject.CreateInstance<Weapons>();

        string name = UnityEditor.AssetDatabase.GenerateUniqueAssetPath("Assets/Weapons.asset");
        AssetDatabase.CreateAsset(asset, name);
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();

        Selection.activeObject = asset;
    }
#endif

    SelectorLibrary Selectors;

    public void SetLibraries(SelectorLibrary selectors)
    {
        Selectors = selectors;
    }

    [SerializeField]
    Weapon sculptureClub;

    [SerializeField]
    Weapon mayanKnife;

    /*
    [SerializeField]
    Weapon fireball;
    */

    [SerializeField]
    Weapon mayanSword;

    [SerializeField]
    Weapon scythe;

    [SerializeField]
    Weapon mace;

    [SerializeField]
    Weapon katana;

    Weapon AddSelector(Weapon weapon)
    {
        return weapon.SetHitSelector(Selectors.WeaponSelector(weapon.Detector));
        /*
        owner.World.AddOccurence(
            new Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Factions.Occurence(
                Weapon.GetSelector(),
                ))*/
    }

    public Weapon MayanSword()
    {
        var sword = AddSelector(Instantiate(mayanSword));
        //sword.Damage = 12f;
        return sword;
    }

    public Weapon MayanKnife()
    {
        var knife = AddSelector(Instantiate(mayanKnife));
        //knife.Damage = 5f;
        return knife;
    }

    /*
    public Weapon Fireball()
    {
        var fb = Instantiate(fireball);
        fb.Damage = 5f;
        return fb;
    }*/

    public Weapon SculptureClub()
    {
        var club = AddSelector(Instantiate(sculptureClub));
        //club.Damage = 22f;
        return club;
    }
    public Weapon Scythe()
    {
        var scythe = AddSelector(Instantiate(this.scythe));
        //scythe.Damage = 30f;
        return scythe;
    }

    public Weapon Mace()
    {
        var mace = AddSelector(Instantiate(this.mace));
        //mace.Damage = 19f;
        return mace;
    }

    public Weapon Katana()
    {
        var katana = AddSelector(Instantiate(this.katana));
        //katana.Damage = 19f;
        return katana;
    }
}
