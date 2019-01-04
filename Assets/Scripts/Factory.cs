using DefaultNamespace;

using UnityEngine;

public class Factory
{
    public string Name;
    public Resource[] Inputs;
    public Resource[] Outputs;

    public float Productivity;
    public int Count;

    public Factory(FactoryDefinition definition)
    {
        this.Name = definition.Name;
        this.Inputs = definition.Inputs;
        this.Outputs = definition.Outputs;
    }

    public void ComputeOutput(Player player)
    {
        this.Productivity = 1f;
        foreach (var resource in this.Inputs)
        {
            float ratio = Mathf.Min(resource.Amount, player.Resources[resource.Name]) / resource.Amount;
            this.Productivity = Mathf.Min(this.Productivity, ratio);
        }

        foreach (var resource in this.Inputs)
        {
            player.Resources[resource.Name] = Mathf.Max(player.Resources[resource.Name] - (resource.Amount * this.Productivity * this.Count), 0f);
        }

        foreach (var resource in this.Outputs)
        {
            player.Resources[resource.Name] += resource.Amount * this.Productivity * this.Count;
        }
    }

    public override string ToString()
    {
        string name = $"{this.Name} ({this.Count})";

        if (this.Productivity < 1f)
        {
            return $"{name} ~ {this.Productivity}";
        }

        return name;
    }
}