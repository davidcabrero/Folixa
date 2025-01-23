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
    }
}
