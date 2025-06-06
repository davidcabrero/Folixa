using MySql.Data.MySqlClient;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Microcharts;
using SkiaSharp.Views.Maui;
using System.Linq;

namespace Folixa
{
    public partial class AdminPage : ContentPage
    {
        private Chart entradasChart;

        public AdminPage()
        {
            InitializeComponent();
            VerUsuarios();
            //MostrarGraficoEntradas();
        }

        private async void VerUsuarios()
        {
            var conexion = new Conexion();
            var usuarios = await conexion.ObtenerUsuariosAsync();
            if (usuarios != null)
            {
                // mostrar toda la info de todos los usuarios
                usersListView.ItemsSource = new ObservableCollection<string>(usuarios.Select(u => $"{u.User} - {u.Email} - Seguidos: {u.Seguidos} - Seguidores: {u.Seguidores}"));
            }
            else
            {
                await DisplayAlert("Información", "No hay usuarios registrados.", "OK");
            }
        }

        private async void MostrarGraficoEntradas()
        {
            var conexion = new Conexion();
            var ventas = await conexion.ObtenerEntradasVendidasPorFechaAsync(); // lista con Fecha y Cantidad

            var entries = ventas.Select(v => new ChartEntry(v.Cantidad)
            {
                Label = v.Fecha.ToString("dd/MM"),
                ValueLabel = v.Cantidad.ToString(),
                Color = SKColor.Parse("#FF7043")
            }).ToList();

            entradasChart = new LineChart
            {
                Entries = entries,
                LineMode = LineMode.Straight,
                LineSize = 4,
                PointMode = PointMode.Circle,
                PointSize = 8
            };

            EntradasChartCanvas.InvalidateSurface(); // fuerza el repintado
        }

        private void EntradasChartCanvas_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            if (entradasChart == null)
                return;

            var canvas = e.Surface.Canvas;
            canvas.Clear();
            entradasChart.DrawContent(canvas, e.Info.Width, e.Info.Height);
        }

        private async void OnDeleteDiscotecaButtonClicked(object sender, EventArgs e)
        {

        }

        private async void OnDeleteUserButtonClicked(object sender, EventArgs e)
        {

        }

        
    }
}
