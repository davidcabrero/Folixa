namespace Folixa
{
    public partial class PerfilPage : ContentPage
    {
        private Conexion _conexion;

        public PerfilPage()
        {
            InitializeComponent();
            _conexion = new Conexion();
            CargarDatosUsuario(GlobalSettings.UsuarioIniciado);
        }

        private async void CargarDatosUsuario(string username)
        {
            Usuario usuario = await _conexion.ObtenerDatosUsuarioAsync(username);
            if (usuario != null)
            {
                user.Text = usuario.User;
                email.Text = usuario.Email;
                seguidos.Text = usuario.Seguidos.ToString();
                seguidores.Text = usuario.Seguidores.ToString();
                fotoPerfil.Source = ImageSource.FromStream(() => new MemoryStream(usuario.Foto));
            }
            else
            {
                await DisplayAlert("Error", "No se pudieron cargar los datos del usuario.", "OK");
            }
        }
    }
}
