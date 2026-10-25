
using Farmacia.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();
app.UseCors();

// 2. Servir el frontend desde wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

// 3. Datos simulados en memoria
Medicamento M(int id, string cod, string nom, string cat, string lab, string pres, decimal precio, int stock,
              bool receta, string vence, decimal desc, string detalle) => new()
{
    Id = id, Codigo = cod, Nombre = nom, Categoria = cat, Laboratorio = lab, Presentacion = pres, Precio = precio,
    Stock = stock, RequiereReceta = receta, FechaVencimiento = vence, Descuento = desc, Descripcion = detalle
};

var medicamentos = new List<Medicamento>
{
    M(1, "MED001", "Paracetamol 500 mg", "Analgésicos", "Laboratorios Andes", "Caja x 100 tabletas", 18.50m, 120, false, "2027-08-30", 0, "Alivia el dolor leve a moderado y reduce la fiebre."),
    M(2, "MED002", "Ibuprofeno 400 mg", "Antiinflamatorios", "Medicor", "Caja x 20 tabletas", 12.90m, 85, false, "2027-05-12", 10, "Antiinflamatorio para dolores musculares y de cabeza."),
    M(3, "MED003", "Amoxicilina 500 mg", "Antibióticos", "BioFarma", "Caja x 21 cápsulas", 24.90m, 40, true, "2027-02-28", 0, "Antibiótico de amplio espectro. Venta con receta médica."),
    M(4, "MED004", "Loratadina 10 mg", "Antialérgicos", "Medicor", "Caja x 10 tabletas", 8.50m, 60, false, "2027-11-20", 5, "Alivia los síntomas de la alergia."),
    M(5, "MED005", "Omeprazol 20 mg", "Gastrointestinales", "Laboratorios Andes", "Caja x 14 cápsulas", 15.80m, 8, false, "2026-12-10", 0, "Reduce la acidez estomacal."),
    M(6, "MED006", "Vitamina C 1000 mg", "Vitaminas", "BioFarma", "Frasco x 30 tabletas", 29.90m, 55, false, "2028-01-15", 15, "Refuerza las defensas del organismo."),
    M(7, "MED007", "Alcohol medicinal 70°", "Botiquín", "Laboratorios Andes", "Frasco 250 ml", 6.50m, 150, false, "2028-06-01", 0, "Antiséptico de uso externo."),
    M(8, "MED008", "Jarabe para la tos", "Resfrío", "Medicor", "Frasco 120 ml", 14.50m, 5, false, "2026-09-15", 0, "Calma la tos seca y la irritación de garganta.")
};
var candado = new object();

// 4. Endpoints de la API (CRUD)
app.MapGet("/api/estado", () => Results.Ok(new { mensaje = "API FARMACIA FUNCIONANDO", version = "1.0", total = medicamentos.Count }));

var api = app.MapGroup("/api/medicamentos");

// Listar
api.MapGet("/", () =>
{
    lock (candado) return Results.Ok(medicamentos.ToList());
});

// Obtener por id
api.MapGet("/{id:int}", (int id) =>
{
    lock (candado)
    {
        var m = medicamentos.FirstOrDefault(x => x.Id == id);
        return m is null ? Results.NotFound(new { mensaje = $"No existe el medicamento {id}." }) : Results.Ok(m);
    }
});

// Crear
api.MapPost("/", (Medicamento nuevo) =>
{
    var error = Validar(nuevo);
    if (error is not null) return Results.BadRequest(new { mensaje = error });

    lock (candado)
    {
        if (medicamentos.Any(x => x.Codigo.Equals(nuevo.Codigo, StringComparison.OrdinalIgnoreCase)))
            return Results.BadRequest(new { mensaje = $"Ya existe un medicamento con el código {nuevo.Codigo}." });

        nuevo.Id = medicamentos.Count == 0 ? 1 : medicamentos.Max(x => x.Id) + 1;
        medicamentos.Add(nuevo);
        return Results.Created($"/api/medicamentos/{nuevo.Id}", nuevo);
    }
});

// Editar
api.MapPut("/{id:int}", (int id, Medicamento datos) =>
{
    var error = Validar(datos);
    if (error is not null) return Results.BadRequest(new { mensaje = error });

    lock (candado)
    {
        var m = medicamentos.FirstOrDefault(x => x.Id == id);
        if (m is null) return Results.NotFound(new { mensaje = $"No existe el medicamento {id}." });

        if (medicamentos.Any(x => x.Id != id && x.Codigo.Equals(datos.Codigo, StringComparison.OrdinalIgnoreCase)))
            return Results.BadRequest(new { mensaje = $"Ya existe otro medicamento con el código {datos.Codigo}." });

        m.Codigo = datos.Codigo;
        m.Nombre = datos.Nombre;
        m.Categoria = datos.Categoria;
        m.Laboratorio = datos.Laboratorio;
        m.Presentacion = datos.Presentacion;
        m.Precio = datos.Precio;
        m.Stock = datos.Stock;
        m.RequiereReceta = datos.RequiereReceta;
        m.FechaVencimiento = datos.FechaVencimiento;
        m.Descuento = datos.Descuento;
        m.Imagen = datos.Imagen;
        m.Descripcion = datos.Descripcion;
        return Results.Ok(m);
    }
});

// Eliminar
api.MapDelete("/{id:int}", (int id) =>
{
    lock (candado)
    {
        var m = medicamentos.FirstOrDefault(x => x.Id == id);
        if (m is null) return Results.NotFound(new { mensaje = $"No existe el medicamento {id}." });
        medicamentos.Remove(m);
        return Results.NoContent();
    }
});

// 5. Puerto asignado por Render o puerto por defecto local
var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
app.Run($"http://0.0.0.0:{port}");

static string? Validar(Medicamento m)
{
    if (string.IsNullOrWhiteSpace(m.Codigo)) return "El código es obligatorio.";
    if (string.IsNullOrWhiteSpace(m.Nombre)) return "El nombre es obligatorio.";
    if (m.Precio < 0) return "El precio no puede ser negativo.";
    if (m.Stock < 0) return "El stock no puede ser negativo.";
    if (m.Descuento < 0 || m.Descuento > 100) return "El descuento debe estar entre 0 y 100.";
    return null;
}