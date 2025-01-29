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
            SeguirUsuarioCommand = new Command(async () => await SeguirUsuarioAsync());
            BindingContext = this;
        }

        private Usuario usuarioActual;

        private async Task BuscarUsuarioAsync(string username)
        {
            var conexion = new Conexion();
            var usuario = await conexion.ObtenerDatosUsuarioAsync(username);
            if (usuario != null)
            {
                usuarioActual = usuario;
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

        private async Task SeguirUsuarioAsync()
        {
            if (usuarioActual == null) return;

            var conexion = new Conexion();
            bool resultado = await conexion.SeguirUsuarioAsync(GlobalSettings.UsuarioIniciado, usuarioActual.User);
            if (resultado)
            {
                usuarioActual.Seguidores++;
                seguidoresUsuario.Text = $"Seguidores: {usuarioActual.Seguidores}";
                await DisplayAlert("Éxito", "Usuario seguido exitosamente", "OK");
            }
            else
            {
                await DisplayAlert("Error", "No se pudo seguir al usuario", "OK");
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
