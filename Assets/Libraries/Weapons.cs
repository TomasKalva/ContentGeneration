using OurFramework.Gameplay.RealWorld;
using UnityEditor;
using UnityEngine;

namespace OurFramework.Libraries
{
    /// <summary>
    /// Defines weapons.
    /// </summary>
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

        [SerializeField]
        Weapon lightMace;

        Weapon CreateWeapon(Weapon prefab)
        {
            var weapon = Instantiate(prefab);
            return weapon.SetHitSelector(Selectors.WeaponSelector(weapon.Detector));
        }

        public Weapon MayanSword()
        {
            var sword = CreateWeapon(mayanSword);
            return sword;
        }

        public Weapon MayanKnife()
        {
            var knife = CreateWeapon(mayanKnife);
            return knife;
        }

        public Weapon SculptureClub()
        {
            var club = CreateWeapon(sculptureClub);
            return club;
        }
        public Weapon Scythe()
        {
            var scythe = CreateWeapon(this.scythe);
            return scythe;
        }

        public Weapon Mace()
        {
            var mace = CreateWeapon(this.mace);
            return mace;
        }

        public Weapon Katana()
        {
            var katana = CreateWeapon(this.katana);
            return katana;
        }

        public Weapon LightMace()
        {
            var mace = CreateWeapon(this.lightMace);
            return mace;
        }
    }
}
