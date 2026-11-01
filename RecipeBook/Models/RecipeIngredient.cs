namespace RecipeBook.Models;

public class RecipeIngredient
{
    public string Name { get; set; } = string.Empty;
    public double Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;

    public RecipeIngredient()
    {
    }

    public RecipeIngredient(string name, double quantity, string unit)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Quantity = quantity;
        Unit = unit ?? throw new ArgumentNullException(nameof(unit));
    }

    public override string ToString() => $"{Name} - {Quantity} {Unit}".Trim();
}
