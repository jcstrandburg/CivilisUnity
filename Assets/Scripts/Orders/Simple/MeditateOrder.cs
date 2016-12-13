/// <summary>
/// Order to generate spirit
/// </summary>
public class MeditateOrder : BaseOrder {
    public MeditateOrder(ActorController a, NeolithicObject target) : base(a) {
    }

    public override void DoStep() {
        actor.GameController.Spirit += 0.03f;
    }
}
