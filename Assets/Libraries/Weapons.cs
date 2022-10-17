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

    [SerializeField]
    Weapon mayanSword;

    [SerializeField]
    Weapon scythe;

    [SerializeField]
    Weapon mace;

    [SerializeField]
    Weapon katana;

    Weapon CreateWeapon(Weapon prefab)
    {
        var weapon = Instantiate(prefab);
        return weapon.SetHitSelector(Selectors.WeaponSelector(weapon.Detector));
        /*
        owner.World.AddOccurence(
            new Assets.ShapeGrammarGenerator.LevelDesignLanguage.Factions.Occurence(
                Weapon.GetSelector(),
                ))*/
    }

    public Weapon MayanSword()
    {
        var sword = CreateWeapon(mayanSword);
        //sword.Damage = 12f;
        return sword;
    }

    public Weapon MayanKnife()
    {
        var knife = CreateWeapon(mayanKnife);
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
        var club = CreateWeapon(sculptureClub);
        //club.Damage = 22f;
        return club;
    }
    public Weapon Scythe()
    {
        var scythe = CreateWeapon(this.scythe);
        //scythe.Damage = 30f;
        return scythe;
    }

    public Weapon Mace()
    {
        var mace = CreateWeapon(this.mace);
        //mace.Damage = 19f;
        return mace;
    }

    public Weapon Katana()
    {
        var katana = CreateWeapon(this.katana);
        //katana.Damage = 19f;
        return katana;
    }
}
