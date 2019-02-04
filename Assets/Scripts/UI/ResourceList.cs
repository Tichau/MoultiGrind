using Simulation.Network;

namespace UI
{
    public class ResourceList : UIList<ResourceLine>
    {
        private void Update()
        {
            this.DisplayList(GameClient.Instance.ActivePlayer.Resources, null, (def, ui) => ui.ResourceType = def.Name);
        }
    }
}
