using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Folixa
{
    public partial class InicioPage : ContentPage
    {
        private Conexion conexion;
        public List<Discoteca> discotecas;
        public List<Discoteca> discotecasFiltradas;
        public ICommand DiscotecaSelectedCommand { get; }

        public InicioPage()
        {
            InitializeComponent();
            conexion = new Conexion();
            CargarDiscotecas();
            searchBar.TextChanged += buscarDiscoteca;
            DiscotecaSelectedCommand = new Command<Discoteca>(OnDiscotecaSelected);
            BindingContext = this;
        }

        private async void CargarDiscotecas()
        {
            discotecas = await conexion.ObtenerDiscotecasAsync();
            foreach (var discoteca in discotecas)
            {
                discoteca.Estrellas = GenerarEstrellas(discoteca.Valoracion);
            }
            discotecasFiltradas = new List<Discoteca>(discotecas);
            discotecasCollectionView.ItemsSource = discotecasFiltradas;
        }

        private List<ImageSource> GenerarEstrellas(string valoracion)
        {
            int valoracionNumerica = int.Parse(valoracion);
            var estrellas = new List<ImageSource>();
            for (int i = 0; i < 5; i++)
            {
                if (i < valoracionNumerica)
                {
                    estrellas.Add(ImageSource.FromFile("images/star_filled.png"));
                }
                else
                {
                    estrellas.Add(ImageSource.FromFile("images/star_empty.png"));
                }
            }
            return estrellas;
        }

        private void buscarDiscoteca(object sender, TextChangedEventArgs e)
        {
            string textoBusqueda = searchBar.Text.ToLower();
            discotecasFiltradas = discotecas.Where(d => d.Nombre.ToLower().Contains(textoBusqueda)).ToList();
            discotecasCollectionView.ItemsSource = discotecasFiltradas;
        }

        private void OnDiscotecaSelected(Discoteca selectedDiscoteca)
        {
            if (selectedDiscoteca != null)
            {
                // Actualizar la UI con la información de la discoteca seleccionada
                nombreDiscoteca.Text = selectedDiscoteca.Nombre;
                imagenDiscoteca.Source = ImageSource.FromStream(() => new MemoryStream(selectedDiscoteca.Imagen));
                ubicacionDiscoteca.Text = selectedDiscoteca.Ubicacion;
                descripcionDiscoteca.Text = selectedDiscoteca.Descripcion;

                // Mostrar la sección de información de la discoteca
                busquedaDiscotecasSection.IsVisible = false;
                DiscotecaSection.IsVisible = true;
            }
        }

        private void OnVolverButtonClicked(object sender, EventArgs e)
        {
            busquedaDiscotecasSection.IsVisible = true;
            DiscotecaSection.IsVisible = false;
        }
    }
}
