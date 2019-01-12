public class CraftTask
{
    public readonly RecipeDefinition Definition;

    public Number TimeSpent;

    public CraftTask(RecipeDefinition definition)
    {
        this.Definition = definition;
    }

    public Number Progress => this.TimeSpent / this.Definition.Duration;

    public override string ToString()
    {
        return $"{this.Definition.Name} ({(float)this.Progress:P0})";
    }
}
