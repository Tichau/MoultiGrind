using System.IO;
using Simulation.Data;

namespace Simulation
{
    using Framework;

    public class CraftTask : ISerializable
    {
        public RecipeDefinition Definition;

        public Number TimeSpent;

        public CraftTask(Simulation.Data.RecipeDefinition definition)
        {
            this.Definition = definition;
        }

        // Used for serialization.
        public CraftTask()
        {
        }

        public Number Progress => this.TimeSpent / this.Definition.Duration;

        public override string ToString()
        {
            return $"{this.Definition.name} ({(float) this.Progress:P0})";
        }

        public void Serialize(BinaryWriter stream)
        {
            stream.WriteReference(this.Definition);
            stream.Write(this.TimeSpent);
        }

        public void Deserialize(BinaryReader stream)
        {
            this.Definition = stream.ReadReference<RecipeDefinition>();
            this.TimeSpent = stream.ReadNumber();
        }
    }
}