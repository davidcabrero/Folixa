namespace Folixa
{
    public partial class InicioPage : ContentPage
    {
        private Conexion conexion;
        public List<Discoteca> discotecas;
        public List<Discoteca> discotecasFiltradas;

        public InicioPage()
        {
            InitializeComponent();
            conexion = new Conexion();
            CargarDiscotecas();
            searchBar.TextChanged += buscarDiscoteca;
        }

        private async void CargarDiscotecas()
        {
            discotecas = await conexion.ObtenerDiscotecasAsync();
            discotecasFiltradas = new List<Discoteca>(discotecas);
            // Método para mostrar estrellas según la valoracion de la discoteca en la lista
            foreach (Discoteca discoteca in discotecasFiltradas)
            {
                discoteca.Estrellas = new List<ImageSource>();
                for (int i = 0; i < 5; i++)
                {
                    ImageSource estrella;
                    if (i < int.Parse(discoteca.Valoracion))
                    {
                        estrella = ImageSource.FromFile("Images/star_filled.png");
                    }
                    else
                    {
                        estrella = ImageSource.FromFile("Images/star_empty.png");
                    }
                    discoteca.Estrellas.Add(estrella);
                }
            }
            discotecasCollectionView.ItemsSource = discotecasFiltradas;
        }

        private void buscarDiscoteca(object sender, TextChangedEventArgs e)
        {
            string textoBusqueda = searchBar.Text.ToLower();
            discotecasFiltradas = discotecas.Where(d => d.Nombre.ToLower().Contains(textoBusqueda)).ToList();
            discotecasCollectionView.ItemsSource = discotecasFiltradas;
        }
    }
}
