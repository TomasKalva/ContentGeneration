using OurFramework.LevelDesignLanguage;
using System.Collections.Generic;
using System.Linq;
using OurFramework.Gameplay.RealWorld;

namespace OurFramework.Gameplay.Data
{
    /// <summary>
    /// State of weapon item.
    /// </summary>
    public class WeaponItem : EquipmentItem<Weapon>
    {
        public Weapon Weapon
        {
            get
            {
                if (_cachedEquipment == null)
                {
                    _cachedEquipment = EquipmentMaker();
                    _cachedEquipment.WeaponItem = this;
                }
                return _cachedEquipment;
            }
        }

        ByUser<Effect>[] BaseEffects { get; set; }
        List<ByUser<Effect>> UpgradeEffects { get; set; }

        public WeaponItem(string name, string description, GeometryMaker<Weapon> weaponMaker, IEnumerable<ByUser<Effect>> baseEffects) : base(name, description, weaponMaker)
        {
            BaseEffects = baseEffects.ToArray();
            UpgradeEffects = new List<ByUser<Effect>>();
        }

        public WeaponItem AddUpgradeEffect(ByUser<Effect> effect)
        {
            UpgradeEffects.Add(effect);
            return this;
        }

        public void DealDamage(CharacterState owner, float damageDuration)
        {
            owner.World.CreateOccurence(
                    Weapon.HitSelectorByDuration(damageDuration)(owner),
                    BaseEffects.Concat(UpgradeEffects).Select(effectByUser => effectByUser(owner)).ToArray()
                );
        }
    }
}
