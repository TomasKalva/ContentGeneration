using ContentGeneration.Assets.UI.Model;

public class UseItem : AnimatedAct
{
    public Inventory Inventory { private get; set; }

    public override void OnStart(Agent agent)
    {
        PlayAnimation(agent);
    }

    public override void EndAct(Agent agent)
    {
        Inventory.UseItem();
    }
}