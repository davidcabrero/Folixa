using System.Collections.ObjectModel;
using System.Windows.Input;
using MySql.Data.MySqlClient;

namespace Folixa
{
    public partial class SocialPage : ContentPage
    {
        public ObservableCollection<Foto> Fotos { get; set; }
        public ICommand BuscarUsuarioCommand { get; }
        public ICommand SeguirUsuarioCommand { get; }

        public SocialPage()
        {
            InitializeComponent();
            Fotos = new ObservableCollection<Foto>();
            BuscarUsuarioCommand = new Command<string>(async (username) => await BuscarUsuarioAsync(username));
            SeguirUsuarioCommand = new Command<Usuario>(async (usuario) => await SeguirUsuarioAsync(usuario));
            BindingContext = this;
        }

        private async Task BuscarUsuarioAsync(string username)
        {
            var conexion = new Conexion();
            var usuario = await conexion.ObtenerDatosUsuarioAsync(username);
            if (usuario != null)
            {
                usuarioPerfilSection.IsVisible = true;
                nombreUsuario.Text = usuario.User;
                emailUsuario.Text = usuario.Email;
                seguidosUsuario.Text = $"Seguidos: {usuario.Seguidos}";
                seguidoresUsuario.Text = $"Seguidores: {usuario.Seguidores}";
                fotoUsuario.Source = ImageSource.FromStream(() => new MemoryStream(usuario.Foto));
            }
            else
            {
                usuarioPerfilSection.IsVisible = false;
            }
        }

        private async Task SeguirUsuarioAsync(Usuario usuario)
        {
            var conexion = new Conexion();
            bool resultado = await conexion.SeguirUsuarioAsync(GlobalSettings.UsuarioIniciado, usuario.User);
            if (resultado)
            {
                usuario.Seguidores++;
                seguidoresUsuario.Text = $"Seguidores: {usuario.Seguidores}";
            }
        }
    }

    public class Foto
    {
        public string FotoUrl { get; set; }
        public string Usuario { get; set; }
        public string Comentario { get; set; }
    }
}
