using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace Folixa
{
    public partial class SocialPage : ContentPage
    {
        public ObservableCollection<Foto> Fotos { get; set; }
        public ObservableCollection<Mensaje> Mensajes { get; set; }
        public List<Seguido> seguidos;
        public ICommand BuscarUsuarioCommand { get; }
        public ICommand SeguirUsuarioCommand { get; }
        public ICommand EnviarMensajeCommand { get; }

        public SocialPage()
        {
            InitializeComponent();
            Fotos = new ObservableCollection<Foto>();
            Mensajes = new ObservableCollection<Mensaje>();
            BuscarUsuarioCommand = new Command<string>(async (username) => await BuscarUsuarioAsync(username));
            SeguirUsuarioCommand = new Command(async () => await SeguirUsuarioAsync());
            EnviarMensajeCommand = new Command<string>(async (texto) => await EnviarMensajeAsync(texto));
            BindingContext = this;
        }

        private Usuario usuarioActual;

        private async Task BuscarUsuarioAsync(string username)
        {
            var conexion = new Conexion();
            var usuario = await conexion.ObtenerDatosUsuarioAsync(username);
            if (usuario != null)
            {
                // Comprobar si le sigue o no
                seguidos = await conexion.VerSeguidosAsync(GlobalSettings.UsuarioIniciado);
                bool usuarioYaSeguido = seguidos.Any(s => s.User == usuario.User);
                if (usuarioYaSeguido)
                {
                    botonSeguir.Text = "Dejar de seguir";
                }
                else
                {
                    botonSeguir.Text = "Seguir";
                }

                // Mostrar datos del usuario
                usuarioActual = usuario;
                usuarioPerfilSection.IsVisible = true;
                chatSection.IsVisible = true;
                nombreUsuario.Text = usuario.User;
                emailUsuario.Text = usuario.Email;
                seguidosUsuario.Text = $"Seguidos: {usuario.Seguidos}";
                seguidoresUsuario.Text = $"Seguidores: {usuario.Seguidores}";
                fotoUsuario.Source = ImageSource.FromStream(() => new MemoryStream(usuario.Foto));

                // Cargar mensajes
                var mensajes = await conexion.ObtenerMensajesAsync(GlobalSettings.UsuarioIniciado, usuario.User);
                Mensajes.Clear();
                foreach (var mensaje in mensajes)
                {
                    Mensajes.Add(mensaje);
                }
            }
            else
            {
                usuarioPerfilSection.IsVisible = false;
                chatSection.IsVisible = false;
            }
        }

        private async Task SeguirUsuarioAsync()
        {
            if (botonSeguir.Text == "Dejar de seguir")
            {
                var conexion = new Conexion();
                string usuarioIniciado = GlobalSettings.UsuarioIniciado;
                bool resultado = await conexion.DejarDeSeguirAsync(usuarioIniciado, usuarioActual.User);
                if (resultado)
                {
                    int numSeguidos = await conexion.ContarSeguidosAsync(usuarioIniciado);
                    int numSeguidores = await conexion.ContarSeguidoresAsync(usuarioActual.User);
                    seguidoresUsuario.Text = $"Seguidores: {numSeguidores}";
                    seguidosUsuario.Text = $"Seguidos: {numSeguidos}";
                    bool actualizar = await conexion.ActualizarSeguidosSeguidoresAsync(usuarioIniciado, numSeguidos, numSeguidores);
                    if (!actualizar)
                    {
                        await DisplayAlert("Error", "No se pudo actualizar la información de seguidos y seguidores", "OK");
                    }
                    botonSeguir.Text = "Seguir";
                    await DisplayAlert("Éxito", "Dejaste de seguir al usuario", "OK");
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo dejar de seguir al usuario", "OK");
                }
            }
            else
            {
                var conexion = new Conexion();
                string usuarioIniciado = GlobalSettings.UsuarioIniciado;

                bool resultado = await conexion.SeguirUsuarioAsync(usuarioIniciado, usuarioActual.User);
                if (resultado)
                {
                    int numSeguidos = await conexion.ContarSeguidosAsync(usuarioIniciado);
                    int numSeguidores = await conexion.ContarSeguidoresAsync(usuarioActual.User);

                    seguidoresUsuario.Text = $"Seguidores: {numSeguidores}";
                    seguidosUsuario.Text = $"Seguidos: {numSeguidos}";

                    bool actualizar = await conexion.ActualizarSeguidosSeguidoresAsync(usuarioIniciado, numSeguidos, numSeguidores);
                    if (!actualizar)
                    {
                        await DisplayAlert("Error", "No se pudo actualizar la información de seguidos y seguidores", "OK");
                    }

                    botonSeguir.Text = "Dejar de seguir";
                    await DisplayAlert("Éxito", "Usuario seguido exitosamente", "OK");
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo seguir al usuario", "OK");
                }
            }
        }

        private async Task EnviarMensajeAsync(string texto)
        {
            if (usuarioActual == null || string.IsNullOrWhiteSpace(texto)) return;

            var mensaje = new Mensaje
            {
                Texto = texto,
                IsSentByCurrentUser = true
            };
            Mensajes.Add(mensaje);

            var conexion = new Conexion();
            bool resultado = await conexion.GuardarMensajeAsync(GlobalSettings.UsuarioIniciado, usuarioActual.User, texto);
            if (!resultado)
            {
                await DisplayAlert("Error", "No se pudo enviar el mensaje", "OK");
            }

            messageEntry.Text = string.Empty;
        }
    }

    public class Foto
    {
        public string FotoUrl { get; set; }
        public string Usuario { get; set; }
        public string Comentario { get; set; }
    }

    public class Mensaje
    {
        public string Texto { get; set; }
        public bool IsSentByCurrentUser { get; set; }
    }
}
