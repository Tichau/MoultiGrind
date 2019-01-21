namespace Game
{
    public class Factory
    {
        public RecipeDefinition Definition;

        public Number Productivity;
        public int Count;

        public Factory(RecipeDefinition definition)
        {
            this.Definition = definition;
        }

        public override string ToString()
        {
            string name = $"{this.Definition.name} ({this.Count})";

            if (this.Productivity < new Number(1))
            {
                float productivity = (float) this.Productivity;
                return $"{name} ~ {productivity:P0}";
            }

            return name;
        }
    }
}