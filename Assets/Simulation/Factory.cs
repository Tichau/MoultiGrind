using System.IO;
using Framework;

namespace Simulation
{
    public class Factory : ISerializable
    {
        public Simulation.Data.RecipeDefinition Definition;

        public Number Productivity;
        public int Count;

        public Factory(Simulation.Data.RecipeDefinition definition)
        {
            this.Definition = definition;
        }

        // Used for serialization.
        public Factory()
        {
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

        public void Serialize(BinaryWriter stream)
        {
            stream.WriteReference(this.Definition);
            stream.Write(this.Productivity);
            stream.Write(this.Count);
        }

        public void Deserialize(BinaryReader stream)
        {
            this.Definition = stream.ReadReference<Data.RecipeDefinition>();
            this.Productivity = stream.ReadNumber();
            this.Count = stream.ReadInt32();
        }
    }
}