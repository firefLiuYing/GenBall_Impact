using GenBall.UI;
using GenBall.Utils.EntityCreator;

namespace GenBall
{
    public partial class GameEntry
    {
        private void RegisterUIs()
        {
            var uiCreator = GetModule<EntityCreator<IUserInterface>>();
            
            uiCreator.AddPrefab<MainHud>("Assets/AssetBundles/UI/MainHud/Form/MainHud.prefab");
            uiCreator.AddPrefab<AccessoryForm>("Assets/AssetBundles/UI/MainHud/Form/AccessoryForm.prefab");
        }
    }
}