namespace Farmacia.Models;

public class Medicamento
{
    public int Id { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Categoria { get; set; } = "";
    public string Laboratorio { get; set; } = "";
    public string Presentacion { get; set; } = "";
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public bool RequiereReceta { get; set; }
    public string FechaVencimiento { get; set; } = "";
    public decimal Descuento { get; set; }
    public string Imagen { get; set; } = "";
    public string Descripcion { get; set; } = "";
}