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
        public List<Comentario> comentarios;
        public List<string> Estrellas { get; set; } = new List<string>();
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
            discotecasFiltradas = new List<Discoteca>(discotecas);
            discotecasCollectionView.ItemsSource = discotecasFiltradas;
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

                // Generar las estrellas según la valoración
                selectedDiscoteca.Estrellas = GenerarEstrellas(selectedDiscoteca.Valoracion);

                // Asignar las estrellas a la vista de detalles
                estrellasDiscoteca.Children.Clear();
                foreach (var estrella in selectedDiscoteca.Estrellas)
                {
                    estrellasDiscoteca.Children.Add(new Image { Source = estrella });
                }

                // Mostrar la sección de información de la discoteca
                busquedaDiscotecasSection.IsVisible = false;
                DiscotecaSection.IsVisible = true;

                // Asignar el valor del picker a la valoración actual
                valoracionPicker.SelectedIndex = int.Parse(selectedDiscoteca.Valoracion);
            }
        }

        private List<ImageSource> GenerarEstrellas(string valoracion)
        {
            int valoracionNumerica = int.Parse(valoracion);
            var estrellas = new List<ImageSource>();
            for (int i = 0; i < 5; i++)
            {
                if (i < valoracionNumerica)
                {
                    estrellas.Add(ImageSource.FromFile("star_filled.png"));
                }
                else
                {
                    estrellas.Add(ImageSource.FromFile("star_empty.png"));
                }
            }
            return estrellas;
        }

        private async void OnValoracionPickerSelectedIndexChanged(object sender, EventArgs e)
        {
            if (DiscotecaSection.IsVisible)
            {
                var selectedDiscoteca = discotecas.FirstOrDefault(d => d.Nombre == nombreDiscoteca.Text);
                if (selectedDiscoteca != null)
                {
                    // Calcular la nueva valoración como la media entre la valoración existente y la nueva
                    int valoracionExistente = int.Parse(selectedDiscoteca.Valoracion);
                    int nuevaValoracion = (valoracionExistente + valoracionPicker.SelectedIndex) / 2;
                    selectedDiscoteca.Valoracion = nuevaValoracion.ToString();
                    selectedDiscoteca.Estrellas = GenerarEstrellas(selectedDiscoteca.Valoracion);

                    // Actualizar las estrellas en la UI
                    estrellasDiscoteca.Children.Clear();
                    foreach (var estrella in selectedDiscoteca.Estrellas)
                    {
                        estrellasDiscoteca.Children.Add(new Image { Source = estrella });
                    }

                    // Actualizar la valoración en la base de datos
                    await conexion.ActualizarValoracionDiscotecaAsync(selectedDiscoteca);
                }
            }
        }

        private async void OnVerComentariosButtonClicked(object sender, EventArgs e)
        {
            var selectedDiscoteca = discotecas.FirstOrDefault(d => d.Nombre == nombreDiscoteca.Text);

            // extrae el idDiscoteca de la discoteca seleccionada
            string idDiscoteca = selectedDiscoteca.idDiscoteca;
            // convierte el string a int
            int idDiscotecaInt = int.Parse(idDiscoteca);

            if (selectedDiscoteca != null)
            {
                comentarios = await conexion.ObtenerComentariosAsync(idDiscotecaInt);
                comentariosCollectionView.ItemsSource = comentarios;

                // Mostrar la sección de comentarios
                DiscotecaSection.IsVisible = false;
                ComentariosSection.IsVisible = true;
            }
        }

        private async void OnEnviarComentarioButtonClicked(object sender, EventArgs e)
        {
            
            var selectedDiscoteca = discotecas.FirstOrDefault(d => d.Nombre == nombreDiscoteca.Text);

            // extrae el idDiscoteca de la discoteca seleccionada
            string idDiscoteca = selectedDiscoteca.idDiscoteca;
            // convierte el string a int
            int idDiscotecaInt = int.Parse(idDiscoteca);


            bool success = await conexion.AgregarComentarioAsync(nuevoComentarioEntry.Text, GlobalSettings.UsuarioIniciado, idDiscotecaInt);
            if (success)
            {
                await DisplayAlert("Éxito", "Comentario subido", "OK");
            }
            else
            {
                await DisplayAlert("Error", "No se pudo registrar el comentario", "OK");
            }
        }

        private void OnVolverComentariosButtonClicked(object sender, EventArgs e)
        {
            ComentariosSection.IsVisible = false;
            DiscotecaSection.IsVisible = true;
        }

        private void OnVolverButtonClicked(object sender, EventArgs e)
        {
            busquedaDiscotecasSection.IsVisible = true;
            DiscotecaSection.IsVisible = false;
        }

        public List<Entrada> entradas;

        private async void OnVerEntradasButtonClicked(object sender, EventArgs e)
        {
            var selectedDiscoteca = discotecas.FirstOrDefault(d => d.Nombre == nombreDiscoteca.Text);

            // extrae el idDiscoteca de la discoteca seleccionada
            string idDiscoteca = selectedDiscoteca.idDiscoteca;
            // convierte el string a int
            int idDiscotecaInt = int.Parse(idDiscoteca);

            if (selectedDiscoteca != null)
            {
                entradas = await conexion.ObtenerEntradasAsync(idDiscotecaInt);
                entradasCollectionView.ItemsSource = entradas;

                // Mostrar la sección de entradas
                DiscotecaSection.IsVisible = false;
                verEntradasSection.IsVisible = true;
            }
        }

        private void OnVolverEntradasButtonClicked(object sender, EventArgs e)
        {
            verEntradasSection.IsVisible = false;
            DiscotecaSection.IsVisible = true;
        }

    }
}
