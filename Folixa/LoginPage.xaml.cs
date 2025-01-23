
namespace Folixa
{
    public partial class LoginPage : ContentPage
    {
        private Conexion conexion;

        public LoginPage()
        {
            InitializeComponent();
            conexion = new Conexion();
        }

        private async void OnLoginButtonClicked(object sender, EventArgs e)
        {
            string username = usernameEntry.Text;
            string password = passwordEntry.Text;

            bool isAuthenticated = await Task.Run(() => conexion.iniciarSesion(username, password));

            if (isAuthenticated)
            {
                Application.Current.MainPage = new AppShell();
            }
            else
            {
                messageLabel.Text = "Usuario o contraseña incorrectos";
                messageLabel.IsVisible = true;
            }
        }
    }
}
