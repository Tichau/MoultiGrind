public class Factory
{
    public FactoryDefinition Definition;

    public Number Productivity;
    public int Count;

    public Factory(FactoryDefinition definition)
    {
        this.Definition = definition;
    }

    public override string ToString()
    {
        string name = $"{this.Definition.Name} ({this.Count})";

        if (this.Productivity < new Number(1))
        {
            float productivity = (float)this.Productivity;
            return $"{name} ~ {productivity:P0}";
        }

        return name;
    }
}