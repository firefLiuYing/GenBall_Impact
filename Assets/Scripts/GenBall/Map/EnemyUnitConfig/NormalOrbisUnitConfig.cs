using GenBall.Map;
using UnityEngine;

namespace GenBall.Map.EnemyUnitConfig
{
    [PlaceableCategory("Enemy", "蓝色奥比斯", 10)]
    [PlaceablePrefab("Assets/MapEditorPrefabs/Enemies/Placeholders/NormalOrbis.prefab")]
    public class NormalOrbisConfig : EnemyUnitConfigBase
    {
        public override string TypeName => "NormalOrbis";

        protected override string OnValidateConfig()
        {
            if (string.IsNullOrEmpty(TypeName))
                return "Type name is required.";
            return null;
        }
    }
}