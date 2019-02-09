using Framework;

namespace Simulation
{
    public class Factory
    {
        public Simulation.Data.RecipeDefinition Definition;

        public Number Productivity;
        public int Count;

        public Factory(Simulation.Data.RecipeDefinition definition)
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